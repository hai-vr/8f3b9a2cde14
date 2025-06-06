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
        [SerializeField] private string address = "";
        [SerializeField] private Color whenInactive = Color.white;
        [SerializeField] private Color whenActive = Color.green;

        private MaterialPropertyBlock _propertyBlock;
        private H12ActuatorRegistrationToken _registeredActuator;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            acquisitionService = AcquisitionService.SceneInstance;

            if (string.IsNullOrEmpty(address))
            {
                H12Debug.LogWarning($"{nameof(P12VixxySampleActuator)} actuator named \"{name}\" has no address defined. It will be disabled.", H12Debug.LogTag.Vixxy);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _registeredActuator = orchestrator.RegisterActuator(address, this, OnAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterActuator(_registeredActuator);
            _registeredActuator = default;
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
            var color = Color.Lerp(whenInactive, whenActive, sample.storedValue);
            _propertyBlock.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
