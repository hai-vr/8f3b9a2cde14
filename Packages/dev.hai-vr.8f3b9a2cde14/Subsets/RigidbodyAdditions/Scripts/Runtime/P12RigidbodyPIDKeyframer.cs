using Hai.Project12.DataViz.Runtime;
using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12RigidbodyPIDKeyframer : MonoBehaviour
    {
        [SerializeField] private ArticulationBody body;
        [FormerlySerializedAs("targetCenterOfMass")] [SerializeField] private Transform target; // TODO: This is no longer a center of mass
        [EarlyInjectable] [SerializeField] private P12QuickDataViz dataViz;

        [SerializeField] private float proportionalGain = 1000;
        [SerializeField] private float integralGain = 10;
        [SerializeField] private float derivativeGain = 40;
        [SerializeField] private float integralMaximumMagnitude = 10;

        [SerializeField] private bool _debug_clickToResetJoints;

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
            _positionPid.proportionalGain = proportionalGain;
            _positionPid.integralGain = integralGain;
            _positionPid.derivativeGain = derivativeGain;
            _positionPid.integralMaximumMagnitude = integralMaximumMagnitude;

            _rotationPid.proportionalGain = proportionalGain * 10;
            _rotationPid.integralGain = integralGain;
            _rotationPid.derivativeGain = derivativeGain;
            _rotationPid.integralMaximumMagnitude = integralMaximumMagnitude;

            if (_debug_clickToResetJoints)
            {
                _debug_clickToResetJoints = false;
                body.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
            }

            var targetCenterOfMass = target.position + target.TransformVector(body.centerOfMass);

            var currentCenterOfMass = body.worldCenterOfMass;
            var force = _positionPid._Update(Time.fixedDeltaTime, currentCenterOfMass, targetCenterOfMass);
            body.AddForce(force, ForceMode.Acceleration);

            var currentRotation = body.transform.rotation;
            var targetRotation = target.rotation;

            var result = _rotationPid._Update(Time.fixedDeltaTime, currentRotation, targetRotation);
            body.AddTorque(result, ForceMode.Acceleration);

            _angleDiff = Mathf.Clamp01(Quaternion.Angle(currentRotation, targetRotation) / 30f);

            _drawTargetCenterOfMass = targetCenterOfMass;
            _drawCurrentCenterOfMass = currentCenterOfMass;

            _drawCurrentRotation = currentRotation;
            _drawTargetRotation = targetRotation;
        }

        private void Update()
        {
            dataViz._DrawLine(_drawCurrentCenterOfMass, _drawTargetCenterOfMass, Color.cyan, Color.red, 1f); // FIXME: Clipping
            if (_angleDiff >= 1f)
            {
                // dataViz._DrawGizmo(new CrossTrackerData
                // {
                    // position = _drawTargetCenterOfMass,
                    // rotation = _drawTargetRotation
                // });
                // dataViz._DrawGizmo(new CrossTrackerData
                // {
                    // position = _drawCurrentCenterOfMass,
                    // rotation = _drawCurrentRotation
                // });
            }
        }
    }
}
