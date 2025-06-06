using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxySubmitSettableToAcquisition : MonoBehaviour
    {
        public string address = "";

        [EarlyInjectable] public P12SettableFloatElement sample;
        [LateInjectable] public AcquisitionService acquisitionService;

        private void Awake()
        {
            acquisitionService = AcquisitionService.SceneInstance;
            sample.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(float value)
        {
            acquisitionService.Submit(address, value);
        }
    }
}
