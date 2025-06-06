using UnityEngine;

namespace Hai.Project12.InteractionElements.Runtime
{
    public class P12Respawnable : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _localPos;
        private Quaternion _localRot;

        private void Awake()
        {
            _localPos = transform.localPosition;
            _localRot = transform.localRotation;

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                BasisDebug.LogWarning($"{nameof(P12Respawnable)} does not have a rigidbody. It will be disabled.");
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!_rigidbody.IsSleeping() && transform.position.y < -100)
            {
                Respawn();
            }
        }

        public void Respawn()
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
            transform.localPosition = _localPos;
            transform.localRotation = _localRot;
        }
    }
}
