using UnityEngine;

namespace Hai.Project12.RigidbodyAdditions.Runtime
{
    public class P12InertiaTensorEditorApplier : MonoBehaviour
    {
        public Vector3 baseInertiaTensor = Vector3.one;
        public float multiplier = 1f;

        public float minMultiplier = 0.001f;

        private void OnValidate()
        {
            Apply();
            // OnValidate ONLY WORKS IN EDITOR.
        }

        public void Apply()
        {
            var rb = GetComponent<Rigidbody>();
            rb.inertiaTensor = baseInertiaTensor * Mathf.Clamp(multiplier, minMultiplier, float.MaxValue);
        }
    }
}
