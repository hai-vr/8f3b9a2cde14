using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.HumanBodyBones;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12HumanoidRearticulator : MonoBehaviour
    {
        [SerializeField] private bool includeFingers = false;
        [SerializeField] private Animator humanoidReference;

        [SerializeField] private float _debug_estimatedBodyHeight;
        [SerializeField] private float _debug_estimatedBodyMass;

        private void Awake()
        {
            var availableBones = new List<(HumanBodyBones, ArticulationBody)>();

            // Create articulations

            for (HumanBodyBones bone = Hips; bone < LastBone; bone++)
            {
                var isFingerBone = bone >= LeftThumbProximal && bone <= RightLittleDistal;
                if (!includeFingers && isFingerBone) continue;

                if (BoneIsInconsequential(bone)) continue;

                var boneTransform = humanoidReference.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    MakeArticulation(bone, boneTransform);

                    var articulationBody = boneTransform.GetComponent<ArticulationBody>(); // TODO: Also support rigidbody?
                    availableBones.Add((bone, articulationBody));
                }
            }

            // HACK: Test conflicts
            // foreach (var availableBone in availableBones)
            // {
            //     var articulationBody = availableBone.Item2;
            //     articulationBody.jointType = ArticulationJointType.SphericalJoint;
            //     articulationBody.swingYLock = ArticulationDofLock.FreeMotion;
            //     articulationBody.swingZLock = ArticulationDofLock.FreeMotion;
            //     articulationBody.twistLock = ArticulationDofLock.FreeMotion;
            // }

            // Adjust mass

            var totalBodyMass = EstimateTotalBodyMass();

            var totalDistribution = 0f;
            foreach (var availableBone in availableBones)
            {
                totalDistribution += MassDistribution(availableBone.Item1);
            }

            foreach (var availableBone in availableBones)
            {
                var massToApply = (MassDistribution(availableBone.Item1) / totalDistribution) * totalBodyMass;
                availableBone.Item2.mass = massToApply;
            }
        }

        private float EstimateTotalBodyMass()
        {
            // FIXME: We need to resolve this from the humanoid info, not world space.
            var leftFoot = humanoidReference.GetBoneTransform(LeftFoot).position.y;
            var headBase = humanoidReference.GetBoneTransform(Head).position.y;
            var height = (headBase - leftFoot) * 1.25f;
            _debug_estimatedBodyHeight = height;

            // Calculate an estimate for the body mass based on height
            var t = (height - 1.35f) / (1.9f - 1.35f);
            var totalBodyMass = Mathf.LerpUnclamped(30f, 85f, t);
            _debug_estimatedBodyMass = totalBodyMass;

            return totalBodyMass;
        }

        // Provides an arbitrary number which is used for the distribution of mass across the body.
        private int MassDistribution(HumanBodyBones bone)
        {
            return bone switch
            {
                Hips => 100,
                LeftUpperLeg or RightUpperLeg => 80,
                LeftLowerLeg or RightLowerLeg => 70,
                LeftFoot or RightFoot => 50,
                Spine => 85,
                Chest => 70,
                UpperChest => 60,
                Neck => 50,
                Head => 30,
                LeftShoulder or RightShoulder => 50,
                LeftUpperArm or RightUpperArm => 40,
                LeftLowerArm or RightLowerArm => 30,
                LeftHand or RightHand => 10,
                LeftToes or RightToes => 10,
                LeftEye or RightEye => 1,
                Jaw => 1,
                LeftThumbProximal
                    or LeftIndexProximal
                    or LeftMiddleProximal
                    or LeftRingProximal
                    or LeftLittleProximal
                    or RightThumbProximal
                    or RightIndexProximal
                    or RightMiddleProximal
                    or RightRingProximal
                    or RightLittleProximal => 4,
                LeftThumbIntermediate
                    or LeftIndexIntermediate
                    or LeftMiddleIntermediate
                    or LeftRingIntermediate
                    or LeftLittleIntermediate
                    or RightThumbIntermediate
                    or RightIndexIntermediate
                    or RightMiddleIntermediate
                    or RightRingIntermediate
                    or RightLittleIntermediate => 3,
                LeftThumbDistal
                    or LeftIndexDistal
                    or LeftMiddleDistal
                    or LeftRingDistal
                    or LeftLittleDistal
                    or RightThumbDistal
                    or RightIndexDistal
                    or RightMiddleDistal
                    or RightRingDistal
                    or RightLittleDistal => 2,
                LastBone => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(bone), bone, null)
            };
        }

        private static bool BoneIsInconsequential(HumanBodyBones bone)
        {
            return bone is LeftEye
                or RightEye
                or Jaw
                or LeftShoulder
                or RightShoulder
                or UpperChest
                or LeftToes
                or RightToes;
        }

        private void MakeArticulation(HumanBodyBones bone, Transform boneTransform)
        {
            var go = boneTransform.gameObject;

            var Damping = 0.3f;

            var articulationBody = go.GetComponent<ArticulationBody>();
            if (articulationBody == null)
            {
                articulationBody = articulationBody != null ? articulationBody : go.AddComponent<ArticulationBody>();
                articulationBody.immovable = false;
                articulationBody.mass = 3f;
                articulationBody.automaticCenterOfMass = true;
                articulationBody.useGravity = true;
                articulationBody.jointType = ArticulationJointType.SphericalJoint;

                articulationBody.swingYLock = ArticulationDofLock.LimitedMotion;
                articulationBody.swingZLock = ArticulationDofLock.LimitedMotion;
                articulationBody.twistLock = ArticulationDofLock.LimitedMotion;

                articulationBody.linearDamping = Damping;
                articulationBody.angularDamping = Damping;

                articulationBody.jointFriction = 0.05f;

                var yDrive = articulationBody.yDrive;
                var zDrive = articulationBody.zDrive;
                var twist = articulationBody.xDrive;
                yDrive.lowerLimit = -90;
                yDrive.upperLimit = +90;

                zDrive.lowerLimit = -0;
                zDrive.upperLimit = +0;

                twist.lowerLimit = -15;
                twist.upperLimit = +15;

                yDrive.damping = Damping;
                zDrive.damping = Damping;
                twist.damping = Damping;

                articulationBody.yDrive = yDrive;
                articulationBody.zDrive = zDrive;
                articulationBody.xDrive = twist;

                articulationBody.excludeLayers = LayerMask.NameToLayer("Default");
                articulationBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                // articulationBody.maxAngularVelocity = 0.1f;
                // articulationBody.maxDepenetrationVelocity = 0.1f;
                // articulationBody.maxJointVelocity = 0.1f;
                // articulationBody.maxLinearVelocity = 0.1f;
            }
            else
            {
                if (articulationBody != null)
                {
                    articulationBody.excludeLayers = LayerMask.NameToLayer("Default");
                    articulationBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                    articulationBody.linearDamping = Damping;
                    articulationBody.angularDamping = Damping;

                    var yDrive = articulationBody.yDrive;
                    var zDrive = articulationBody.zDrive;
                    var twist = articulationBody.xDrive;
                    yDrive.damping = Damping;
                    zDrive.damping = Damping;
                    twist.damping = Damping;
                    articulationBody.yDrive = yDrive;
                    articulationBody.zDrive = zDrive;
                    articulationBody.xDrive = twist;

                    // articulationBody.maxAngularVelocity = 0.1f;
                    // articulationBody.maxDepenetrationVelocity = 0.1f;
                    // articulationBody.maxJointVelocity = 0.1f;
                    // articulationBody.maxLinearVelocity = 0.1f;
                }
            }
        }
    }
}
