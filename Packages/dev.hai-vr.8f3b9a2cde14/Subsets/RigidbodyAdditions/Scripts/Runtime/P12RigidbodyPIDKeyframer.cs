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

        private H12PIDControllerVector3 _positionPid;
        private H12PIDControllerAngularVelocity _rotationPid;

        private void Awake()
        {
            _positionPid = new H12PIDControllerVector3();
            _rotationPid = new H12PIDControllerAngularVelocity();
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

            var targetCenterOfMass = target.position + target.TransformVector(body.centerOfMass);

            var currentCenterOfMass = body.worldCenterOfMass;
            var force = _positionPid._Update(Time.fixedDeltaTime, currentCenterOfMass, targetCenterOfMass);
            body.AddForce(force, ForceMode.Acceleration);

            dataViz._DrawLine(currentCenterOfMass, targetCenterOfMass, Color.cyan, Color.red, 1f); // FIXME: Clipping
            Debug.DrawLine(currentCenterOfMass, targetCenterOfMass, Color.cyan);

            var result = _rotationPid._Update(Time.fixedDeltaTime, body.transform.rotation, target.rotation);
            body.AddTorque(result, ForceMode.Acceleration);
        }
    }
}
