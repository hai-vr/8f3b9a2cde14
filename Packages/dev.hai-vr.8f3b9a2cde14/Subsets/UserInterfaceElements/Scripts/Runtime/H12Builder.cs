using System;
using System.Collections.Generic;
using BattlePhaze.SettingsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hai.Project12.UserInterfaceElements
{
    internal class H12Builder
    {
        private const string EnglishOffLabel = "OFF";
        private const string EnglishOnLabel = "ON";
        private readonly P12UIEScriptedPrefabs _prefabs;
        private readonly Transform _layoutGroupHolder;
        private readonly Transform _titleGroupHolder;
        private readonly float _controlExpansion;

        public event Action AnyValueChanged;

        public H12Builder(P12UIEScriptedPrefabs prefabs, Transform layoutGroupHolder, Transform titleGroupHolder, float controlExpansion)
        {
            _prefabs = prefabs;
            _layoutGroupHolder = layoutGroupHolder;
            _titleGroupHolder = titleGroupHolder;
            _controlExpansion = controlExpansion;
        }

        private void SetupLine(P12UILine line)
        {
            line.SetControlExpansion(_controlExpansion);
        }

        internal P12UILine P12Button(string rawTitle, Action clickFn)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.titleButton, _layoutGroupHolder);
            ours.name = $"P12B-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);
            SetupLine(line);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => clickFn());

            return line;
        }

        internal P12UILine P12TitleButton(string rawTitle, Action clickFn)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.titleButton, _titleGroupHolder);
            ours.name = $"P12TB-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);
            SetupLine(line);
            line.SetFocused(false);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => clickFn());

            return line;
        }

        internal void P12SliderElement(P12SettableFloatElement settableFloat)
        {
            float Getter() => settableFloat.storedValue;
            void Setter(float newValue) => settableFloat.storedValue = newValue;

            Internal_SetupSlider(Getter, Setter, settableFloat.englishTitle, settableFloat.min, settableFloat.max, false, settableFloat.displayAs);
        }

        internal void P12SliderElement(P12SettableIntElement settableInt)
        {
            float Getter() => settableInt.storedValue;
            void Setter(float newValue) => settableInt.storedValue = (int)newValue;

            Internal_SetupSlider(Getter, Setter, settableInt.englishTitle, settableInt.min, settableInt.max, true, P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat);
        }

        private void Internal_SetupSlider(Func<float> getter, Action<float> setter, string rawString, float min, float max, bool wholeNumbers, P12SettableFloatElement.P12UnitDisplayKind displayAs)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.slider, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawString);
            SetupLine(line);

            var slider = ours.GetComponentInChildren<Slider>(true);

            var current = getter();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;

            slider.SetValueWithoutNotify(current);
            line.SetValue(ToDisplay(current, displayAs));

            slider.onValueChanged.AddListener(newValue =>
            {
                line.SetValue(ToDisplay(newValue, displayAs));
                setter(newValue);
                AnyValueChanged?.Invoke();
            });
        }

        internal void P12DropdownElement(P12SettableStringElement settableChoice, SettingsMenuInput bpOptionTemp)
        {
            string Getter() => settableChoice.storedValue;
            void Setter(string newValue) => settableChoice.storedValue = newValue;

            Func<string> getter = Getter;
            Action<string> setter = Setter;
            var ours = UnityEngine.Object.Instantiate(_prefabs.dropdown, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(settableChoice.englishTitle);
            SetupLine(line);

            var dropdown = ours.GetComponentInChildren<TMP_Dropdown>(true);

            var dropdownOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var realValue in bpOptionTemp.RealValues)
            {
                dropdownOptions.Add(new TMP_Dropdown.OptionData(realValue));
            }
            dropdown.options = dropdownOptions;

            var current = getter();

            dropdown.SetValueWithoutNotify(bpOptionTemp.RealValues.IndexOf(settableChoice.storedValue));
            line.SetValue($"{current}");

            dropdown.onValueChanged.AddListener(newIndex =>
            {
                var realNewValue = bpOptionTemp.RealValues[newIndex];
                Debug.Log($"value changed to {newIndex} ({realNewValue})");
                line.SetValue($"{realNewValue}");
                setter(realNewValue);
                AnyValueChanged?.Invoke();
            });
        }

        internal void P12ToggleForFloat(P12SettableFloatElement settableChoice)
        {
            bool Getter() => settableChoice.storedValue >= 1f;
            void Setter(bool newValue) => settableChoice.storedValue = newValue ? 0f : 1f;

            Func<bool> getter = Getter;
            Action<bool> setter = Setter;
            var ours = UnityEngine.Object.Instantiate(_prefabs.toggle, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(settableChoice.englishTitle);
            SetupLine(line);

            var dropdown = ours.GetComponentInChildren<Toggle>(true);

            var current = getter();

            var truthness = current;

            dropdown.SetIsOnWithoutNotify(truthness);
            line.SetValue(truthness ? EnglishOnLabel : EnglishOffLabel);

            dropdown.onValueChanged.AddListener(newTruthness =>
            {
                line.SetValue(newTruthness ? EnglishOnLabel : EnglishOffLabel);
                setter(newTruthness);
                AnyValueChanged?.Invoke();
            });
        }

        internal void P12ToggleForDropdown(P12SettableStringElement settableChoice, SettingsMenuInput bpOptionTemp)
        {
            string Getter() => settableChoice.storedValue;
            void Setter(string newValue) => settableChoice.storedValue = newValue;

            Func<string> getter = Getter;
            Action<string> setter = Setter;
            var ours = UnityEngine.Object.Instantiate(_prefabs.toggle, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(settableChoice.englishTitle);
            SetupLine(line);

            var dropdown = ours.GetComponentInChildren<Toggle>(true);

            var current = getter();

            var truthness = current == "true";

            dropdown.SetIsOnWithoutNotify(truthness);
            line.SetValue(truthness ? EnglishOnLabel : EnglishOffLabel);

            dropdown.onValueChanged.AddListener(newTruthness =>
            {
                var realNewValue = bpOptionTemp.RealValues[newTruthness ? 0 : 1];
                Debug.Log($"value changed to {newTruthness} ({realNewValue})");
                line.SetValue(newTruthness ? EnglishOnLabel : EnglishOffLabel);
                setter(realNewValue);
                AnyValueChanged?.Invoke();
            });
        }

        private string ToDisplay(float value, P12SettableFloatElement.P12UnitDisplayKind displayAs)
        {
            switch (displayAs)
            {
                case P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat:
                    return $"{value:0.00}";
                case P12SettableFloatElement.P12UnitDisplayKind.Percentage01:
                    return $"{(value * 100):0}%";
                case P12SettableFloatElement.P12UnitDisplayKind.Percentage080:
                    return $"{((value / 80f) * 100f):0}%";
                case P12SettableFloatElement.P12UnitDisplayKind.Percentage0100:
                    return $"{value:0}%";
                case P12SettableFloatElement.P12UnitDisplayKind.AngleDegrees:
                {
                    if (value < 0) return EnglishOffLabel;
                    return $"{value:0} deg";
                }
                case P12SettableFloatElement.P12UnitDisplayKind.InGameRangeUnityUnits:
                    return $"{value:0.0}m";
                case P12SettableFloatElement.P12UnitDisplayKind.RealWorldPhysicalSpaceMetricUnits:
                    return $"{value:0.0}m";
                case P12SettableFloatElement.P12UnitDisplayKind.RealWorldPhysicalSpaceImperialUnits:
                    return $"{value:0.0}TODO-ft";
                default:
                    throw new ArgumentOutOfRangeException(nameof(displayAs), displayAs, null);
            }
        }
    }
}
