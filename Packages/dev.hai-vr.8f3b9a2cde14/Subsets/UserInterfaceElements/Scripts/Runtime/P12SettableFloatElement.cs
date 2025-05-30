using System;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    [CreateAssetMenu(fileName = "P12SettableFloatElement", menuName = "HVR.Basis/P12/Settable Float Element")]
    public class P12SettableFloatElement : ScriptableObject
    {
        public string locKey;
        public string englishTitle;
        public float min = 0f;
        public float max = 1f;
        public P12UnitDisplayKind displayAs = P12UnitDisplayKind.ArbitraryFloat;

        public float defaultValue;

        [NonSerialized] public float storedValue;

        private void OnEnable()
        {
            storedValue = defaultValue;
        }

        public enum P12UnitDisplayKind
        {
            ArbitraryFloat,
            Percentage01,
            Percentage080,
            Percentage0100,
            AngleDegrees,
            InGameRangeUnityUnits,
            RealWorldPhysicalSpaceMetricUnits,
            RealWorldPhysicalSpaceImperialUnits,
        }
    }
}
