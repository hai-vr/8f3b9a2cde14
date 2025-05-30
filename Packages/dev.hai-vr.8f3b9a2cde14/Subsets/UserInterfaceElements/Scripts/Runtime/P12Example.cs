using System;
using System.Collections.Generic;
using BattlePhaze.SettingsManager;
using UnityEngine;
using UnityEngine.UI;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12Example : MonoBehaviour
    {
        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;

        public event Action AnyValueChanged;

        public P12SettableFloatElement settableFloatSliderA;
        public P12SettableFloatElement settableFloatSliderB;
        public P12SettableIntElement settableIntSlider;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _videoOptions;

        private void Start()
        {
            _audioOptions = new List<string>(new[]
            {
                "Main Audio",
                "Menus Volume",
                "World Volume",
                "Player Volume",
                "Microphone Volume",
                "Microphone Range",
                "Hearing Range",
            });
            _controlsOptions = new List<string>(new[]
            {
                "Controller DeadZone",
                "Snap Turn Angle"
            });
            _videoOptions = new List<string>(new[]
            {
                "Render Resolution",
                "Foveated Rendering Level",
                "Field Of View",
                "Maximum Avatar Distance"
            });

            P12TitleButton("Gadgets", () => Click(P12ExampleCategory.Gadgets));
            P12TitleButton("Actions", () => Click(P12ExampleCategory.Actions));
            P12TitleButton("Audio", () => Click(P12ExampleCategory.Audio));
            P12TitleButton("Video", () => Click(P12ExampleCategory.Video));
            P12TitleButton("Controls", () => Click(P12ExampleCategory.Controls));
            Click(P12ExampleCategory.Audio);
        }

        private void Click(P12ExampleCategory exampleCategory)
        {
            Debug.Log($"Clicked on {exampleCategory}");
            foreach (var comp in layoutGroupHolder.GetComponentsInChildren<P12UILine>())
            {
                if (comp) Destroy(comp.gameObject);
            }
            // while (layoutGroupHolder.childCount > 0)
            // {
            //     Destroy(layoutGroupHolder.GetChild(0).gameObject);
            // }

            switch (exampleCategory)
            {
                case P12ExampleCategory.Audio:
                    CreateOptionsFor(_audioOptions);
                    break;
                case P12ExampleCategory.Controls:
                    CreateOptionsFor(_controlsOptions);
                    break;
                case P12ExampleCategory.Video:
                    CreateOptionsFor(_videoOptions);
                    break;
                case P12ExampleCategory.Actions:
                    CreateActions();
                    break;
            }
        }

        private void CreateActions()
        {
            P12Button("Switch to VR", () => { });
            P12Button("Switch to Desktop", () => { });
            P12Button("Console", () => { });
            P12Button("Admin", () => { });
        }

        private void CreateOptionsFor(List<string> elements)
        {
            foreach (var element in elements)
            {
                CreateOptionFor(element);
            }
        }

        private void CreateOptionFor(string optionName)
        {
            foreach (var option in SettingsManager.Instance.Options)
            {
                if (option.Name == optionName)
                {
                    CreateLineFor(option);
                    break;
                }
            }
        }

        private void CreateLineFor(SettingsMenuInput option)
        {
            if (option.Type == SettingsManagerEnums.IsType.Slider)
            {
                var minValue = float.Parse(option.SliderMinValue);
                var maxValue = float.Parse(option.SliderMaxValue);

                var isPercentage0100 = maxValue == 100f;
                var isAngle = maxValue == 90f;

                var so = ScriptableObject.CreateInstance<P12SettableFloatElement>();
                // so.englishTitle = $"{option.Name} ({minValue} -> {maxValue})";
                so.englishTitle = $"{option.Name}";
                so.min = minValue;
                so.max = maxValue;
                so.defaultValue = float.Parse(option.SelectedValue);
                so.storedValue = float.Parse(option.SelectedValue);
                so.displayAs = isPercentage0100
                    ? P12SettableFloatElement.P12UnitDisplayKind.Percentage0100
                    : isAngle ? P12SettableFloatElement.P12UnitDisplayKind.AngleDegrees : P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat;
                P12SliderElement(so);
            }
        }

        private void P12Button(string rawTitle, Action clickFn)
        {
            var ours = Instantiate(prefabs.titleButton, layoutGroupHolder);
            ours.name = $"P12B-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => clickFn());
        }

        private void P12TitleButton(string rawTitle, Action clickFn)
        {
            var ours = Instantiate(prefabs.titleButton, titleGroupHolder);
            ours.name = $"P12TB-{rawTitle}";

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawTitle);

            var btn = ours.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() => clickFn());
        }

        private void P12SliderElement(P12SettableFloatElement settableFloat)
        {
            float Getter() => settableFloat.storedValue;
            void Setter(float newValue) => settableFloat.storedValue = newValue;

            Internal_SetupSlider(Getter, Setter, settableFloat.englishTitle, settableFloat.min, settableFloat.max, false, settableFloat.displayAs);
        }

        private void P12SliderElement(P12SettableIntElement settableInt)
        {
            float Getter() => settableInt.storedValue;
            void Setter(float newValue) => settableInt.storedValue = (int)newValue;

            Internal_SetupSlider(Getter, Setter, settableInt.englishTitle, settableInt.min, settableInt.max, true, P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat);
        }

        private void Internal_SetupSlider(Func<float> getter, Action<float> setter, string rawString, float min, float max, bool wholeNumbers, P12SettableFloatElement.P12UnitDisplayKind displayAs)
        {
            var ours = Instantiate(prefabs.slider, layoutGroupHolder);

            var line = ours.GetComponent<P12UILine>();
            line.SetTitle(rawString);

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

        private string ToDisplay(float value, P12SettableFloatElement.P12UnitDisplayKind displayAs)
        {
            switch (displayAs)
            {
                case P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat:
                    return $"{value:0.00}";
                case P12SettableFloatElement.P12UnitDisplayKind.Percentage01:
                    return $"{(value * 100):0}%";
                case P12SettableFloatElement.P12UnitDisplayKind.Percentage0100:
                    return $"{value:0}%";
                case P12SettableFloatElement.P12UnitDisplayKind.AngleDegrees:
                {
                    if (value < 0) return "Disabled";
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

    internal enum P12ExampleCategory
    {
        Gadgets,
        Actions,
        Controls,
        Audio,
        Video
    }
}
