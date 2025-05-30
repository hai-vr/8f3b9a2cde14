using System.Collections.Generic;
using System.Linq;
using BattlePhaze.SettingsManager;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12Example : MonoBehaviour
    {
        private const string SpecialPrefix = "//SPECIAL/";
        private const string SnapTurn = SpecialPrefix + "SnapTurn";
        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _videoOptions;

        private P12UILine _lineGadgets;
        private P12UILine _lineActions;
        private P12UILine _lineAudio;
        private P12UILine _lineVideo;
        private P12UILine _lineControls;

        private H12Builder _h12builder;

        private void Start()
        {
            Debug.Log(string.Join(",", SettingsManager.Instance.Options
                .Where(input => input.Type == SettingsManagerEnums.IsType.Dynamic)
                .Select(input => input.Name)
            ));

            _h12builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, 1.75f);

            _audioOptions = new List<string>(new[]
            {
                "Main Audio", // Slider
                "Menus Volume", // Slider
                "World Volume", // Slider
                "Player Volume", // Slider
                "Microphone Volume", // Slider
                "Microphone Range", // Slider
                "Hearing Range", // Slider

                "Microphone Denoiser" // Dropdown
            });
            _controlsOptions = new List<string>(new[]
            {
                "Controller DeadZone", // Slider
                SnapTurn, // Special
                "Snap Turn Angle" // Slider
            });
            _videoOptions = new List<string>(new[]
            {
                "Maximum Avatar Distance", // Slider
                "Debug Visuals", // Slider

                "Render Resolution", // Slider

                "Master Quality", // Dropdown
                "Quality Level", // Dropdown
                "Texture Quality", // Dropdown
                "Shadow Quality", // Dropdown
                "Volumetric Quality", // Dropdown
                "HDR Support", // Dropdown
                "Terrain Quality", // Dropdown
                "UpScaler", // Dropdown
                "Antialiasing", // Dropdown

                "Foveated Rendering Level", // Slider
                "Field Of View", // Slider
            });

            _lineGadgets = _h12builder.P12TitleButton("Gadgets", () => Click(P12ExampleCategory.Gadgets));
            _lineActions = _h12builder.P12TitleButton("Actions", () => Click(P12ExampleCategory.Actions));
            _lineAudio = _h12builder.P12TitleButton("Audio", () => Click(P12ExampleCategory.Audio));
            _lineVideo = _h12builder.P12TitleButton("Video", () => Click(P12ExampleCategory.Video));
            _lineControls = _h12builder.P12TitleButton("Controls", () => Click(P12ExampleCategory.Controls));
            Click(P12ExampleCategory.Audio);
        }

        private void Click(P12ExampleCategory exampleCategory)
        {
            Debug.Log($"Clicked on {exampleCategory}");
            foreach (var comp in layoutGroupHolder.GetComponentsInChildren<P12UILine>())
            {
                if (comp) Destroy(comp.gameObject);
            }

            // TODO: Iterate
            _lineGadgets.SetFocused(false);
            _lineActions.SetFocused(false);
            _lineAudio.SetFocused(false);
            _lineVideo.SetFocused(false);
            _lineControls.SetFocused(false);

            switch (exampleCategory)
            {
                case P12ExampleCategory.Gadgets:
                    _lineGadgets.SetFocused(true);
                    CreateGadgets();
                    break;
                case P12ExampleCategory.Audio:
                    _lineAudio.SetFocused(true);
                    CreateOptionsFor(_audioOptions);
                    break;
                case P12ExampleCategory.Controls:
                    _lineControls.SetFocused(true);
                    CreateOptionsFor(_controlsOptions);
                    break;
                case P12ExampleCategory.Video:
                    _lineVideo.SetFocused(true);
                    CreateOptionsFor(_videoOptions);
                    break;
                case P12ExampleCategory.Actions:
                    _lineActions.SetFocused(true);
                    CreateActions();
                    break;
            }
        }

        private void CreateGadgets()
        {
            _h12builder.P12ToggleForFloat(new P12SettableFloatElement { englishTitle = "Eye Tracking", storedValue = 1f});
            _h12builder.P12ToggleForFloat(new P12SettableFloatElement { englishTitle = "Face Tracking", storedValue = 0f});
            _h12builder.P12SliderElement(new P12SettableFloatElement { englishTitle = "Unlit" });
            _h12builder.P12SliderElement(new P12SettableFloatElement { englishTitle = "Blush", storedValue = 0.6f });
        }

        private void CreateActions()
        {
            _h12builder.P12Button("Switch to VR", () => { });
            _h12builder.P12Button("Switch to Desktop", () => { });
            _h12builder.P12Button("Console", () => { });
            _h12builder.P12Button("Admin", () => { });
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
            if (optionName.StartsWith(SpecialPrefix))
            {
                if (optionName == SnapTurn)
                {
                    _h12builder.P12Button("Snap Turn", () => { });
                    _h12builder.P12Button("Smooth Turn", () => { });
                    _h12builder.P12Button("Do Not Turn", () => { });
                }
            }

            // FIXME: Don't iterate like this just to find the option that matches the name
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

                var isPercentage01 = minValue == 0f && maxValue == 1f;
                var isPercentage0100 = maxValue == 100f;
                var isPercentage080 = maxValue == 80f;
                var isAngle = maxValue == 90f;

                var so = ScriptableObject.CreateInstance<P12SettableFloatElement>();
                // so.englishTitle = $"{option.Name} ({minValue} -> {maxValue})";
                so.englishTitle = $"{option.Name}";
                so.min = minValue;
                so.max = maxValue;
                so.defaultValue = float.Parse(option.ValueDefault);
                so.storedValue = float.Parse(option.SelectedValue);
                so.displayAs =
                    isPercentage01 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage01
                    : isPercentage0100 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage0100
                    : isPercentage080 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage080
                    : isAngle ? P12SettableFloatElement.P12UnitDisplayKind.AngleDegrees
                    : P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat;
                _h12builder.P12SliderElement(so);
            }
            else if (option.Type == SettingsManagerEnums.IsType.DropDown)
            {
                var so = ScriptableObject.CreateInstance<P12SettableStringElement>();
                so.englishTitle = $"{option.Name}";
                so.defaultValue = option.ValueDefault;
                so.storedValue = option.SelectedValue;
                var isToggleForDropdown = option.RealValues.Count == 2 && option.RealValues[0] == "true";
                if (isToggleForDropdown)
                {
                    _h12builder.P12ToggleForDropdown(so, option);
                }
                else
                {
                    _h12builder.P12DropdownElement(so, option);
                }
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
