using System.Collections.Generic;
using System.Linq;
using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Enums;
using Basis.Scripts.Device_Management;
using Basis.Scripts.UI.UI_Panels;
using BattlePhaze.SettingsManager;
using TMPro;
using UnityEngine;
using static Hai.Project12.UserInterfaceElements.H12Localization;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12SettingsMenu : MonoBehaviour
    {
        private const string SpecialPrefix = "//SPECIAL/";
        private const string SpecialSnapTurn = SpecialPrefix + "SnapTurn";
        private const string SpecialMicrophone = SpecialPrefix + "Microphone";
        private const string Settings_SnapTurnAngle_Key = "Snap Turn Angle"; // This is not a localization label.
        [SerializeField] private P12UIEScriptedPrefabs prefabs;
        [SerializeField] private P12UIHaptics haptics;
        [SerializeField] private Transform layoutGroupHolder;
        [SerializeField] private Transform titleGroupHolder;

        private List<string> _audioOptions;
        private List<string> _controlsOptions;
        private List<string> _interfaceOptions;
        private List<string> _videoOptions;

        private P12UILine[] _lines;
        private P12UILine _lineGadgets;
        private P12UILine _lineActions;
        private P12UILine _lineAudio;
        private P12UILine _lineVideo;
        private P12UILine _lineInterface;
        private P12UILine _lineControls;

        private H12Builder _h12Builder;
        private H12BattlePhazeSettingsHandler _h12BattlePhazeSettings;

        private void Start()
        {
            SettingsManager.Instance.Initalize(true);

            _h12BattlePhazeSettings = new H12BattlePhazeSettingsHandler(SettingsManager.Instance);
            _h12Builder = new H12Builder(prefabs, layoutGroupHolder, titleGroupHolder, P12MainMenu.StandardControlExpansion, haptics);

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
            _videoOptions = new List<string>(new[]
            {
                "Maximum Avatar Distance", // Slider

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
            _interfaceOptions = new List<string>(new[]
            {
                "Debug Visuals", // Slider
            });
            _controlsOptions = new List<string>(new[]
            {
                "Controller DeadZone", // Slider
                SpecialSnapTurn, // Special
                "Snap Turn Angle" // Slider
            });

            _lineGadgets = _h12Builder.P12TitleButton(_L("ui.settings.menu.gadgets"), () => Click(P12SettingsCategory.Gadgets));
            _lineActions = _h12Builder.P12TitleButton(_L("ui.settings.menu.actions"), () => Click(P12SettingsCategory.Actions));
            _lineAudio = _h12Builder.P12TitleButton(_L("ui.settings.menu.audio"), () => Click(P12SettingsCategory.Audio));
            _lineVideo = _h12Builder.P12TitleButton(_L("ui.settings.menu.video"), () => Click(P12SettingsCategory.Video));
            _lineInterface = _h12Builder.P12TitleButton(_L("ui.settings.menu.interface"), () => Click(P12SettingsCategory.Interface));
            _lineControls = _h12Builder.P12TitleButton(_L("ui.settings.menu.controls"), () => Click(P12SettingsCategory.Controls));
            _lines = new[]
            {
                _lineGadgets,
                _lineActions,
                _lineAudio,
                _lineVideo,
                _lineInterface,
                _lineControls,
            };
            Click(P12SettingsCategory.Audio);
        }

        private void Click(P12SettingsCategory settingsCategory)
        {
            foreach (var comp in layoutGroupHolder.GetComponentsInChildren<P12UILine>())
            {
                if (comp) Destroy(comp.gameObject);
            }

            foreach (var line in _lines)
            {
                line.SetFocused(false);
            }

            switch (settingsCategory)
            {
                case P12SettingsCategory.Gadgets:
                    _lineGadgets.SetFocused(true);
                    CreateGadgets();
                    break;
                case P12SettingsCategory.Actions:
                    _lineActions.SetFocused(true);
                    CreateActions();
                    break;
                case P12SettingsCategory.Audio:
                    _lineAudio.SetFocused(true);
                    CreateOptionsFor(_audioOptions);
                    break;
                case P12SettingsCategory.Video:
                    _lineVideo.SetFocused(true);
                    CreateOptionsFor(_videoOptions);
                    break;
                case P12SettingsCategory.Interface:
                    _lineInterface.SetFocused(true);
                    CreateOptionsFor(_interfaceOptions);
                    _h12Builder.P12SingularButton(_L("ui.settings.option.localization"), _L("ui.settings.action.toggle_localization"), () =>
                    {
                        DebugShowKeysOnly = !DebugShowKeysOnly;
                        Click(P12SettingsCategory.Interface);
                    });
                    break;
                case P12SettingsCategory.Controls:
                    _lineControls.SetFocused(true);
                    CreateOptionsFor(_controlsOptions);
                    break;
            }
        }

        private void CreateGadgets()
        {
            _h12Builder.P12ToggleForFloat(UserProvided_SettableFloatElement("Eye Tracking", 1f));
            _h12Builder.P12ToggleForFloat(UserProvided_SettableFloatElement("Face Tracking", 0f));
            _h12Builder.P12SliderElement(UserProvided_SettableFloatElement("Unlit", 0f));
            _h12Builder.P12SliderElement(UserProvided_SettableFloatElement("Blush", 0.6f));
            _h12Builder.P12SingularButton(LocalizeUserProvidedString("ui.settings.extra.send_netmessage_test", "Send Netmessage Test"), _L("ui.settings.extra.trigger"), () => { });
        }

        private static P12SettableFloatElement UserProvided_SettableFloatElement(string battlePhazeName, float storedValue)
        {
            var localizationKey = BattlePhazeNameToLocalizationKey(battlePhazeName);

            var result = ScriptableObject.CreateInstance<P12SettableFloatElement>();
            result.locKey = localizationKey;
            result.localizedTitle = LocalizeUserProvidedString(result.locKey, battlePhazeName);
            result.storedValue = storedValue;
            return result;
        }

        private static string BattlePhazeNameToLocalizationKey(string battlePhazeName)
        {
            return H12BattlePhazeNameToLocalizationKey.GetKeyOrNull(battlePhazeName) ?? $"ui.unknown.gen.{battlePhazeName.Replace(" ", "").ToLowerInvariant()}";
        }

        private void CreateActions()
        {
            _h12Builder.P12SingularButton(_L("ui.settings.option.vr_mode"), _L("ui.settings.action.switch_to_vr"), () =>
            {
                BasisDeviceManagement.Instance.SwitchMode("OpenVRLoader");
            });
            _h12Builder.P12SingularButton(_L("ui.settings.option.desktop_mode"), _L("ui.settings.action.switch_to_desktop"), () =>
            {
                BasisDeviceManagement.Instance.SwitchMode(BasisDeviceManagement.Desktop);
            });
            _h12Builder.P12SingularButton(_L("ui.settings.option.debug"), _L("ui.settings.action.open_console"), () =>
            {
                // See class: BasisUISettings
                BasisUIManagement.CloseAllMenus();
                AddressableGenericResource resource = new AddressableGenericResource("LoggerUI", AddressableExpectedResult.SingleItem);
                BasisUIBase.OpenMenuNow(resource);
            });
            _h12Builder.P12SingularButton(_L("ui.settings.option.moderation"), _L("ui.settings.action.open_admin_panel"), () =>
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

                    _h12Builder.P12SingularButton(_L("ui.settings.option.turn"), _L("ui.settings.action.snap_turn"), () =>
                    {
                        var currentValue = _h12BattlePhazeSettings.ParseFloat(snapTurnAngleOption.SelectedValue);
                        if (currentValue <= 0f)
                        {
                            var defaultValue = _h12BattlePhazeSettings.ParseFloat(snapTurnAngleOption.ValueDefault);
                            _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, defaultValue);
                        }
                    });
                    _h12Builder.P12SingularButton("", _L("ui.settings.action.smooth_turn"), () =>
                    {
                        _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, -1f);
                    });
                    _h12Builder.P12SingularButton("", _L("ui.settings.action.no_turn"), () =>
                    {
                        _h12BattlePhazeSettings.SaveAndSubmitFloatToBPManager(snapTurnAngleOption, 0f);
                    });
                }
                else if (optionName == SpecialMicrophone)
                {
                    var stringElement = ScriptableObject.CreateInstance<P12SettableStringElement>();
                    stringElement.locKey = "ui.settings.option.microphone";
                    stringElement.localizedTitle = _L("ui.settings.option.microphone");
                    var line = _h12Builder.P12DropdownElement(stringElement, null, false);
                    line.SetControlExpansion(P12MainMenu.StandardControlExpansion * 1.75f);

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
            var localizationKey = BattlePhazeNameToLocalizationKey(option.Name);
            var localizedTitle = _L(localizationKey);
            if (option.Type == SettingsManagerEnums.IsType.Slider)
            {
                var minValue = _h12BattlePhazeSettings.ParseFloat(option.SliderMinValue);
                var maxValue = _h12BattlePhazeSettings.ParseFloat(option.SliderMaxValue);

                var isPercentage01 = minValue == 0f && maxValue == 1f;
                var isPercentage0100 = maxValue == 100f;
                var isPercentage080 = maxValue == 80f;
                var isAngle = maxValue == 90f;

                var so = ScriptableObject.CreateInstance<P12SettableFloatElement>();
                so.locKey = localizationKey;
                so.localizedTitle = localizedTitle;
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
                so.locKey = localizationKey;
                so.localizedTitle = localizedTitle;
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
                    _h12Builder.P12DropdownElement(so, option, option.Type == SettingsManagerEnums.IsType.Dynamic);
                }
            }
        }
    }

    internal enum P12SettingsCategory
    {
        Gadgets,
        Actions,
        Audio,
        Interface,
        Video,
        Controls,
    }
}
