using Hai.Project12.DataViz.Runtime;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.Remesher.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12RigidbodyPIDKeyframer : MonoBehaviour
    {
        [FormerlySerializedAs("body")] [SerializeField] private Rigidbody bodyOptional;

        [SerializeField] private P12Remesher remesherOptional;
        [SerializeField] private HumanBodyBones humanBodyBone;

        [FormerlySerializedAs("targetCenterOfMass")] [SerializeField] private Transform target; // TODO: This is no longer a center of mass
        [EarlyInjectable] [SerializeField] private P12QuickDataViz dataViz;

        [SerializeField] private float proportionalGain = 1000;
        [SerializeField] private float integralGain = 10;
        [SerializeField] private float derivativeGain = 40;
        [SerializeField] private float integralMaximumMagnitude = 10;

        [SerializeField] private bool compensateGravity = true;
        [SerializeField] private float forceLimit = 1000f;
        [SerializeField] private float torqueLimit = 2000f;

        [SerializeField] private bool _debug_clickToResetJoints;
        [SerializeField] private bool _debug_print;
        [SerializeField] private bool _debug_superControl;

        private H12PIDControllerVector3 _positionPid;
        private H12PIDControllerAngularVelocity _rotationPid;

        private Vector3 _drawTargetCenterOfMass;
        private Vector3 _drawCurrentCenterOfMass;
        private float _angleDiff;
        private Quaternion _drawCurrentRotation;
        private Quaternion _drawTargetRotation;

        private void Awake()
        {
            _positionPid = new H12PIDControllerVector3();
            _rotationPid = new H12PIDControllerAngularVelocity();
        }

        private void OnDisable()
        {
            _positionPid._ResetAsFirstFrame();
            _rotationPid._ResetAsFirstFrame();
        }

        private void FixedUpdate()
        {
            if (bodyOptional == null)
            {
                var rigBone = remesherOptional.Rig[humanBodyBone];
                bodyOptional = rigBone.GetComponent<Rigidbody>();
            }

            _positionPid.proportionalGain = proportionalGain;
            _positionPid.integralGain = integralGain;
            _positionPid.derivativeGain = derivativeGain;
            _positionPid.integralMaximumMagnitude = integralMaximumMagnitude;

            _rotationPid.proportionalGain = proportionalGain * 10;
            _rotationPid.integralGain = integralGain * 1;
            _rotationPid.derivativeGain = derivativeGain * 1;
            _rotationPid.integralMaximumMagnitude = integralMaximumMagnitude;

            if (_debug_clickToResetJoints)
            {
                _debug_clickToResetJoints = false;
                // TODO: Reset configurable joint
            }

            var currentCenterOfMass = bodyOptional.worldCenterOfMass;
            if (float.IsNaN(currentCenterOfMass.x)) return;

            var targetCenterOfMass = target.position + target.TransformVector(bodyOptional.centerOfMass);

            var force = _positionPid._Update(Time.fixedDeltaTime, currentCenterOfMass, targetCenterOfMass);
            var forceMagnitude = force.magnitude;
            if (forceMagnitude > forceLimit) force = force.normalized * forceLimit;
            bodyOptional.AddForce(force, ForceMode.Acceleration);

            var currentRotation = bodyOptional.transform.rotation;
            var targetRotation = target.rotation;

            var torque = _rotationPid._Update(Time.fixedDeltaTime, currentRotation, targetRotation);
            var torqueMagnitude = torque.magnitude;
            if (torqueMagnitude > torqueLimit) torque = torque.normalized * torqueLimit;
            bodyOptional.AddTorque(torque, ForceMode.Acceleration);

            if (_debug_print)
            {
                H12Debug.Log($"Torque is {torque}");
            }

            _angleDiff = Mathf.Clamp01(Quaternion.Angle(currentRotation, targetRotation) / 30f);

            _drawTargetCenterOfMass = targetCenterOfMass;
            _drawCurrentCenterOfMass = currentCenterOfMass;

            _drawCurrentRotation = currentRotation;
            _drawTargetRotation = targetRotation;

            // Compensate for gravity
            if (compensateGravity)
            {
                bodyOptional.AddForce(-Physics.gravity, ForceMode.Force);
            }

            Debug__SuperControl();
        }

        private void Debug__SuperControl()
        {
            if (_debug_superControl)
            {
                bodyOptional.position = target.position;
                bodyOptional.rotation = target.rotation;
                bodyOptional.transform.position = target.position;
                bodyOptional.transform.rotation = target.rotation;
            }
        }

        private void Update()
        {
            Debug__SuperControl();

            dataViz._DrawLine(_drawCurrentCenterOfMass, _drawTargetCenterOfMass, Color.cyan, Color.red, 1f * proportionalGain / 1000);
        }

        private void LateUpdate()
        {
            Debug__SuperControl();
        }
    }
}
