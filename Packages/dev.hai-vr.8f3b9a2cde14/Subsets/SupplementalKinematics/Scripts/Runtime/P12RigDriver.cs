using System;
using System.Collections.Generic;
using Hai.Project12.DataViz.Runtime;
using Hai.Project12.HaiSystems.DataStructures;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.RigidbodyAdditions.Runtime;
using UnityEngine;

namespace Hai.Project12.SupplementalKinematics.Runtime
{
    [DefaultExecutionOrder(-80)] // Must be run after P12HumanoidRearticulator
    public class P12RigDriver : MonoBehaviour
    {
        private static readonly Vector3 InertiaTensor_Powered = Vector3.one * 0.17f;
        private static readonly Vector3 InertiaTensor_Hips_Powered = Vector3.one * 1.05f;
        private static readonly Vector3 InertiaTensor_Spine_Powered = Vector3.one * 1.05f;
        private static readonly Vector3 InertiaTensor_Chest_Powered = Vector3.one * 0.54f;

        [EarlyInjectable] [SerializeField] private P12QuickDataViz dataViz;
        [SerializeField] private P12Rig rig;
        [SerializeField] private Animator puppetReference;
        [SerializeField] private Transform pidRoot;

        private readonly Dictionary<HumanBodyBones, float> _defaultMass = new();
        private readonly Dictionary<HumanBodyBones, H12RigData> _rigData = new();

        [Range(0,2)] [SerializeField] private int iHips = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iSpine = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iChest = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iNeck = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iHead = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftUpperArm = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftLowerArm = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftHand = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iRightUpperArm = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iRightLowerArm = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iRightHand = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftUpperLeg = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftLowerLeg = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iLeftFoot = (int)P12RigState.Keyframed;
        [Range(0,2)] [SerializeField] private int iRightUpperLeg = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iRightLowerLeg = (int)P12RigState.Powered;
        [Range(0,2)] [SerializeField] private int iRightFoot = (int)P12RigState.Keyframed;

        private P12RigState controlHips => (P12RigState)iHips;
        private P12RigState controlSpine => (P12RigState)iSpine;
        private P12RigState controlChest => (P12RigState)iChest;
        private P12RigState controlNeck => (P12RigState)iNeck;
        private P12RigState controlHead => (P12RigState)iHead;
        private P12RigState controlLeftUpperArm => (P12RigState)iLeftUpperArm;
        private P12RigState controlLeftLowerArm => (P12RigState)iLeftLowerArm;
        private P12RigState controlLeftHand => (P12RigState)iLeftHand;
        private P12RigState controlRightUpperArm => (P12RigState)iRightUpperArm;
        private P12RigState controlRightLowerArm => (P12RigState)iRightLowerArm;
        private P12RigState controlRightHand => (P12RigState)iRightHand;
        private P12RigState controlLeftUpperLeg => (P12RigState)iLeftUpperLeg;
        private P12RigState controlLeftLowerLeg => (P12RigState)iLeftLowerLeg;
        private P12RigState controlLeftFoot => (P12RigState)iLeftFoot;
        private P12RigState controlRightUpperLeg => (P12RigState)iRightUpperLeg;
        private P12RigState controlRightLowerLeg => (P12RigState)iRightLowerLeg;
        private P12RigState controlRightFoot => (P12RigState)iRightFoot;

        private struct H12RigData
        {
            public HumanBodyBones hbb;
            public Rigidbody body;
            public ConfigurableJoint jointNullableIfHips;
            public P12RigidbodyPIDKeyframer keyframer;
        }

        private void OnValidate()
        {
            // ONLY WORKS IN EDITORk
            foreach (var rigData in _rigData.Values)
            {
                var state = StateOf(rigData.hbb);
                ConfigureRigMember(rigData, state);
            }
        }

        private void Awake()
        {
            foreach (var hbb in P12PoseMerger.HumanoidRigHbbsInExpectedHierarchyOrder_EyesAndJawNotIncluded)
            {
                var rigTransform = rig.GetBoneTransform(hbb);
                if (rigTransform != null)
                {
                    var go = new GameObject
                    {
                        transform =
                        {
                            parent = pidRoot
                        },
                        name = $"PID-{hbb}"
                    };
                    go.SetActive(false);

                    // Store state
                    var body = rigTransform.GetComponent<Rigidbody>();
                    _defaultMass[hbb] = body.mass;

                    // Create
                    var state = StateOf(hbb);
                    H12RigData rigData;
                    {
                        var keyframer = CreateKeyframer(go, hbb, state);

                        ConfigurableJoint configurableJointNullable;
                        if (hbb != HumanBodyBones.Hips)
                        {
                            configurableJointNullable = rigTransform.GetComponent<ConfigurableJoint>();
                        }
                        else
                        {
                            configurableJointNullable = null;
                        }

                        rigData = new H12RigData
                        {
                            hbb = hbb,
                            body = body,
                            keyframer = keyframer,
                            jointNullableIfHips = configurableJointNullable
                        };
                        _rigData.Add(hbb, rigData);
                    }

                    // Configure
                    ConfigureRigMember(rigData, state);

                    go.SetActive(true);
                }
            }
        }

