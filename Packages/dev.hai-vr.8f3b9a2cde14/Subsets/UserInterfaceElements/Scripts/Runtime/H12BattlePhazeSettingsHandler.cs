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

        public void SaveAndSubmitToBPManager(SettingsMenuInput option, float value)
        {
            SettingsManagerStorageManagement.Save(BPMgr);

            // HACK: SendOption requires editing the original sliders
            var bpInput = SettingsManager.Instance.Options[option.OptionIndex].ObjectInput;
            if (bpInput is UnityEngine.UI.Slider slider)
            {
                slider.value = value;
            }
            else
            {
                int Index = BPMgr.FindOrAddOption(option.OptionIndex);
                SMWorkAround WorkAround = BPMgr.WorkArounds[Index];
                WorkAround.SelectedValue = option.SelectedValue;
            }
            BPMgr.SendOption(option);
        }
    }
}
