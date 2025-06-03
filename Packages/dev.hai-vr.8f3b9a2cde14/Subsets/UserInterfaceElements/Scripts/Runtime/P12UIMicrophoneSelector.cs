using System.Collections.Generic;
using Basis.Scripts.Device_Management;
using TMPro;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12UIMicrophoneSelector : MonoBehaviour
    {
        // See BasisMicrophoneSelection.class

        public TMP_Dropdown Dropdown;

        public void Awake()
        {
            Dropdown.onValueChanged.AddListener(ApplyChanges);
            BasisDeviceManagement.OnBootModeChanged += OnBootModeChanged;
            GenerateUI();
        }

        public void OnDestroy()
        {
            BasisDeviceManagement.OnBootModeChanged -= OnBootModeChanged;
        }

        public void GenerateUI()
        {
            SMDMicrophone.LoadInMicrophoneData(BasisDeviceManagement.Instance.CurrentMode);
            Dropdown.ClearOptions();
            var TmpOptions = new List<TMP_Dropdown.OptionData>();

            foreach (string device in SMDMicrophone.MicrophoneDevices)
            {
                var option = new TMP_Dropdown.OptionData(device);
                TmpOptions.Add(option);
            }

            Dropdown.AddOptions(TmpOptions);
            Dropdown.value = MicrophoneToValue(SMDMicrophone.SelectedMicrophone);
        }

        public int MicrophoneToValue(string Active)
        {
            for (var Index = 0; Index < Dropdown.options.Count; Index++)
            {
                var optionData = Dropdown.options[Index];
                if (Active == optionData.text) return Index;
            }

            return 0;
        }

        private void OnBootModeChanged(string obj)
        {
            GenerateUI();
        }

        private void ApplyChanges(int index)
        {
            SMDMicrophone.SaveMicrophoneData(BasisDeviceManagement.Instance.CurrentMode, SMDMicrophone.MicrophoneDevices[index]);
        }
    }
}
