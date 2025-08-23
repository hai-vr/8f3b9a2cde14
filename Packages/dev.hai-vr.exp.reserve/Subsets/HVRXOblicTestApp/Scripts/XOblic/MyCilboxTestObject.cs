using UnityEngine;

namespace Hai.Project12.HVRXOblicTestApp.XOblic
{
    [Cilboxable]
    public class MyCilboxTestObject : MonoBehaviour
    {
        public string input = "Bananas";
        public float anglePerSecond;

        public float _angle;

        private void OnEnable()
        {
            Debug.Log($"Input is {input}");
        }

        private void Update()
        {
            _angle += anglePerSecond * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0, _angle, 0);
        }
    }
}
