using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxySampleActuator : MonoBehaviour, I12Actuator
    {
        [EarlyInjectable] public P12VixxyOrchestrator orchestrator;
        [EarlyInjectable] public P12SettableFloatElement sample;
        [EarlyInjectable] public MeshRenderer renderer;

        private MaterialPropertyBlock _propertyBlock;
        private string _registeredAddress;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            _registeredAddress = P12VixxySubmitSettableToAcquisition.TestAddress;
            orchestrator.RegisterActuator(_registeredAddress, this);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterActuator(_registeredAddress, this);
        }

        public void Actuate()
        {
            var color = Color.Lerp(Color.white, Color.green, sample.storedValue);
            _propertyBlock.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(_propertyBlock);
            BasisDebug.Log($"Setting color to <color=#{ColorUtility.ToHtmlStringRGB(color)}>this color</color>.");
        }
    }
}
