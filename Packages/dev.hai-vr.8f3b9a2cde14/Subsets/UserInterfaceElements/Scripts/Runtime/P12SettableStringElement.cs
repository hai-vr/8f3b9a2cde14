using System;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    [CreateAssetMenu(fileName = "P12SettableStringElement", menuName = "HVR.Basis/P12/Settable String Element")]
    public class P12SettableStringElement : ScriptableObject
    {
        public string locKey;
        public string englishTitle;

        public string defaultValue;

        [NonSerialized] public string storedValue;

        private void OnEnable()
        {
            Debug.Log($"OnEnable called on {name}");
            storedValue = defaultValue;
        }
    }
}
