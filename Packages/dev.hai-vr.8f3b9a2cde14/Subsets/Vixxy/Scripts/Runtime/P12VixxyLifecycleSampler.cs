using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyLifecycleSampler : MonoBehaviour
    {
        [EarlyInjectable] public P12SettableFloatElement sample;
        [EarlyInjectable] public MeshRenderer renderer;

        private bool _needsUpdateThisTick;
        private MaterialPropertyBlock _propertyBlock;

        private void Awake()
        {
            sample.OnValueChanged += OnValueChanged;
            _propertyBlock = new MaterialPropertyBlock();
            _needsUpdateThisTick = true;
        }

        private void OnValueChanged(float _)
        {
            _needsUpdateThisTick = true;
        }

        private void Update()
        {
            if (!_needsUpdateThisTick) return;
            _needsUpdateThisTick = false;

            Evaluate(sample.storedValue);
        }

        private void Evaluate(float value01)
        {
            var color = Color.Lerp(Color.white, Color.green, value01);
            _propertyBlock.SetColor(Shader.PropertyToID("_BaseColor"), color);
            renderer.SetPropertyBlock(_propertyBlock);
            BasisDebug.Log($"Setting color to <color=#{ColorUtility.ToHtmlStringRGB(color)}>this color</color>.");
        }
    }
}
