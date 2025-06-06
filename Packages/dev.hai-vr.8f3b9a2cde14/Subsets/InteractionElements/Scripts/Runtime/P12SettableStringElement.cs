using UnityEngine;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    [CreateAssetMenu(fileName = "P12SettableStringElement", menuName = "HVR.Basis/Project12/Settable String Element")]
    public class P12SettableStringElement : ScriptableObject
    {
        public string locKey;
        public string localizedTitle;

        public string defaultValue;

        private string _storedValue;
        public string storedValue
        {
            get => _storedValue;
            set
            {
                _storedValue = value;
                OnValueChanged?.Invoke(_storedValue);
            }
        }

        public event ValueChanged OnValueChanged;
        public delegate void ValueChanged(string newValue);

        private void OnEnable()
        {
            storedValue = defaultValue;
        }
    }
}
