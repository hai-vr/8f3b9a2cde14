using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public class P12VixxyControl : MonoBehaviour, I12VixxyActuator
    {
        // Licensing notes:
        // Portions of the code below originally comes from portions of a proprietary software that I (Haï~) am the author of,
        // and is notably used in "Vixen" (2023-2024).
        // The code below is released under the same terms as the LICENSE file of the specific "Vixxy/" subdirectory that this file is contained in which is MIT,
        // including the specific portions of the code that originally came from "Vixen".

        private const string PropMaterialPrefix = "material.";
        private const string PropBlendShapePrefix = "blendShape.";

        /// The orchestrator defines the context that the subjects of this control will affect (e.g. Recursive Search).
        /// Vixxy is not an avatar-specific component, so it needs that limited context.
        [SerializeField] internal P12VixxyOrchestrator orchestrator;
        [SerializeField] internal string address;
        [SerializeField] internal P12SettableFloatElement sample;

        [SerializeField] internal P12VixxyActivations[] activations;
        [SerializeField] internal P12VixxySubject[] subjects;

        public float lowerBound = 0f;
        public float upperBound = 1f;
        public AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Runtime only
        private int _iddress;
        private Transform _context;
        private H12ActuatorRegistrationToken _registeredActuator;

        private readonly Dictionary<string, Type> _typeCache_mayContainNullObjects = new();

        public void Awake()
        {
            _iddress = H12VixxyAddress.AddressToId(address);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            _context = orchestrator.Context();

            // In this phase, we do all the checks, so that when actuation is requested (this might be as expensive
            // as running every frame), we don't need to do type checks or other work.
            // This means that we need to catch all invalid cases. See below comments that say "Not Applicable".
            for (var i = 0; i < subjects.Length; i++)
            {
                var subject = subjects[i];
                subject.BakeAffectedObjects(_context);

                if (subject.BakedObjects.Count == 0)
                {
                    // Subject Not applicable: No objects
                    subject.IsApplicable = false;
                    subjects[i] = subject;
                    continue;
                }

                var isAnyPropertyApplicable = false;
                var isAnyPropertyDependentOnMaterialPropertyBlock = false;

                for (var index = 0; index < subject.properties.Count; index++)
                {
                    var property = subject.properties[index];
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
                                isAnyPropertyApplicable = true;
                                property.IsApplicable = true;
                                property.FoundType = foundType;
                                property.FoundComponents = foundComponents;
                                if (affectsMaterialPropertyBlock)
                                {
                                    isAnyPropertyDependentOnMaterialPropertyBlock = true;
                                    property.ShaderMaterialProperty = Shader.PropertyToID(property.propertyName.Substring(PropMaterialPrefix.Length));
                                    property.SpecialMarker = P12SpecialMarker.AffectsMaterialPropertyBlock;
                                }
                                else if (property.propertyName.StartsWith(PropBlendShapePrefix) && foundType == typeof(SkinnedMeshRenderer))
                                {
                                    property.SpecialMarker = P12SpecialMarker.BlendShape;
                                }
                                else
                                {
                                    // FIXME: If field is null, then this property is not applicable!
                                    var fieldInfoNullable = GetFieldInfoOrNull(property);
                                    property.FieldIfMarkedAsFieldAccess = fieldInfoNullable;
                                    property.SpecialMarker = P12SpecialMarker.FieldAccess;

                                    // FIXME: This is set to false late, we should be checking that earlier. This whole method could be turned into TryResolveProperty(out Property) or something.
                                    property.IsApplicable = fieldInfoNullable != null;
                                }
                                property.PropertySuffix = property.propertyName.Contains('.') ? property.propertyName.Substring(property.propertyName.IndexOf('.') + 1) : "";
                            }
                            else
                            {
                                // Not applicable: No objects has that component
                                property.IsApplicable = false;
                            }
                        }
                        else
                        {
                            // Not applicable: Property requiring a MaterialPropertyBlock has no Renderer
                            property.IsApplicable = false;
                        }
                    }
                    else
                    {
                        // Not applicable: Type not found
                        property.IsApplicable = false;
                    }

                    subject.properties[index] = property;
                }

                if (isAnyPropertyDependentOnMaterialPropertyBlock)
                {
                    foreach (var bakedObject in subject.BakedObjects)
                    {
                        orchestrator.RequireMaterialPropertyBlock(bakedObject);
                    }
                }

                // When Subject Not applicable: As no property is applicable, the subject is not applicable either.
                subject.IsApplicable = isAnyPropertyApplicable;
                subjects[i] = subject;
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
            _registeredActuator = orchestrator.RegisterActuator(_iddress, this, OnAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterActuator(_registeredActuator);
            _registeredActuator = default;
        }

        private void OnAddressUpdated(string _, float value)
        {
            // FIXME: Storing that value is probably not a good idea to do at this specific stage of the processing.
            //           For comparison, we can't do this for aggregators (which can have multiple input values), it's not their responsibility.
            sample.storedValue = value;

            orchestrator.PassAddressUpdated(_iddress);
        }

        public void Actuate()
        {
            // FIXME: We really need to figure out how actuators sample values from their dependents.
            var linear01 = Mathf.InverseLerp(lowerBound, upperBound, sample.storedValue);
            var active01 = interpolationCurve.Evaluate(linear01);
            ActuateActivations(active01);
            ActuateSubjects(active01);
        }

        private void ActuateActivations(float active01)
        {
            foreach (var activation in activations)
            {
                var target = activation.whenActive ? 1f : 0f;
                switch (activation.threshold)
                {
                    case ActivationThreshold.Blended:
                        H12Utilities.SetToggleState(activation.component, Mathf.Abs(target - active01) < 1f);
                        break;
                    case ActivationThreshold.Strict:
                        H12Utilities.SetToggleState(activation.component, Mathf.Approximately(target, active01));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ActuateSubjects(float active01)
        {
            foreach (var subject in subjects)
            {
                // TODO: Rather than do that check every time, only keep applicable subjects into an internal field.
                if (!subject.IsApplicable) return;

                foreach (var property in subject.properties)
                {
                    // TODO: Rather than do that check every time, bake the applicable properties into an internal field.
                    if (!property.IsApplicable) continue;

                    var propertyNeedsCleanup = false;
                    object lerpValue = property switch
                    {
                        P12VixxyProperty<float> valueFloat => Mathf.Lerp(valueFloat.unbound, valueFloat.bound, active01),
                        P12VixxyProperty<Vector4> valueVector4 => Color.Lerp(valueVector4.unbound, valueVector4.bound, active01),
                        _ => null
                    };
                    foreach (var component in property.FoundComponents)
                    {
                        // Defensive check in case of external destruction.
                        if (null != component)
                        {
                            if (property.SpecialMarker == P12SpecialMarker.AffectsMaterialPropertyBlock)
                            {
                                var materialPropertyBlock = orchestrator.GetMaterialPropertyBlockForBakedObject(component.gameObject);
                                if (lerpValue is Vector4 lerpVector4Value)
                                {
                                    materialPropertyBlock.SetVector(property.ShaderMaterialProperty, lerpVector4Value);
                                }
                                else if (lerpValue is Color lerpColorValue)
                                {
                                    materialPropertyBlock.SetColor(property.ShaderMaterialProperty, lerpColorValue);
                                }
                                // TODO: Other types
                                orchestrator.StagePropertyBlock(component.gameObject);
                            }
                            else if (property.SpecialMarker == P12SpecialMarker.BlendShape)
                            {
                                if (lerpValue is float lerpFloatValue)
                                {
                                    var smr = (SkinnedMeshRenderer)component;
                                    // FIXME: We need to cache this blendShape index
                                    // Maybe all applicators need to be cached (float) => () lambdas in Awake or something.
                                    var index = smr.sharedMesh.GetBlendShapeIndex(property.PropertySuffix);
                                    smr.SetBlendShapeWeight(index, lerpFloatValue);
                                }
                            }
                            else if (property.SpecialMarker == P12SpecialMarker.FieldAccess)
                            {
                                var fieldInfo = property.FieldIfMarkedAsFieldAccess;
                                // TODO: Cast to the type that this field expects
                                fieldInfo.SetValue(component, lerpValue);
                            }
                            else if (property.SpecialMarker == P12SpecialMarker.Undefined)
                            {
                                throw new ArgumentException("We tried to access an Undefined property, but Undefined properties are not supposed" +
                                                            " to be valid if the property IsApplicable. This may be a programming error, did you" +
                                                            " properly check that the property IsApplicable?");
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

        private static FieldInfo GetFieldInfoOrNull(P12VixxyPropertyBase property)
        {
            var fields = property.FoundType.GetFields();
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.Name == property.propertyName)
                {
                    return fieldInfo;
                }
            }

            return null;
        }

        private static void ConsiderCleaningUpProperty(List<Component> foundComponents)
        {
            H12Utilities.RemoveDestroyedFromList(foundComponents);
            // TODO: If the list becomes empty, then it may be relevant to make that property non-applicable,
            // but we can't do that while iterating it in a foreach.
        }
    }

    [Serializable]
    public struct P12VixxyActivations
    {
        public Component component; // To toggle a GameObject, provide the Transform instead. It makes things easier as GameObject is not a component.
        public ActivationThreshold threshold;
        public bool whenActive;
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
        [SerializeReference] public List<P12VixxyPropertyBase> properties;

        // Runtime only
        [NonSerialized] internal List<GameObject> BakedObjects;
        [NonSerialized] internal bool IsApplicable;

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
    public class P12VixxyProperty<T> : P12VixxyPropertyBase
    {
        public T bound;
        public T unbound;
    }

    [Serializable]
    public class P12VixxyPropertyBase : I12VixxyProperty
    {
        // TODO: It might be relevant to use another approach than getting animatable properties,
        // since we have control over the system. It doesn't have to piggyback on the animation APIs.
        public string fullClassName;
        public string propertyName;

        public bool flip;

        // Runtime only
        [NonSerialized] internal bool IsApplicable;
        [NonSerialized] internal Type FoundType;
        [NonSerialized] internal List<Component> FoundComponents;
        [NonSerialized] internal P12SpecialMarker SpecialMarker;
        [NonSerialized] internal int ShaderMaterialProperty;
        [NonSerialized] internal string PropertySuffix;
        [NonSerialized] internal FieldInfo FieldIfMarkedAsFieldAccess; // null if SpecialMarker is not FieldAccess
    }

    interface I12VixxyProperty
    {
    }

    public enum P12SpecialMarker
    {
        Undefined, AffectsMaterialPropertyBlock, BlendShape, FieldAccess
    }
}
