using UnityEngine;
using Random = UnityEngine.Random;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12PhysicsEmitter : MonoBehaviour
    {
        private const float NonContinuousDynamicForceWhenForceIsZero = 0.5f;

        [SerializeField] private P12CollisionSystem collisionSystem;

        private Rigidbody _ourRigidbody;
        private float _timeoutSeconds = float.MinValue;

        private void OnEnable()
        {
            _ourRigidbody = GetComponent<Rigidbody>();
            if (_ourRigidbody.interpolation == RigidbodyInterpolation.None)
            {
                Debug.LogWarning($"Rigidbody {gameObject} interpolation is set to None. This may cause visible stuttering at high-framerate, as the physics timestep is locked in Basis (as opposed to some other apps). Consider setting the Rigidbody interpolation type to Interpolate. See: https://docs.hai-vr.dev/docs/basis/physics-timestep");
            }
        }

        private void OnCollisionEnter(Collision collisionData)
        {
            var hasAtLeastOneContact = collisionData.contactCount > 0;
            if (!hasAtLeastOneContact) return;

            var collisionDamageForceMulltiplier = 1f;
            var contactPoint = collisionData.GetContact(0);

            var collisionMagnitude = collisionData.impulse.magnitude;
            float collisionForce01_ForSFX;
            var detectionMode = _ourRigidbody.collisionDetectionMode;
            if (collisionMagnitude == 0)
            {
                // Many non-ContinuousDynamic collisions will return a magnitude of 0, so for those, force SFX to play.
                collisionForce01_ForSFX = detectionMode != CollisionDetectionMode.ContinuousDynamic ? NonContinuousDynamicForceWhenForceIsZero : 0f;
            }
            else
            {
                // FIXME: Despite following the docs to get the total force applied, damage dealt varies heavily from single to triple depending
                // on the value of fixedDeltaTime. This needs an adjustment
                // var collisionForce = collisionMagnitude / Time.fixedDeltaTime; // According to Unity docs, divide by last frame's fixedDeltaTime to get the force. We're just going to use the current fixedDeltaTime for no good reason.
                var collisionForce = collisionMagnitude / (1 / 90f); // ??? FIXME: Try to debug an inconsistency in the damage reports if the app uses a 240Hz timestep vs 50Hz
                collisionForce01_ForSFX = Mathf.InverseLerp(15f, 1000f, collisionForce);
                if (collisionForce01_ForSFX == 0 && detectionMode != CollisionDetectionMode.ContinuousDynamic)
                {
                    collisionForce01_ForSFX = NonContinuousDynamicForceWhenForceIsZero;
                }
            }

            if (collisionForce01_ForSFX > 0f)
            {
                TryProduceSoundEffect(collisionData, collisionForce01_ForSFX);
            }
        }

        private void TryProduceSoundEffect(Collision collisionData, float collisionForce01)
        {
            if (Time.time < _timeoutSeconds)
            {
                return;
            }

            _timeoutSeconds = Time.time + Random.Range(0.10f, 0.23f);

            var collisionSfx = collisionSystem.TempSfx();
            if (collisionSfx != null)
            {
                if (collisionData.contactCount > 0)
                {
                    var position = collisionData.contacts[0].point;

                    collisionSystem.PlayCollision(collisionSfx, position, collisionForce01);
                }
            }
        }
    }
}
