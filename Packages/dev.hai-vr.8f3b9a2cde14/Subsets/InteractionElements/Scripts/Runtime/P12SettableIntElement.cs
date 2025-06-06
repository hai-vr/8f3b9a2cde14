using System;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    [CreateAssetMenu(fileName = "P12SettableIntElement", menuName = "HVR.Basis/Project12/Settable Int Element")]
    public class P12SettableIntElement : ScriptableObject
    {
        public string locKey;
        public string localizedTitle;
        public int min = 0;
        public int max = 10;

        public int defaultValue;

        [NonSerialized] public int storedValue;

        private void OnEnable()
        {
            storedValue = defaultValue;
        }
    }
}
