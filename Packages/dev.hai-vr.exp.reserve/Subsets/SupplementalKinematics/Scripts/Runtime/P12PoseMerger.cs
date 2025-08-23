using System;
using Hai.Project12.DataViz.Runtime;
using Hai.Project12.HaiSystems.DataStructures;
#if RESILIENCE_IN_PROJECT
using Resilience.Kinematics.KinematicsSolver;
using Resilience.SharedObjects;
using Resilience.Utility;
#endif
using UnityEditor;
using UnityEngine;
using static UnityEngine.HumanBodyBones;

namespace Hai.Project12.SupplementalKinematics.Runtime
{
    public class P12PoseMerger : MonoBehaviour
    {
        // In the HumanBodyBones, UpperChest comes near the end. This creates a problem if we need
        // to set the position and rotation of each transform, as iterating the original enum would shift the
        // position of the bones that had been set earlier.
        internal static readonly HumanBodyBones[] HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded = {
            Hips, Spine, Chest, UpperChest, Neck, Head,

            LeftUpperLeg, LeftLowerLeg, LeftFoot, LeftToes,
            RightUpperLeg, RightLowerLeg, RightFoot, RightToes,

            LeftShoulder, LeftUpperArm, LeftLowerArm, LeftHand,
            RightShoulder, RightUpperArm, RightLowerArm, RightHand,

            LeftLittleProximal, LeftLittleIntermediate, LeftLittleDistal,
            LeftRingProximal, LeftRingIntermediate, LeftRingDistal,
            LeftMiddleProximal, LeftMiddleIntermediate, LeftMiddleDistal,
            LeftIndexProximal, LeftIndexIntermediate, LeftIndexDistal,
            LeftThumbProximal, LeftThumbIntermediate, LeftThumbDistal,

            RightLittleProximal, RightLittleIntermediate, RightLittleDistal,
            RightRingProximal, RightRingIntermediate, RightRingDistal,
            RightMiddleProximal, RightMiddleIntermediate, RightMiddleDistal,
            RightIndexProximal, RightIndexIntermediate, RightIndexDistal,
            RightThumbProximal, RightThumbIntermediate, RightThumbDistal,
        };

        [SerializeField] private Animator traditionalInput;
        /// Can be null if we're using the CopyTraditional strategy.
        [SerializeField] private P12Rig physicsRig;
        [SerializeField] private Animator visualOutput;
        [SerializeField] private P12PoseMergerStrategy strategy;
        [SerializeField] private P12QuickDataViz dataViz;

        [SerializeField] private bool runInUpdateLoop = true;

#if RESILIENCE_IN_PROJECT
        [SerializeField] private RVRIKEffectors solverEffectors;
        [SerializeField] private RVRKinematicsSolver solverOptional;
#endif

        [SerializeField] private bool _debug_readjustHighMassPhysics = true;
        [SerializeField] private AnimationCurve _debug_readjustHighMassPhysicsCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _debug_readjustHighMassPhysicsCurveClose = 0.02f;
        [SerializeField] private float _debug_readjustHighMassPhysicsCurveFar = 0.05f;
        [SerializeField] private bool _debug_drawSolverLines;
        [SerializeField] private bool _debug_skipSovler;

