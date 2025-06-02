using System.Globalization;
using BattlePhaze.SettingsManager;

namespace Hai.Project12.UserInterfaceElements
{
    public class H12BattlePhazeSettingsHandler
    {
        private readonly SettingsManager BPMgr;

        public H12BattlePhazeSettingsHandler(SettingsManager bpMgr)
        {
            BPMgr = bpMgr;
        }

        public void SaveAndSubmitFloatToBPManager(SettingsMenuInput option, float newValue)
        {
            option.SelectedValue = newValue.ToString(CultureInfo.InvariantCulture);
            SettingsManagerStorageManagement.Save(BPMgr);

            // HACK: SendOption may require accessing the original sliders
            var options = SettingsManager.Instance.Options;
            var bpInput = option.OptionIndex >= options.Count ? options[option.OptionIndex].ObjectInput : null;
            if (bpInput is UnityEngine.UI.Slider slider)
            {
                slider.value = newValue;
            }
            else
            {
                int Index = BPMgr.FindOrAddOption(option.OptionIndex);
                SMWorkAround WorkAround = BPMgr.WorkArounds[Index];
                WorkAround.SelectedValue = option.SelectedValue;
            }
            BPMgr.SendOption(option);
        }

        public void SaveAndSubmitStringToBPManager(SettingsMenuInput option, string newValue)
        {
            option.SelectedValue = newValue;
            SettingsManagerStorageManagement.Save(BPMgr);
            BPMgr.SendOption(option);
        }

        public SettingsMenuInput FindOptionByNameOrNull(string optionName)
        {
            // FIXME: Don't iterate like this just to find the option that matches the name
            foreach (var option in SettingsManager.Instance.Options)
            {
                if (option.Name == optionName)
                {
                    return option;
                }
            }

            return null;
        }

        public float ParseFloat(string valueDefault)
        {
            return float.Parse(valueDefault, CultureInfo.InvariantCulture);
        }
    }
}