        private void ConfigureRigMember(H12RigData rigData, P12RigState state)
        {
            var hbb = rigData.hbb;
            ConfigureKeyframer(rigData.keyframer, hbb, state);
            ConfigureRigidbody(rigData.body, hbb, state); // TODO: Get the rigidbody from the rig
            if (rigData.hbb != HumanBodyBones.Hips)
            {
                ConfigureJoint(rigData.jointNullableIfHips, hbb, state);
            }
        }

        private P12RigState StateOf(HumanBodyBones hbb)
        {
            return hbb switch
            {
                HumanBodyBones.Hips => controlHips,
                HumanBodyBones.Spine => controlSpine,
                HumanBodyBones.Chest => controlChest,
                HumanBodyBones.Neck => controlNeck,
                HumanBodyBones.Head => controlHead,
                HumanBodyBones.LeftUpperArm => controlLeftUpperArm,
                HumanBodyBones.LeftLowerArm => controlLeftLowerArm,
                HumanBodyBones.LeftHand => controlLeftHand,
                HumanBodyBones.RightUpperArm => controlRightUpperArm,
                HumanBodyBones.RightLowerArm => controlRightLowerArm,
                HumanBodyBones.RightHand => controlRightHand,
                HumanBodyBones.LeftUpperLeg => controlLeftUpperLeg,
                HumanBodyBones.LeftLowerLeg => controlLeftLowerLeg,
                HumanBodyBones.LeftFoot => controlLeftFoot,
                HumanBodyBones.RightUpperLeg => controlRightUpperLeg,
                HumanBodyBones.RightLowerLeg => controlRightLowerLeg,
                HumanBodyBones.RightFoot => controlRightFoot,
                _ => P12RigState.Powered
            };
        }

        private P12RigidbodyPIDKeyframer CreateKeyframer(GameObject go, HumanBodyBones hbb, P12RigState state)
        {
            var keyframer = go.AddComponent<P12RigidbodyPIDKeyframer>();
            keyframer.physicsRig = rig;
            keyframer.humanBodyBone = hbb;
            keyframer.target = null;
            keyframer.dataViz = dataViz;
            keyframer.targetReferenceOptional = puppetReference;
            keyframer.proportionalGain = Proportional(state);
            keyframer.integralGain = 0;
            keyframer.derivativeGain = 20;
            keyframer.integralMaximumMagnitude = 10;
            keyframer.proportionalTorqueMul = 10;
            keyframer.integralTorqueMul = 1;
            keyframer.derivativeTorqueMul = 1;
            keyframer.integralMaximumMagnitudelTorqueMul = 1;
            keyframer.compensateGravity = false;
            keyframer.forceLimit = 1000;
            keyframer.torqueLimit = 6000;
            return keyframer;
        }

        private void ConfigureKeyframer(P12RigidbodyPIDKeyframer keyframer, HumanBodyBones hbb, P12RigState state)
        {
            keyframer.enabled = KeyframerEnabled(state);
            keyframer.target = null;
            keyframer.targetReferenceOptional = puppetReference;
            keyframer.proportionalGain = Proportional(state);
        }

        private void ConfigureRigidbody(Rigidbody body, HumanBodyBones hbb, P12RigState state)
        {
            body.useGravity = Gravity(state);
            body.inertiaTensor = hbb switch
            {
                // HumanBodyBones.Hips => InertiaTensor_Hips_Powered,
                // HumanBodyBones.Spine => InertiaTensor_Spine_Powered,
                // HumanBodyBones.Chest => InertiaTensor_Chest_Powered,
                _ => InertiaTensor_Powered
            };
            body.mass = Mass(state, hbb);
        }

        private void ConfigureJoint(ConfigurableJoint configurableJoint, HumanBodyBones hbb, P12RigState state)
        {
            var drive = configurableJoint.slerpDrive;
            drive.positionDamper = Damper(state);
            drive.positionSpring = Spring(state);
            configurableJoint.slerpDrive = drive;
        }

        private bool KeyframerEnabled(P12RigState state)
        {
            return state switch
            {
                P12RigState.Powered => true,
                P12RigState.Keyframed => true,
                P12RigState.Flailing => false,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private float Proportional(P12RigState state)
        {
            return state switch
            {
                P12RigState.Powered => 300f,
                P12RigState.Keyframed => 1000f,
                P12RigState.Flailing => 0f, // This will be necessary for lerping .enabled if we choose to add rig controller lerping
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private bool Gravity(P12RigState state)
        {
            return state switch
            {
                P12RigState.Powered => false,
                P12RigState.Keyframed => false,
                P12RigState.Flailing => true,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private float Mass(P12RigState state, HumanBodyBones hbb)
        {
            return state switch
            {
                P12RigState.Powered => _defaultMass[hbb],
                P12RigState.Keyframed => 100f,
                P12RigState.Flailing => _defaultMass[hbb],
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private float Damper(P12RigState state)
        {
            return state switch
            {
                P12RigState.Powered => 100f,
                P12RigState.Keyframed => 100f,
                P12RigState.Flailing => 0f,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private static float Spring(P12RigState state)
        {
            return state switch
            {
                P12RigState.Powered => 0f,
                P12RigState.Keyframed => 0f,
                P12RigState.Flailing => 10f,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }

    public enum P12RigState
    {
        Flailing, Powered, Keyframed
    }
}
