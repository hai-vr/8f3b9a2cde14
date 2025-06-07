using System;
using System.Collections.Generic;
using System.Reflection;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyControl : MonoBehaviour, I12VixxyActuator
    {
        private const string PropMaterialPrefix = "material.";
        private const string PropBlendShapePrefix = "blendShape.";
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

        private readonly Dictionary<string, Type> _typeCache_mayContainNullObjects = new();

        public void Awake()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            _context = orchestrator.Context();
            for (var i = 0; i < subjects.Length; i++)
            {
                var subject = subjects[i];
                subject.BakeAffectedObjects(_context);
                subjects[i] = subject;

                var isAnyPropertyDependentOnMaterialPropertyBlock = false;

                // TODO: Bake for other properties too.
                // FIXME: Having different property types is not maintainable, we need different loops that practically do the same thing.
                for (var index = 0; index < subject.propertiesForVector4.Length; index++)
                {
                    var property = subject.propertiesForVector4[index];
                    if (TryGetType(assemblies, property.fullClassName, out var foundType))
                    {
                        // FIXME: This is not always correct, think material swaps and some other subtleties (can't remember which).
                        var affectsMaterialPropertyBlock = property.propertyName.StartsWith(PropMaterialPrefix);

                        // This is just a sanity check:
                        // - If it's not a material property block, it is OK.
                        // - If it *is* a material property block, then the type must be a renderer, so that we can skip the type check at runtime.
                        if (!affectsMaterialPropertyBlock || typeof(Renderer).IsAssignableFrom(foundType))
                        {
                            var foundComponents = new List<Component>();
                            foreach (var bakedObject in subject.BakedObjects)
                            {
                                var component = bakedObject.GetComponent(foundType);
                                if (component != null)
                                {
                                    foundComponents.Add(component);
                                }
                            }

                            if (foundComponents.Count > 0)
                            {
                                property.IsApplicable = true;
                                property.FoundType = foundType;
                                property.FoundComponents = foundComponents;
                                property.AffectsMaterialPropertyBlock = affectsMaterialPropertyBlock;
                                if (affectsMaterialPropertyBlock)
                                {
                                    isAnyPropertyDependentOnMaterialPropertyBlock = true;
                                    property.ShaderMaterialProperty = Shader.PropertyToID(property.propertyName.Substring(PropMaterialPrefix.Length));
                                }

                                // FIXME: This can never be true unless we are passed false data. Blendshapes are floats, not Vector4s
                                if (property.propertyName.StartsWith(PropBlendShapePrefix) && foundType == typeof(SkinnedMeshRenderer))
                                {
                                    property.SpecialMarker = P12SpecialMarker.BlendShape;
                                }
                                else
                                {
                                    property.SpecialMarker = P12SpecialMarker.None;
                                }
                                property.PropertySuffix = property.propertyName.Contains('.') ? property.propertyName.Substring(property.propertyName.IndexOf('.') + 1) : "";
                            }
                            else
                            {
                                property.IsApplicable = false;
                            }
                        }
                        else
                        {
                            property.IsApplicable = false;
                        }
                    }
                    else
                    {
                        property.IsApplicable = false;
                    }

                    subject.propertiesForVector4[index] = property;
                }

                if (isAnyPropertyDependentOnMaterialPropertyBlock)
                {
                    foreach (var bakedObject in subject.BakedObjects)
                    {
                        orchestrator.RequireMaterialPropertyBlock(bakedObject);
                    }
                }
            }
        }

        private bool TryGetType(Assembly[] assemblies, string propertyFullClassName, out Type foundType)
        {
            if (_typeCache_mayContainNullObjects.TryGetValue(propertyFullClassName, out var typeNullable))
            {
                foundType = typeNullable;
                return typeNullable != null;
            }

            foreach (var assembly in assemblies)
            {
                foreach (var thatType in assembly.GetTypes())
                {
                    if (thatType.FullName == propertyFullClassName)
                    {
                        _typeCache_mayContainNullObjects.Add(propertyFullClassName, thatType);

                        foundType = thatType;
                        return true;
                    }
                }
            }

            // We do cache null when we don't find that class, so that we don't try to find that again.
            _typeCache_mayContainNullObjects.Add(propertyFullClassName, null);

            foundType = null;
            return false;
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
            ActuateActivations(value);
            ActuateSubjects(value);
        }

        private void ActuateActivations(float value)
        {
            foreach (var activation in activations)
            {
                switch (activation.threshold)
                {
                    case ActivationThreshold.Blended:
                        H12Utilities.SetToggleState(activation.component, Mathf.Abs(activation.target - value) < 1f);
                        break;
                    case ActivationThreshold.Strict:
                        H12Utilities.SetToggleState(activation.component, Mathf.Approximately(activation.target, value));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ActuateSubjects(float value)
        {
            foreach (var subject in subjects)
            {
                foreach (var property in subject.propertiesForVector4)
                {
                    // TODO: Rather than do that check every time, bake the applicable properties into an internal field.
                    if (!property.IsApplicable) continue;

                    var propertyNeedsCleanup = false;
                    var lerpValue = Color.Lerp(property.unbound, property.bound, value);
                    foreach (var component in property.FoundComponents)
                    {
                        // Defensive check in case of external destruction.
                        if (null != component)
                        {
                            if (property.AffectsMaterialPropertyBlock)
                            {
                                var materialPropertyBlock = orchestrator.GetMaterialPropertyBlockForBakedObject(component.gameObject);
                                materialPropertyBlock.SetColor(property.ShaderMaterialProperty, lerpValue);
                                orchestrator.StagePropertyBlock(component.gameObject);
                            }
                            else
                            {
                                // FIXME: This is slow. Bake it into the property itself in Awake.
                                var fields = property.FoundType.GetFields();
                                foreach (var fieldInfo in fields)
                                {
                                    if (fieldInfo.Name == property.propertyName)
                                    {
                                        // TODO: Cast to the type that this field expects
                                        fieldInfo.SetValue(component, lerpValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            propertyNeedsCleanup = true;
                        }
                    }

                    if (propertyNeedsCleanup) ConsiderCleaningUpProperty(property.FoundComponents);
                }

                // FIXME: Having a different foreach loop for each type is pure annoyance. This is not maintainable, find a way to bake it into one loop.
                foreach (var property in subject.propertiesForFloat)
                {
                    // TODO: Rather than do that check every time, bake the applicable properties into an internal field.
                    if (!property.IsApplicable) continue;

                    var propertyNeedsCleanup = false;
                    var lerpValue = Mathf.Lerp(property.unbound, property.bound, value);
                    foreach (var component in property.FoundComponents)
                    {
                        // Defensive check in case of external destruction.
                        if (null != component)
                        {
                            if (property.AffectsMaterialPropertyBlock)
                            {
                                var materialPropertyBlock = orchestrator.GetMaterialPropertyBlockForBakedObject(component.gameObject);
                                materialPropertyBlock.SetFloat(property.ShaderMaterialProperty, lerpValue);
                                orchestrator.StagePropertyBlock(component.gameObject);
                            }
                            else if (property.SpecialMarker == P12SpecialMarker.BlendShape)
                            {
                                var smr = (SkinnedMeshRenderer)component;
                                // FIXME: We need to cache this blendShape index
                                var index = smr.sharedMesh.GetBlendShapeIndex(property.PropertySuffix);
                                smr.SetBlendShapeWeight(index, lerpValue);
                            }
                            else
                            {
                                // FIXME: This is slow. Bake it into the property itself in Awake.
                                var fields = property.FoundType.GetFields();
                                foreach (var fieldInfo in fields)
                                {
                                    if (fieldInfo.Name == property.propertyName)
                                    {
                                        // TODO: Cast to the type that this field expects
                                        fieldInfo.SetValue(component, lerpValue);
                                    }
                                }
                            }
                        }
                        else
                        {
                            propertyNeedsCleanup = true;
                        }
                    }

                    if (propertyNeedsCleanup) ConsiderCleaningUpProperty(property.FoundComponents);
                }
            }
        }

        private static void ConsiderCleaningUpProperty(List<Component> foundComponents)
        {
            H12Utilities.RemoveNullsFromList(foundComponents);
            // TODO: If the list becomes empty, then it may be relevant to make that property non-applicable,
            // but we can't do that while iterating it in a foreach.
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

        [NonSerialized] internal List<GameObject> BakedObjects;

        public void BakeAffectedObjects(Transform context)
        {
            BakedObjects = new List<GameObject>();
            BakedObjects.AddRange(targets);
        }
    }

    public enum P12VixxySelection
    {
        Normal,
        RecursiveSearch,
        Everything
    }

    [Serializable]
    public struct P12VixxyProperty<T> : I12VixxyProperty
    {
        // TODO: It might be relevant to use another approach than getting animatable properties,
        // since we have control over the system. It doesn't have to piggyback on the animation APIs.
        public string fullClassName;
        public string propertyName;

        public bool flip;
        public T bound;
        public T unbound;

        // Runtime only
        [NonSerialized] internal bool IsApplicable;
        [NonSerialized] internal Type FoundType;
        [NonSerialized] internal List<Component> FoundComponents;
        [NonSerialized] internal bool AffectsMaterialPropertyBlock;
        [NonSerialized] internal P12SpecialMarker SpecialMarker;
        [NonSerialized] internal int ShaderMaterialProperty;
        [NonSerialized] internal string PropertySuffix;
    }

    interface I12VixxyProperty
    {
    }

    public enum P12SpecialMarker
    {
        None, BlendShape
    }
}
