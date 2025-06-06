using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.InteractionElements.Runtime
{
    public class P12GadgetMenuItem : MonoBehaviour
    {
        [EarlyInjectable] public P12SettableFloatElement element;
        [LateInjectable] [SerializeField] private P12GadgetRepository repository;
        public bool isToggle;

        // private P12SettableFloatElement _element;

        private void Awake()
        {
            H12LateInjector.InjectDependenciesInto(this);

            if (element == null)
            {
                element = ScriptableObject.CreateInstance<P12SettableFloatElement>();
                element.localizedTitle = gameObject.name;
                element.min = 0f;
                element.max = 1f;
                element.displayAs = isToggle ? P12SettableFloatElement.P12UnitDisplayKind.Toggle : P12SettableFloatElement.P12UnitDisplayKind.Percentage01;
            }
            element.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(float newValue)
        {
            transform.localPosition = Vector3.up * newValue;
        }

        private void OnEnable()
        {
            repository.Add(element);
        }

        private void OnDisable()
        {
            repository.Remove(element);
        }

        private void OnDestroy()
        {
            element.OnValueChanged -= OnValueChanged;
        }
    }
}
