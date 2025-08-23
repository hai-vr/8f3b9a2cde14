using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Basis.Scripts.BasisSdk;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    /// UGC Rule: GameObjects and Components referenced by this class should be treated defensively as being UGC at runtime:<br/>
    /// - There may be null values in the arrays, as they may be unreliable user input or removed as part of a build process (e.g. EditorOnly),<br/>
    /// - Non-null values in the array may reference objects that will be destroyed later, so treat objects and components as potentially destroyable,<br/>
    /// - Similarly to animation, it is fine for the user to define rules that cannot apply (i.e. setting a material property on a type that isn't a Renderer,
    ///   referencing a field on a type that cannot exist, etc.); do not treat those as errors,<br/>
    /// - Do not treat anything else defensively than the above points, which are expectations of this specific system.
    public partial class P12VixxyControl : MonoBehaviour, I12VixxyActuator
    {
        // Licensing notes:
        // Portions of the code below originally comes from portions of a proprietary software that I (Haï~) am the author of,
        // and is notably used in "Vixen" (2023-2024).
        // The code below is released under the same terms as the LICENSE file of the specific "Vixxy/" subdirectory that this file is contained in which is MIT,
        // including the specific portions of the code that originally came from "Vixen".

        // Runtime only
        private BasisAvatar _avatarNullable;

        private int _iddress;
        private Transform _context;
        private H12ActuatorRegistrationToken _registeredActuator;

        private P12SettableFloatElement _menuElement;
        private float _previousValue;
        private float _bakedDefaultValue;

        [NonSerialized] internal string Address;
        [NonSerialized] internal P12VixxyRememberScope Remember;
        [NonSerialized] internal string RememberTagNullable;
        [NonSerialized] internal bool Networked;
        [NonSerialized] internal bool WasOnAvatarReadyCalled;
        [NonSerialized] internal bool IsWearer;

        public void Awake()
        {
            _context = orchestrator.Context();

            // null avatar can happen in testing scenes, where the control is outside an avatar.
            // We don't want to treat this as being an error.
            _avatarNullable = GetComponentInParent<BasisAvatar>(true);

            Address = string.IsNullOrWhiteSpace(address) ? GenerateAddressFromPath() : address;
            _iddress = H12VixxyAddress.AddressToId(Address);

            switch (mode)
            {
                case P12VixxyControlMode.Simplified:
                {
                    Remember = P12VixxyRememberScope.RememberAcrossAvatars;
                    RememberTagNullable = null;
                    Networked = true;
                    break;
                }
                case P12VixxyControlMode.Advanced:
                {
                    if (remember == P12VixxyRememberScope.RememberInThisTag)
                    {
                        if (!string.IsNullOrWhiteSpace(rememberTag))
                        {
                            Remember = P12VixxyRememberScope.RememberInThisTag;
                            RememberTagNullable = rememberTag;
                        }
                        else
                        {
                            // Get rid of invalid user-provided configurations.
                            Remember = P12VixxyRememberScope.RememberInThisAvatar;
                            RememberTagNullable = null;
                        }
                    }
                    else
                    {
                        Remember = remember;
                    }
                    Networked = networked;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // UGC Rule: Sanitize arrays.
            activations ??= Array.Empty<P12VixxyActivation>();
            subjects ??= Array.Empty<P12VixxySubject>();

            BakeControlSubjectsForRuntime();
            _bakedDefaultValue = defaultValue; // TODO: The baked value depends on the level of detail in the control, and the type of control.

            // TODO: This is temporary code
            _menuElement = ScriptableObject.CreateInstance<P12SettableFloatElement>();
            _menuElement.localizedTitle = gameObject.name;
            _menuElement.min = 0f;
            _menuElement.max = 1f;
            _menuElement.displayAs = P12SettableFloatElement.P12UnitDisplayKind.Toggle; // TODO: This depends on the type of control.
            _menuElement.defaultValue = _bakedDefaultValue;
            _menuElement.storedValue = _bakedDefaultValue;
            sample = _menuElement;

            if (_avatarNullable != null)
            {
                _avatarNullable.OnAvatarReady -= OnAvatarReady;
                _avatarNullable.OnAvatarReady += OnAvatarReady;
            }
            else
            {
                // A lack of avatar probably means we're in a testing scene, so add it to the menu.
                orchestrator.RegisterMenu(_menuElement);
            }

            if (Networked)
            {
                var netDataUsage = P12VixxyNetDataUsage.Bit; // TODO: This depends on the type of control.
                orchestrator.RequireNetworked(Address, _bakedDefaultValue, netDataUsage);
            }
        }

        private void OnDestroy()
        {
            orchestrator.UnregisterMenu(_menuElement);
            sample.OnValueChanged -= OnValueChanged;
            if (_avatarNullable != null)
            {
                _avatarNullable.OnAvatarReady -= OnAvatarReady;
            }
        }

        private void OnAvatarReady(bool isOwner)
        {
            WasOnAvatarReadyCalled = true;
            IsWearer = isOwner;
            if (isOwner)
            {
                orchestrator.RegisterMenu(_menuElement);
            }
            sample.OnValueChanged -= OnValueChanged;
            sample.OnValueChanged += OnValueChanged;
        }

        private string GenerateAddressFromPath()
        {
            var componentIndex = Array.IndexOf(transform.GetComponents<P12VixxyControl>(), this);
            var path = H12Utilities.ResolveRelativePath(orchestrator.Context().transform, transform);
            var newAddress = $"{path}@{H12Utilities.SimpleSha1(path)}+{componentIndex}";
            return newAddress;
        }

        private void OnValueChanged(float newValue)
        {
            orchestrator.___SubmitToAcquisitionService(Address, newValue);
        }

        /// Called by the Editor when a serialized property changes due to live edits. This is not to be invoked by anything else.
        internal void DebugOnly_ReBakeControl()
        {
            BakeControlSubjectsForRuntime();
        }

        private void BakeControlSubjectsForRuntime()
        {
            // In this phase, we do all the checks, so that when actuation is requested (this might be as expensive
            // as running every frame), we don't need to do type checks or other work.
            // This means that we need to catch all invalid cases.
            foreach (var subject in subjects)
            {
                // UGC Rule: Sanitize input arrays.
                subject.targets ??= Array.Empty<GameObject>();
                subject.childrenOf ??= Array.Empty<GameObject>();
                subject.exceptions ??= Array.Empty<GameObject>();
                subject.properties ??= new List<P12VixxyPropertyBase>();

                BakeSubjectAffectedObjects(subject, _context);

                if (subject.BakedObjects.Count == 0)
                {
                    subject.IsApplicable = false;
                    subject.BakeResult = P12VixxySubjectsBakeResult.NoBakedObjects;

                    foreach (var property in subject.properties)
                    {
                        property.IsApplicable = false;
                        property.BakeResult = P12VixxyPropertyBakeResult.SubjectHasNoBakedObjects;
                    }

                    continue;
                }

                var isAnyPropertyApplicable = false;
                var isAnyPropertyDependentOnMaterialPropertyBlock = false;

                foreach (var property in subject.properties)
                {
                    var bakeResult = BakeProperty(property, subject);
                    var isApplicable = bakeResult == P12VixxyPropertyBakeResult.Success;
                    property.IsApplicable = isApplicable;
                    property.BakeResult = bakeResult;

                    isAnyPropertyApplicable |= isApplicable;
                    if (isApplicable && property.KindMarker == P12KindMarker.AffectsMaterialPropertyBlock)
                    {
                        isAnyPropertyDependentOnMaterialPropertyBlock = true;
                    }
                }

                if (isAnyPropertyDependentOnMaterialPropertyBlock)
                {
                    foreach (var bakedObject in subject.BakedObjects)
                    {
                        orchestrator.RequireMaterialPropertyBlock(bakedObject);
                    }
                }

                subject.IsApplicable = isAnyPropertyApplicable;
                subject.BakeResult = isAnyPropertyApplicable ? P12VixxySubjectsBakeResult.Success : P12VixxySubjectsBakeResult.NoPropertyIsApplicable;
            }
        }

        private static void BakeSubjectAffectedObjects(P12VixxySubject subject, Transform context)
        {
            var bakedObjects = new List<GameObject>();

            switch (subject.selection)
            {
                case P12VixxySelection.Normal:
                {
                    bakedObjects.AddRange(subject.targets);
                    break;
                }
                case P12VixxySelection.RecursiveSearch:
                {
                    // TODO: Prevent out-of-context searches
                    // TODO: Handle "except these objects" of a recursive search (check how Vixen does it)
                    foreach (var childrenRoot in subject.childrenOf)
                    {
                        if (childrenRoot != null) // UGC rule.
                        {
                            var allTransforms = childrenRoot.GetComponentsInChildren<Transform>(true);
                            foreach (var t in allTransforms)
                            {
                                bakedObjects.Add(t.gameObject);
                            }
                        }
                    }
                    break;
                }
                case P12VixxySelection.Everything:
                {
                    // TODO: Handle "except these objects" of a recursive search
                    var allTransforms = context.GetComponentsInChildren<Transform>(true);
                    foreach (var t in allTransforms)
                    {
                        bakedObjects.Add(t.gameObject);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            H12Utilities.RemoveDestroyedFromList(bakedObjects); // Following the UGC rule; see class header.

            subject.BakedObjects = bakedObjects;
        }

        private P12VixxyPropertyBakeResult BakeProperty(P12VixxyPropertyBase property, P12VixxySubject subject)
        {
            if (!H12ComponentDictionary.TryGetComponentType(property.fullClassName, out var foundType)) return P12VixxyPropertyBakeResult.TypeNotFound;

            var affectsMaterialPropertyBlock = property.variant == P12VixxyPropertyVariant.MaterialProperty;
            var affectsBlendShape = property.variant == P12VixxyPropertyVariant.BlendShape;

            // UGC: Detect misconfiguration oddities
            if (affectsMaterialPropertyBlock && !typeof(Renderer).IsAssignableFrom(foundType))
            {
                return P12VixxyPropertyBakeResult.MaterialPropertyBlockCanOnlyBeUsedOnRenderers;
            }
            if (affectsBlendShape && foundType != typeof(SkinnedMeshRenderer))
            {
                return P12VixxyPropertyBakeResult.BlendShapeCanOnlyBeUsedOnSkinnedMeshRenderers;
            }

            var foundComponents = new List<Component>();
            foreach (var bakedObject in subject.BakedObjects)
            {
                var component = bakedObject.GetComponent(foundType);
                if (component != null) // This is *NOT* UGC Rule. Some of the targets just may not have that component, especially the non-first objects, and recursive searches.
                {
                    foundComponents.Add(component);
                }
            }

            if (foundComponents.Count <= 0) return P12VixxyPropertyBakeResult.NoObjectsHasThatComponent;

            if (affectsMaterialPropertyBlock)
            {
                property.ShaderMaterialProperty = Shader.PropertyToID(property.propertyName);
                property.KindMarker = P12KindMarker.AffectsMaterialPropertyBlock;
            }
            else if (affectsBlendShape)
            {
                var nonApplicableComponents = new List<Component>();
                var smrToIndex = new Dictionary<SkinnedMeshRenderer, int>();
                foreach (var component in foundComponents)
                {
                    var smr = (SkinnedMeshRenderer)component;
                    var sharedMesh = smr.sharedMesh;
                    if (sharedMesh != null)
                    {
                        var blendShapeIndex = sharedMesh.GetBlendShapeIndex(property.propertyName);
                        if (blendShapeIndex != -1)
                        {
                            smrToIndex[smr] = blendShapeIndex;
                        }
                        else
                        {
                            nonApplicableComponents.Add(component);
                        }
                    }
                    else
                    {
                        nonApplicableComponents.Add(component);
                    }
                }

                foreach (var nonApplicableComponent in nonApplicableComponents)
                {
                    foundComponents.Remove(nonApplicableComponent);
                }

                if (foundComponents.Count == 0) return P12VixxyPropertyBakeResult.NoSkinnedMeshRendererHasThisBlendShape;

                property.KindMarker = P12KindMarker.BlendShape;
                property.SmrToBlendshapeIndex = smrToIndex;
            }
            else
            {
                var fieldInfoNullable = GetFieldInfoOrNull(foundType, property.propertyName);
                if (fieldInfoNullable != null)
                {
                    property.FieldIfMarkedAsFieldAccess = fieldInfoNullable;
                    property.KindMarker = P12KindMarker.FieldAccess;
                }
                else
                {
                    var propertyInfoNullable = GetPropertyInfoOrNull(foundType, property.propertyName);
                    if (propertyInfoNullable == null) return P12VixxyPropertyBakeResult.NoFieldNorPropertyMatches;

                    property.TPropertyIfMarkedAsTPropertyAccess = propertyInfoNullable;
                    property.KindMarker = P12KindMarker.PropertyAccess;
                }
            }

            property.FoundType = foundType;
            property.FoundComponents = foundComponents;

            return P12VixxyPropertyBakeResult.Success;
        }

        private void OnEnable()
        {
            _previousValue = float.MinValue + 1.23456789f;
            _registeredActuator = orchestrator.RegisterActuator(_iddress, this, OnImplicitAddressUpdated);
        }

        private void OnDisable()
        {
            orchestrator.UnregisterActuator(_registeredActuator);
            _registeredActuator = default;
        }

        private void OnImplicitAddressUpdated(float value)
        {
            // FIXME: This is a bypass so that we don't update an address that hasn't changed.
            // Ideally, the orchestrator should instead provide us a guarantee of calling us only when the value changes.
            if (value == _previousValue) return;
            _previousValue = value;

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
            // TODO: Bake activations in Awake, so that we may remove components that were destroyed without affecting the serialized state of the control.
            foreach (var activation in activations)
            {
                // Defensive check in case of external destruction.
                if (null != activation.component)
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
        }

        private void ActuateSubjects(float active01)
        {
            foreach (var subject in subjects)
            {
                // TODO: Rather than do that check every time, only keep applicable subjects into an internal field.
                if (!subject.IsApplicable) continue;

                foreach (var property in subject.properties)
                {
                    // TODO: Rather than do that check every time, bake the applicable properties into an internal field.
                    if (!property.IsApplicable) continue;

                    var propertyNeedsCleanup = false;
                    object lerpValue = property switch
                    {
                        P12VixxyProperty<float> valueFloat => Mathf.Lerp(valueFloat.unbound, valueFloat.bound, active01),
                        P12VixxyProperty<Color> valueColor => Color.Lerp(valueColor.unbound, valueColor.bound, active01),
                        P12VixxyProperty<Vector4> valueVector4 => Vector4.Lerp(valueVector4.unbound, valueVector4.bound, active01),
                        P12VixxyProperty<Vector3> valueVector3 => Vector3.Lerp(valueVector3.unbound, valueVector3.bound, active01),
                        _ => null
                    };
                    foreach (var component in property.FoundComponents)
                    {
                        // Defensive check in case of external destruction.
                        if (null != component)
                        {
                            switch (property.KindMarker)
                            {
                                case P12KindMarker.AffectsMaterialPropertyBlock:
                                {
                                    var materialPropertyBlock = orchestrator.GetMaterialPropertyBlockForBakedObject(component.gameObject);
                                    // TODO: Instead of checking the type, use something like property.ApplyMaterialProperty(materialPropertyBlock, value), where the property itself knows how to apply it to the property block.
                                    switch (lerpValue)
                                    {
                                        case float lerpFloatValue: materialPropertyBlock.SetFloat(property.ShaderMaterialProperty, lerpFloatValue); break;
                                        case Color lerpColorValue: materialPropertyBlock.SetColor(property.ShaderMaterialProperty, lerpColorValue); break;
                                        case Vector4 lerpVector4Value: materialPropertyBlock.SetVector(property.ShaderMaterialProperty, lerpVector4Value); break;
                                        case Vector3 lerpVector3Value: materialPropertyBlock.SetVector(property.ShaderMaterialProperty, lerpVector3Value); break;
                                        // TODO: Other types
                                    }
                                    orchestrator.StagePropertyBlock(component.gameObject);
                                    break;
                                }
                                case P12KindMarker.BlendShape:
                                {
                                    if (lerpValue is float lerpFloatValue)
                                    {
                                        var smr = (SkinnedMeshRenderer)component;
                                        var blendShapeIndex = property.SmrToBlendshapeIndex[smr];
                                        smr.SetBlendShapeWeight(blendShapeIndex, lerpFloatValue);
                                    }
                                    break;
                                }
                                case P12KindMarker.FieldAccess:
                                {
                                    var fieldInfo = property.FieldIfMarkedAsFieldAccess;
                                    fieldInfo.SetValue(component, lerpValue);
                                    orchestrator.StagePossibleSpecialComponentHandling(component);
                                    break;
                                }
                                case P12KindMarker.PropertyAccess:
                                {
                                    var propertyInfo = property.TPropertyIfMarkedAsTPropertyAccess;
                                    propertyInfo.SetValue(component, lerpValue);
                                    orchestrator.StagePossibleSpecialComponentHandling(component);
                                    break;
                                }
                                case P12KindMarker.Undefined:
                                default:
                                    throw new ArgumentException("We tried to access an Undefined property, but Undefined properties are not supposed" +
                                                                " to be valid if the property IsApplicable. This may be a programming error, did we" +
                                                                " properly check that the property IsApplicable?");
                            }
                        }
                        else
                        {
                            propertyNeedsCleanup = true;
                        }
                    }

                    if (propertyNeedsCleanup)
                    {
                        H12Utilities.RemoveDestroyedFromList(property.FoundComponents);
                        if (property.FoundComponents.Count == 0)
                        {
                            property.IsApplicable = false;
                            // TODO: Also invalidate the subject if no property of that subject is applicable
                        }
                    }
                }
            }
        }

        private static FieldInfo GetFieldInfoOrNull(Type foundType, string propertyName)
        {
            var fields = foundType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.Name == propertyName)
                {
                    return fieldInfo;
                }
            }

            return null;
        }

        private static PropertyInfo GetPropertyInfoOrNull(Type foundType, string propertyName)
        {
            var typeProperties = foundType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var propertyInfo in typeProperties)
            {
                if (propertyInfo.Name == propertyName)
                {
                    return propertyInfo;
                }
            }

            return null;
        }
    }

    internal enum P12KindMarker
    {
        Undefined,
        AffectsMaterialPropertyBlock,
        BlendShape,
        FieldAccess,
        PropertyAccess
    }

    /// When baking properties, the user may have misconfigured the property, or the property configuration may not apply to a
    /// given targeted app. This enumeration describes the various errors that can happen, and it also serves as a comment in the
    /// code explaining why the property is not applicable.
    internal enum P12VixxyPropertyBakeResult
    {
        Success,
        SubjectHasNoBakedObjects,
        TypeNotFound,
        NoObjectsHasThatComponent,
        MaterialPropertyBlockCanOnlyBeUsedOnRenderers,
        BlendShapeCanOnlyBeUsedOnSkinnedMeshRenderers,
        NoSkinnedMeshRendererHasThisBlendShape,
        NoFieldNorPropertyMatches
    }

    internal enum P12VixxySubjectsBakeResult
    {
        Success,
        NoBakedObjects,
        NoPropertyIsApplicable
    }
}
