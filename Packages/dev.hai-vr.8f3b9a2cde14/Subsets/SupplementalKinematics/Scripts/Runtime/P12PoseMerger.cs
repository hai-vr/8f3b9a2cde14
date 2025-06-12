using System;
using Hai.Project12.DataViz.Runtime;
using Hai.Project12.HaiSystems.DataStructures;
using UnityEngine;
using static UnityEngine.HumanBodyBones;

namespace Hai.Project12.SupplementalKinematics.Runtime
{
    public class P12PoseMerger : MonoBehaviour
    {
        // In the HumanBodyBones, UpperChest comes near the end. This creates a problem if we need
        // to set the position and rotation of each transform, as iterating the original enum would shift the
        // position of the bones that had been set earlier.
        private static readonly HumanBodyBones[] HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded = {
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
        [SerializeField] private P12Rig physicsRig;
        [SerializeField] private Animator visualOutput;
        [SerializeField] private P12PoseMergerStrategy strategy;
        [SerializeField] private P12QuickDataViz dataViz;

        // TODO: Remove this in the future, when we have a process to create an execution loop that is more tightly controlled.
        // The changes made by this component must not contaminate the networking data.
        private void Update()
        {
            Resolve();
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
                            visual.position = physicsNullable != null ? physicsNullable.position : traditionalNullable.position;
                        }
                        else
                        {
                            visual.localPosition = traditionalNullable.localPosition;
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
