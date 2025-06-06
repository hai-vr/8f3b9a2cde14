using System;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyControl : MonoBehaviour, I12VixxyActuator
    {
        // Licensing notes:
        // Portions of the code below originally comes from portions of a proprietary software that I (Haï~) am the author of,
        // and is notably used in "Vixen" (2023-2024).
        // The code below is released under the same terms of the "Vixxy/" subdirectory that this file is contained in which is NOT DEFINED yet,
        // including the specific portions of the code that originally came from "Vixen".

        /// The orchestrator defines the context that the subjects of this control will affect (e.g. Recursive Search).
        /// Vixxy is not an avatar-specific component, so it needs that limited context.
        [SerializeField] private P12VixxyOrchestrator orchestrator;
        [SerializeField] private string address;
        [SerializeField] private P12SettableFloatElement sample;

        [SerializeField] private P12VixxyActivations[] activations;
        [SerializeField] private P12VixxySubject[] subjects;

        // Runtime only
        private Transform _context;
        private H12ActuatorRegistrationToken _registeredActuator;

        public void Awake()
        {
            _context = orchestrator.Context();
            // TODO: Resolve properties.
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
            // FIXME: We really need to figure out how actuators sample values from their dependents.
            float value = sample.storedValue;
            foreach (var activation in activations)
            {
                switch (activation.threshold)
                {
                    case ActivationThreshold.Blended:
                        Toggle(activation.component, Mathf.Abs(activation.target - value) < 1f);
                        break;
                    case ActivationThreshold.Strict:
                        Toggle(activation.component, Mathf.Approximately(activation.target, value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Toggle(Component component, bool isOn)
        {
            if (component is Transform) component.gameObject.SetActive(isOn);
            else if (component is Behaviour behaviour) behaviour.enabled = isOn;
            // else, there is no effect on other non-Behaviour components.
        }
    }

    [Serializable]
    public struct P12VixxyActivations
    {
        public Component component; // To toggle a GameObject, provide the Transform instead. It makes things easier as GameObject is not a component.
        public ActivationThreshold threshold;
        public float target;
    }

    public enum ActivationThreshold
    {
        /// Is considered to be ON when the absolute difference to the target is strictly smaller than 1.
        /// This is the best choice for stuff like material dissolves, where the object appears before it is even complete, and therefore the default.
        Blended,
        /// Is considered to be ON when the current value is equal to the target value.
        Strict,
    }

    [Serializable]
    public struct P12VixxySubject
    {
        public P12VixxySelection selection;

        // TODO: It may be relevant to create a MonoBehaviour that represents groups of objects that can be referenced multiple times throughout.
        public GameObject[] targets;
        public GameObject[] childrenOf;
        public GameObject[] exceptions;

        // Note: The list of properties may sometimes contain properties that are not shown in the UI,
        // because the first target does not contain the component type referenced by that property.
        //
        // In that case, when the Processor runs, these properties are NOT applied, even if the actual
        // objects being changed do contain the component type.
        // We don't want to apply "ghost" properties that are not visible to the user in the UI.
        //
        // In the case of Vixxy (and not Vixen), we should just prune these properties at runtime.
        public P12VixxyProperty<float>[] propertiesForFloat;
        public P12VixxyProperty<Vector4>[] propertiesForVector4;
        public P12VixxyProperty<Material>[] propertiesForMaterial;
        public P12VixxyProperty<Transform>[] propertiesForTransform;
        public P12VixxyProperty<Texture>[] propertiesForTexture;
    }

    public enum P12VixxySelection
    {
        Normal,
        RecursiveSearch,
        Everything
    }

    [Serializable]
    public struct P12VixxyProperty<T>
    {
        // TODO: It might be relevant to use another approach than getting animatable properties,
        // since we have control over the system. It doesn't have to piggyback on the animation APIs.
        public string fullClassName;
        public string propertyName;

        public bool flip;
        public T bound;
        public T unbound;
    }
}
