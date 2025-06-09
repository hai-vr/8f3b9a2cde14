using UnityEngine;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12RigidbodyKeyframer : MonoBehaviour
    {
        private const float ControlledProximityRadius = 1.2f;

        [SerializeField] private float stabilisationForcePerSecond = 1000000f;
        [SerializeField] private float stabilisationTorquePerSecond = 700000f;
        [SerializeField] private ArticulationBody body;
        [SerializeField] private Transform pin;

        private float FixedTimeAdjustment;
        private float FixedTimeAdjustmentSq;

        private void FixedUpdate()
        {
            FixedTimeAdjustment = 1f / 90f;
            FixedTimeAdjustmentSq = FixedTimeAdjustment * FixedTimeAdjustment;

            //

            var desiredPosition = pin.position;
            var desiredRotation = pin.rotation;

            var distance = Vector3.Distance(body.worldCenterOfMass, desiredPosition + desiredRotation * body.centerOfMass); // FIXME: Issue in center of mass
            var control = Mathf.Clamp01(distance / ControlledProximityRadius);
            if (true)
            {
                var pow = true ? Mathf.Pow(control, 2) : 0.1f;
                var totalChange = (pow * stabilisationForcePerSecond * FixedTimeAdjustment);
                // var globalForce = (desiredPosition - body.worldCenterOfMass).normalized * Mathf.Clamp(totalChange, 0f, 100f);
                var globalForce = (desiredPosition - body.worldCenterOfMass).normalized * totalChange;
                body.AddForce(globalForce, ForceMode.Acceleration);
                // body.AddForce(Vector3.up * 9.1f, ForceMode.Force);
                body.linearVelocity = Vector3.zero;
            }

            if (true)
            {
                var movementRequired = desiredRotation * Quaternion.Inverse(body.transform.rotation);
                movementRequired.ToAngleAxis(out var signedDegrees, out var axis);
                if (signedDegrees > 180f) signedDegrees -= 360f; // NOT SURE: Try to fix a weird singularity in the angular velocity
                var angularVelBase = axis * (signedDegrees * Mathf.Deg2Rad);
                // body.angularVelocity = angularVelBase * (stabilisationTorquePerSecond * FixedTimeAdjustmentSq);
                // if (false) body.angularVelocity = angularVelBase * (stabilisationTorquePerSecond * FixedTimeAdjustmentSq);
                body.AddTorque(angularVelBase * (stabilisationTorquePerSecond * FixedTimeAdjustment), ForceMode.Acceleration);
                // body.angularVelocity = Vector3.zero;
            }

            if (float.IsNaN(body.transform.position.x))
            {
                body.TeleportRoot(pin.position, pin.rotation);
            }

            // body.maxLinearVelocity = 1f;
            // body.maxAngularVelocity = 1f;
            // body.maxDepenetrationVelocity = 1f;
            // body.maxJointVelocity = 1f;

            body.mass = Mathf.Lerp(1f, 100f, control);
            // body.mass = 1f;
            // body.linearDamping = 10;
            // body.angularDamping = 10;
            body.useGravity = false;
        }
    }
}
