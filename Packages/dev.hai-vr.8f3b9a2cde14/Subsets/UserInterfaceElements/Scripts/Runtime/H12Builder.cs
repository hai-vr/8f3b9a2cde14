﻿using System;
using System.Collections.Generic;
using Basis.Scripts.Drivers;
using BattlePhaze.SettingsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Hai.Project12.UserInterfaceElements.Runtime.H12Localization;
using Object = UnityEngine.Object;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    internal class H12Builder
    {
        private readonly P12UIEScriptedPrefabs _prefabs;
        private readonly P12UIHaptics _haptics;
        private readonly Transform _layoutGroupHolder;
        private readonly Transform _titleGroupHolder;
        private readonly float _controlExpansion;

        public event ValueChanged OnAnyValueChanged;
        public delegate void ValueChanged();

        public H12Builder(P12UIEScriptedPrefabs prefabs, Transform layoutGroupHolder, Transform titleGroupHolder, float controlExpansion, P12UIHaptics haptics)
        {
            _prefabs = prefabs;
            _layoutGroupHolder = layoutGroupHolder;
            _titleGroupHolder = titleGroupHolder;
            _controlExpansion = controlExpansion;
            _haptics = haptics;
        }

        private void SetupLine(P12UILine line)
        {
            line.SetControlExpansion(_controlExpansion);
        }

        internal P12UILine P12CenteredButton(string rawTitle, Action clickFn)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.titleButton, _layoutGroupHolder);
            ours.name = $"P12B-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);
            SetupLine(line);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                clickFn();
                PlayClickAudio();
                _haptics.Click();
            });

            return line;
        }

        internal P12UILine P12SingularButton(string rawTitle, string rawButtonLabel, Action clickFn)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.singularButton, _layoutGroupHolder);
            ours.name = $"P12B-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);
            line.SetValue(rawButtonLabel);
            SetupLine(line);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                clickFn();
                _haptics.Click();
            });

            return line;
        }

        internal P12UILine P12TitleButton(string rawTitle, Action clickFn)
        {
            var ours = UnityEngine.Object.Instantiate(_prefabs.titleButton, _titleGroupHolder);
            ours.name = $"P12TB-{rawTitle}";

            Object.Destroy(ours.GetComponent<P12AnimButton>());

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);
            SetupLine(line);
            line.SetFocused(false);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                clickFn();
                PlayClickAudio();
                _haptics.Click();
            });

            return line;
        }

        internal void P12SliderElement(P12SettableFloatElement settableFloat)
        {
            float Getter() => settableFloat.storedValue;
            void Setter(float newValue) => settableFloat.storedValue = newValue;

            Internal_SetupSlider(Getter, Setter, settableFloat.localizedTitle, settableFloat.min, settableFloat.max, false, settableFloat.displayAs);
        }

        internal void P12SliderElement(P12SettableIntElement settableInt)
        {
            float Getter() => settableInt.storedValue;
            void Setter(float newValue) => settableInt.storedValue = (int)newValue;

            Internal_SetupSlider(Getter, Setter, settableInt.localizedTitle, settableInt.min, settableInt.max, true, P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat);
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
                OnAnyValueChanged?.Invoke();
            });
        }

        internal P12UILine P12DropdownElement(P12SettableStringElement settableChoice, SettingsMenuInput bpOptionTemp_nullable, bool ignoreIncompleteLocalization)
        {
            string Getter() => settableChoice.storedValue;
            void Setter(string newValue) => settableChoice.storedValue = newValue;

            Func<string> getter = Getter;
            Action<string> setter = Setter;
            var ours = UnityEngine.Object.Instantiate(_prefabs.dropdown, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(settableChoice.localizedTitle);
            SetupLine(line);

            var dropdown = ours.GetComponentInChildren<TMP_Dropdown>(true);

            var dropdownOptions = new List<TMP_Dropdown.OptionData>();
            if (bpOptionTemp_nullable != null)
            {
                foreach (var battlePhazeName in bpOptionTemp_nullable.RealValues)
                {
                    var locKeyNullableWhenLocalizationIncomplete = H12BattlePhazeNameToLocalizationKey.GetKeyOrNull(battlePhazeName);
                    string dropdownTitle;
                    if (locKeyNullableWhenLocalizationIncomplete == null)
                    {
                        if (!ignoreIncompleteLocalization && !int.TryParse(battlePhazeName, out _)) BasisDebug.LogError($"Localization is incomplete: {battlePhazeName} has no match.");
                        dropdownTitle = battlePhazeName;
                    }
                    else
                    {
                        dropdownTitle = _L(locKeyNullableWhenLocalizationIncomplete);
                    }
                    dropdownOptions.Add(new TMP_Dropdown.OptionData(dropdownTitle));
                }
            }
            dropdown.options = dropdownOptions;

            var current = getter();

            if (bpOptionTemp_nullable != null)
            {
                dropdown.SetValueWithoutNotify(bpOptionTemp_nullable.RealValues.IndexOf(settableChoice.storedValue));
            }
            line.SetValue($"{current}");

            if (bpOptionTemp_nullable != null)
            {
                dropdown.onValueChanged.AddListener(newIndex =>
                {
                    var realNewValue = bpOptionTemp_nullable.RealValues[newIndex];
                    line.SetValue($"{realNewValue}");
                    setter(realNewValue);
                    OnAnyValueChanged?.Invoke();
                });
            }

            return line;
        }

        internal void P12ToggleForFloat(P12SettableFloatElement settableChoice)
        {
            bool Getter() => settableChoice.storedValue >= 1f;
            void Setter(bool newValue) => settableChoice.storedValue = newValue ? 1f : 0f;

            Func<bool> getter = Getter;
            Action<bool> setter = Setter;
            var ours = UnityEngine.Object.Instantiate(_prefabs.toggle, _layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(settableChoice.localizedTitle);
            SetupLine(line);

            var dropdown = ours.GetComponentInChildren<Toggle>(true);

            var current = getter();

            var truthness = current;

            dropdown.SetIsOnWithoutNotify(truthness);
            line.SetValue(truthness ? _L("ui.settings.dropdown.on") : _L("ui.settings.dropdown.off"));

            dropdown.onValueChanged.AddListener(newTruthness =>
            {
                line.SetValue(newTruthness ? _L("ui.settings.dropdown.on") : _L("ui.settings.dropdown.off"));
                setter(newTruthness);
                OnAnyValueChanged?.Invoke();
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
            line.SetTitle(settableChoice.localizedTitle);
            SetupLine(line);

            var toggle = ours.GetComponentInChildren<Toggle>(true);

            var current = getter();

            var truthness = current == "true";

            toggle.SetIsOnWithoutNotify(truthness);
            line.SetValue(truthness ? _L("ui.settings.dropdown.on") : _L("ui.settings.dropdown.off"));

            toggle.onValueChanged.AddListener(newTruthness =>
            {
                var realNewValue = bpOptionTemp.RealValues[newTruthness ? 0 : 1];
                line.SetValue(newTruthness ? _L("ui.settings.dropdown.on") : _L("ui.settings.dropdown.off"));
                setter(realNewValue);
                OnAnyValueChanged?.Invoke();
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
                    if (value < 0) return _L("ui.settings.dropdown.off");
                    return $"{value:0} deg";
                }
                case P12SettableFloatElement.P12UnitDisplayKind.InGameRangeUnityUnits:
                    return $"{value:0.0}m";
                case P12SettableFloatElement.P12UnitDisplayKind.RealWorldPhysicalSpaceMetricUnits:
                    return $"{value:0.0}m";
                case P12SettableFloatElement.P12UnitDisplayKind.RealWorldPhysicalSpaceImperialUnits:
                    return $"{value:0.0}TODO-ft";
                case P12SettableFloatElement.P12UnitDisplayKind.Toggle:
                    return value >= 1f ? _L("ui.settings.dropdown.on") : _L("ui.settings.dropdown.off");
                default:
                    throw new ArgumentOutOfRangeException(nameof(displayAs), displayAs, null);
            }
        }

        private void PlayClickAudio()
        {
            BasisLocalCameraDriver.Instance.AudioSource.PlayOneShot(_prefabs.clickAudio);
        }
    }
}
