using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.InteractionElements.Runtime
{
    public class P12GadgetMenuItem : MonoBehaviour
    {
        [LateInjectable] [SerializeField] private P12GadgetRepository repository;
        public bool isToggle;

        private P12SettableFloatElement _element;

        private void Awake()
        {
            H12LateInjector.InjectDependenciesInto(this);

            _element = ScriptableObject.CreateInstance<P12SettableFloatElement>();
            _element.localizedTitle = gameObject.name;
            _element.min = 0f;
            _element.max = 1f;
            _element.OnValueChanged += OnValueChanged;
            _element.displayAs = isToggle ? P12SettableFloatElement.P12UnitDisplayKind.Toggle : P12SettableFloatElement.P12UnitDisplayKind.Percentage01;
        }

        private void OnValueChanged(float newValue)
        {
            transform.localPosition = Vector3.up * newValue;
        }

        private void OnEnable()
        {
            repository.Add(_element);
        }

        private void OnDisable()
        {
            repository.Remove(_element);
        }
    }
}