        // TODO: Remove this in the future, when we have a process to create an execution loop that is more tightly controlled.
        // The changes made by this component must not contaminate the networking data.
        private void Update()
        {
            if (runInUpdateLoop)
            {
                Resolve();
#if RESILIENCE_IN_PROJECT
                if (solverOptional != null && !_debug_skipSovler)
                {
                    // TODO: All invocations to GetBoneTransform should be cached.
                    var head = traditionalInput.GetBoneTransform(Head);
                    var leftHand = traditionalInput.GetBoneTransform(LeftHand);
                    var rightHand = traditionalInput.GetBoneTransform(RightHand);
                    var hips = visualOutput.GetBoneTransform(Hips);
                    var leftFoot = visualOutput.GetBoneTransform(LeftFoot);
                    var rightFoot = visualOutput.GetBoneTransform(RightFoot);

                    solverEffectors.headTarget.position = head.position;
                    solverEffectors.leftHandTarget.position = leftHand.position;
                    solverEffectors.rightHandTarget.position = rightHand.position;
                    solverEffectors.hipTarget.position = hips.position;
                    solverEffectors.leftFootTarget.position = leftFoot.position;
                    solverEffectors.rightFootTarget.position = rightFoot.position;

                    solverEffectors.headTarget.rotation = head.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, Head);
                    solverEffectors.leftHandTarget.rotation = leftHand.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, LeftHand);
                    solverEffectors.rightHandTarget.rotation = rightHand.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, RightHand);
                    solverEffectors.hipTarget.rotation = hips.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, Hips);
                    solverEffectors.leftFootTarget.rotation = leftFoot.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, LeftFoot);
                    solverEffectors.rightFootTarget.rotation = rightFoot.rotation * MbusAnimatorUtil.ReflectiveGetPostRotation(traditionalInput.avatar, RightFoot);

                    solverOptional.DoUpdate();

                    if (_debug_drawSolverLines)
                    {
                        Resilience.Visualize.DataViz.Instance.DrawLine(leftHand.position, head.position, Color.cyan, 2f);
                        Resilience.Visualize.DataViz.Instance.DrawLine(rightHand.position, head.position, Color.cyan, 2f);
                    }
                }
