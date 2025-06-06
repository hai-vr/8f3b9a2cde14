using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using HVR.Basis.Comms;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxySampleActuator : MonoBehaviour, I12VixxyActuator
    {
        [EarlyInjectable] public P12VixxyOrchestrator orchestrator;
        [EarlyInjectable] public P12SettableFloatElement sample;
        [EarlyInjectable] public MeshRenderer renderer;
        [LateInjectable] public AcquisitionService acquisitionService;

        // Cannot change after the component gets enabled.
        public string address = "";

        private MaterialPropertyBlock _propertyBlock;
        private string _registeredAddress;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            acquisitionService = AcquisitionService.SceneInstance;
        }

        private void OnEnable()
        {
            _registeredAddress = address;
            orchestrator.RegisterActuator(_registeredAddress, this);
            acquisitionService.RegisterAddresses(new []{ _registeredAddress }, OnAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterActuator(_registeredAddress, this);
            acquisitionService.UnregisterAddresses(new []{ _registeredAddress }, OnAddressUpdated);
        }

        private void OnAddressUpdated(string whichAddress, float value)
        {
            // FIXME: Storing that value is probably not a good idea to do at this specific stage of the processing.
            //           For comparison, we can't do this for aggregators (which can have multiple input values), it's not their responsibility.
            sample.storedValue = value;

            orchestrator.PassAddressUpdated(whichAddress);
        }

        public void Actuate()
        {
            var color = Color.Lerp(Color.white, Color.green, sample.storedValue);
            _propertyBlock.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(_propertyBlock);
            // BasisDebug.Log($"Setting color to {H12Debug.ColorAsStartTag(color)}this color</color>.");
        }
    }
}
