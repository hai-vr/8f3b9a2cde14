using System.Collections.Generic;
using System.Linq;
using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Enums;
using Basis.Scripts.Device_Management;
using Basis.Scripts.UI.UI_Panels;
using BattlePhaze.SettingsManager;
using TMPro;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12SettingsMenu : MonoBehaviour
    {
        private const string SpecialPrefix = "//SPECIAL/";
        private const string SpecialSnapTurn = SpecialPrefix + "SnapTurn";
        private const string SpecialMicrophone = SpecialPrefix + "Microphone";
        private const string Settings_SnapTurnAngle_Key = "Snap Turn Angle"; // This is not a localization label.
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

        private H12Builder _h12Builder;
        private H12BattlePhazeSettingsHandler _h12BattlePhazeSettings;

        private void Start()
        {
            SettingsManager.Instance.Initalize(true);

            _h12BattlePhazeSettings = new H12BattlePhazeSettingsHandler(SettingsManager.Instance);
            _h12Builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, 1.75f);

            // TODO:
            // - Microphone Source
            // Is Dynamic:
            // - Monitor
            // - Resolution
            // - ScreenMode

            BasisDebug.Log(string.Join(",", SettingsManager.Instance.Options
                .Select(input => $"{input.Name}({input.Type})")));

            _audioOptions = new List<string>(new[]
            {
                "Main Audio", // Slider
                "Menus Volume", // Slider
                "World Volume", // Slider
                "Player Volume", // Slider
                SpecialMicrophone, // Special
                "Microphone Denoiser", // Dropdown
                "Microphone Volume", // Slider
                "Microphone Range", // Slider
                "Hearing Range", // Slider
            });
            _controlsOptions = new List<string>(new[]
            {
                "Controller DeadZone", // Slider
                SpecialSnapTurn, // Special
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

                "Memory Allocation", // Dropdown

                "Resolution", // Dynamic
                "ScreenMode", // Dynamic
                "Monitor", // Dynamic
            });

            _lineGadgets = _h12Builder.P12TitleButton("Gadgets", () => Click(P12ExampleCategory.Gadgets));
            _lineActions = _h12Builder.P12TitleButton("Actions", () => Click(P12ExampleCategory.Actions));
            _lineAudio = _h12Builder.P12TitleButton("Audio", () => Click(P12ExampleCategory.Audio));
            _lineVideo = _h12Builder.P12TitleButton("Video", () => Click(P12ExampleCategory.Video));
            _lineControls = _h12Builder.P12TitleButton("Controls", () => Click(P12ExampleCategory.Controls));
            Click(P12ExampleCategory.Audio);
        }

        private void Click(P12ExampleCategory exampleCategory)
        {
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
            _h12Builder.P12ToggleForFloat(SettableFloatElement("Eye Tracking", 1f));
            _h12Builder.P12ToggleForFloat(SettableFloatElement("Face Tracking", 0f));
            _h12Builder.P12SliderElement(SettableFloatElement("Unlit", 0f));
            _h12Builder.P12SliderElement(SettableFloatElement("Blush", 0.6f));
            _h12Builder.P12SingularButton("Send Netmessage Test", "Trigger", () => { });
        }

        private static P12SettableFloatElement SettableFloatElement(string englishTitle, float storedValue)
        {
            var result = ScriptableObject.CreateInstance<P12SettableFloatElement>();
            result.englishTitle = englishTitle;
            result.storedValue = storedValue;
            return result;
        }

        private void CreateActions()
        {
            _h12Builder.P12SingularButton("VR Mode", "Switch to VR", () =>
            {
                BasisDeviceManagement.Instance.SwitchMode("OpenVRLoader");
            });
            _h12Builder.P12SingularButton("Desktop Mode", "Switch to Desktop", () =>
            {
                BasisDeviceManagement.Instance.SwitchMode(BasisDeviceManagement.Desktop);
            });
            _h12Builder.P12SingularButton("Debug", "Open Console", () =>
            {
                // See class: BasisUISettings
                BasisUIManagement.CloseAllMenus();
                AddressableGenericResource resource = new AddressableGenericResource("LoggerUI", AddressableExpectedResult.SingleItem);
                BasisUIBase.OpenMenuNow(resource);
            });
            _h12Builder.P12SingularButton("Moderation", "Open Admin Panel", () =>
            {

                BasisUIManagement.CloseAllMenus();
                AddressableGenericResource resource = new AddressableGenericResource("BasisUIAdminPanel", AddressableExpectedResult.SingleItem);
                BasisUIBase.OpenMenuNow(resource);
            });
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
                if (optionName == SpecialSnapTurn)
                {
                    var snapTurnAngleOption = _h12BattlePhazeSettings.FindOptionByNameOrNull(Settings_SnapTurnAngle_Key);
                    // TODO: Clicking these buttons should reflect back on the slider. Need event listeners

                    _h12Builder.P12SingularButton("Turn", "Snap Turn", () =>
                    {
                        var currentValue = _h12BattlePhazeSettings.ParseFloat(snapTurnAngleOption.SelectedValue);
                        if (currentValue <= 0f)
                        {
                            var defaultValue = _h12BattlePhazeSettings.ParseFloat(snapTurnAngleOption.ValueDefault);
                            _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, defaultValue);
                        }
                    });
                    _h12Builder.P12SingularButton("", "Smooth Turn", () =>
                    {
                        _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, -1f);
                    });
                    _h12Builder.P12SingularButton("", "Do Not Turn", () =>
                    {
                        _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, 0f);
                    });
                }
                else if (optionName == SpecialMicrophone)
                {
                    var stringElement = ScriptableObject.CreateInstance<P12SettableStringElement>();
                    stringElement.englishTitle = "Microphone";
                    var line = _h12Builder.P12DropdownElement(stringElement, null);

                    line.gameObject.SetActive(false);
                    var micSelector = line.gameObject.AddComponent<P12UIMicrophoneSelector>();
                    micSelector.Dropdown = line.GetComponentInChildren<TMP_Dropdown>();
                    line.gameObject.SetActive(true);
                }
            }

            var option = _h12BattlePhazeSettings.FindOptionByNameOrNull(optionName);
            if (option != null)
            {
                CreateLineFor(option);
            }
        }

        private void CreateLineFor(SettingsMenuInput option)
        {
            if (option.Type == SettingsManagerEnums.IsType.Slider)
            {
                var minValue = _h12BattlePhazeSettings.ParseFloat(option.SliderMinValue);
                var maxValue = _h12BattlePhazeSettings.ParseFloat(option.SliderMaxValue);

                var isPercentage01 = minValue == 0f && maxValue == 1f;
                var isPercentage0100 = maxValue == 100f;
                var isPercentage080 = maxValue == 80f;
                var isAngle = maxValue == 90f;

                var so = ScriptableObject.CreateInstance<P12SettableFloatElement>();
                so.englishTitle = $"{option.Name}";
                so.min = minValue;
                so.max = maxValue;
                so.defaultValue = _h12BattlePhazeSettings.ParseFloat(option.ValueDefault);
                so.displayAs =
                    isPercentage01 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage01
                    : isPercentage0100 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage0100
                    : isPercentage080 ? P12SettableFloatElement.P12UnitDisplayKind.Percentage080
                    : isAngle ? P12SettableFloatElement.P12UnitDisplayKind.AngleDegrees
                    : P12SettableFloatElement.P12UnitDisplayKind.ArbitraryFloat;

                so.storedValue = _h12BattlePhazeSettings.ParseFloat(option.SelectedValue);
                // Order matters: Need to declare this event only after we initialized storedValue
                so.OnValueChanged += value => _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(option, value);

                _h12Builder.P12SliderElement(so);
            }
            else if (option.Type == SettingsManagerEnums.IsType.DropDown || option.Type == SettingsManagerEnums.IsType.Dynamic)
            {
                var so = ScriptableObject.CreateInstance<P12SettableStringElement>();
                so.englishTitle = $"{option.Name}";
                so.defaultValue = option.ValueDefault;

                so.storedValue = option.SelectedValue;
                // Order matters: Need to declare this event only after we initialized storedValue
                so.OnValueChanged += value => _h12BattlePhazeSettings.SaveAndSubmitStringToBPManager(option, value);

                var isToggleForDropdown = option.RealValues.Count == 2 && option.RealValues[0] == "true";
                if (isToggleForDropdown)
                {
                    _h12Builder.P12ToggleForDropdown(so, option);
                }
                else
                {
                    _h12Builder.P12DropdownElement(so, option);
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