#endif
            }
        }

        /// Call Resolve after the Traditional IK has been resolved, **and after Networking has registered the solved pose**.
        /// The networked avatar must not have the physics simulation in it.
        public void Resolve()
        {
            switch (strategy)
            {
                case P12PoseMergerStrategy.ApplyDirectly:
                    Strategy_ApplyPhysicsDirectlyToVisualRepresentation();
                    break;
                case P12PoseMergerStrategy.UnstretchBonesAndApply:
                    Strategy_UnstretchBones();
                    break;
                case P12PoseMergerStrategy.CopyTraditional:
                    Strategy_ApplyTraditionalDirectlyToVisualRepresentation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// Copies the traditional pose directly onto the visual representation.
        /// This provides an easy way to keep the animator of the traditional pose enabled,
        /// while having a vidual representation for debugging purposes which can be enabled and disabled
        /// without needing to select all the individual renderers within the Animator.
        private void Strategy_ApplyTraditionalDirectlyToVisualRepresentation()
        {
            foreach (var hbb in HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded)
            {
                var visual = visualOutput.GetBoneTransform(hbb);
                if (visual != null)
                {
                    var traditionalNullable = traditionalInput.GetBoneTransform(hbb);
                    if (traditionalNullable != null)
                    {
                        if (hbb == Hips)
                        {
                            visual.position = traditionalNullable.position;
                            visual.rotation = traditionalNullable.rotation;
                        }
                        else
                        {
                            visual.localPosition = traditionalNullable.localPosition;
                            visual.localRotation = traditionalNullable.localRotation;
                        }
                    }
                }
            }
        }

        /// Copies the physics pose directly to the visual representation.
        /// The physics pose may have stretched bones (distance betwen joints are shorter or longer than expected),
        /// so the visual representation may be stretched.
        private void Strategy_ApplyPhysicsDirectlyToVisualRepresentation()
        {
            foreach (var hbb in HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded)
            {
                var visual = visualOutput.GetBoneTransform(hbb);
                if (visual != null)
                {
                    var physicsNullable = physicsRig.GetBoneTransform(hbb);
                    if (physicsNullable != null)
                    {
                        // Setting the position may stretch the visual representation
                        visual.position = physicsNullable.position;
                        visual.rotation = physicsNullable.rotation;
                    }
                    else
                    {
                        var traditionalNullable = traditionalInput.GetBoneTransform(hbb);
                        if (traditionalNullable != null)
                        {
                            if (hbb == Hips)
                            {
                                visual.position = traditionalNullable.position;
                                visual.rotation = traditionalNullable.rotation;
                            }
                            else
                            {
                                visual.localPosition = traditionalNullable.localPosition;
                                visual.localRotation = traditionalNullable.localRotation;
                            }
                        }
                    }
                }
            }
        }

        /// The simulated physics may increase or decrease the distance between the joints,
        /// which causes the model to stretch. This operation unstretches and applies it to the visual representation.
        /// Unstretching is currently done around the Hips joint, which stays in place.
        private void Strategy_UnstretchBones()
        {
            foreach (var hbb in HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded)
            {
                var visual = visualOutput.GetBoneTransform(hbb);
                if (visual != null)
                {
                    var physicsNullable = physicsRig.GetBoneTransform(hbb);
                    var traditionalNullable = traditionalInput.GetBoneTransform(hbb);
                    if (traditionalNullable != null)
                    {
                        if (hbb == Hips)
                        {
                            // FIXME: Moving the hips to the physics root may not be the correct strategy to adopt.
                            if (physicsNullable != null)
                            {
                                visual.position = physicsNullable.position;
                            }
                            else
                            {
                                visual.position = traditionalNullable.position;
                            }
                        }
                        else
                        {
                            if (_debug_readjustHighMassPhysics && physicsNullable != null)
                            {
                                // Order matters, we reuse the position of the visual right below the physics control test
                                visual.localPosition = traditionalNullable.localPosition;

                                // Order matters
                                // FIXME: Move this to the Keyframe controller
                                // We're gonna need finer control per-bone
                                if (physicsNullable.GetComponent<Rigidbody>().mass > 75
                                    // FIXME: This adjustment is not good for the Head bone
                                    // FIXME: This is a hack for debug purposes only
                                    && !physicsNullable.name.Contains("Head"))
                                {
                                    var physicsPositionWeJustSet = visual.position;
                                    var distance = Vector3.Distance(physicsPositionWeJustSet, traditionalNullable.position);
                                    var closeToTooFar = Mathf.InverseLerp(_debug_readjustHighMassPhysicsCurveClose, _debug_readjustHighMassPhysicsCurveFar, distance);
                                    if (closeToTooFar < 1f)
                                    {
                                        var curvedCloseToTooFar = _debug_readjustHighMassPhysicsCurve.Evaluate(closeToTooFar);
                                        var newPosition = Vector3.Lerp(traditionalNullable.position, physicsPositionWeJustSet, curvedCloseToTooFar);
                                        dataViz._DrawLine(physicsPositionWeJustSet, newPosition, Color.orange, Color.orange, 4f);

                                        // FIXME: this condition is not necessary, currently we never readjust high mass on the hip
                                        if (hbb != Hips)
                                        {
                                            // FIXME: This logic is incorrect of the high mass object has multiple children, this whole thing needs to be restricter further up
                                            var visualParent = visual.parent;
                                            var modifier = Quaternion.FromToRotation(
                                                physicsPositionWeJustSet - visualParent.position,
                                                newPosition - visualParent.position
                                            );
                                            visualParent.rotation *= modifier;
                                        }
                                        // Order matters, rotate the parent bone before doing this
                                        visual.position = newPosition;
                                    }
                                }
                            }
                            else
                            {
                                visual.localPosition = traditionalNullable.localPosition;
                            }
                        }

                        if (physicsNullable != null)
                        {
                            visual.rotation = physicsNullable.rotation;
                        }
                        else
                        {
                            if (hbb == Hips)
                            {
                                visual.rotation = traditionalNullable.rotation;
                            }
                            else
                            {
                                visual.localRotation = traditionalNullable.localRotation;
                            }
                        }

                        if (physicsNullable != null)
                        {
                            // FIXME: Expose rigidbody in the rig
                            var body = physicsNullable.GetComponent<Rigidbody>();
                            var centerOfMass = body.centerOfMass;

                            dataViz._DrawLine(physicsNullable.TransformPoint(centerOfMass), visual.TransformPoint(centerOfMass), Color.black, Color.green, 0.4f);
                        }
                    }

                }
            }
        }
    }

    public enum P12PoseMergerStrategy
    {
        ApplyDirectly,
        UnstretchBonesAndApply,
        CopyTraditional
    }
}
