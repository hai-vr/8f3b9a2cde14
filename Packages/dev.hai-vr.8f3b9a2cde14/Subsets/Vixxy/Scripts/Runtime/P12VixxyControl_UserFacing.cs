using System;
using System.Collections.Generic;
using System.Reflection;
using Hai.Project12.UserInterfaceElements.Runtime;
using UnityEngine;

namespace Hai.Project12.Vixxy.Runtime
{
    public partial class P12VixxyControl
    {
        /// The orchestrator defines the context that the subjects of this control will affect (e.g. Recursive Search).
        /// Vixxy is not an avatar-specific component, so it needs that limited context.
        [SerializeField] internal P12VixxyOrchestrator orchestrator;

        /// An address is not necessary, but if one is provided, then we will be using that provided address.
        /// If not, we will generate one at runtime.
        [SerializeField] internal string address = "";

        [SerializeField] internal P12SettableFloatElement sample;

        [SerializeField] internal P12VixxyActivation[] activations = Array.Empty<P12VixxyActivation>();
        [SerializeField] internal P12VixxySubject[] subjects = Array.Empty<P12VixxySubject>();

        /// The value that is considered to be OFF. This may be larger than upperBound.
        [SerializeField] internal float lowerBound = 0f;
        /// The value that is considered to be ON.
        [SerializeField] internal float upperBound = 1f;
        [SerializeField] internal AnimationCurve interpolationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Menu and Networking

        /// If the menu type is bool-like, we use isActiveByDefault.
        [SerializeField] public bool isActiveByDefault;
        /// If the menu type is float-like, we use defaultValue.
        [SerializeField] public float defaultValue;

        /// When the control mode is set to Simplified, we ignore some of the internally serialized values of this control and use
        /// sensible defaults: Remember across avatars, and networked by default.
        [SerializeField] public P12VixxyControlMode mode;

        // Those values below may be ignored depending on the control type.

        /// IF NOT ADVANCED MENU MODE: By default, we remember the control setting across all avatars, so long the address does not change.
        [SerializeField] internal P12VixxyRememberScope remember = P12VixxyRememberScope.RememberAcrossAvatars;
        /// IF NOT ADVANCED MENU MODE: Used only when remember is set to RememberInThisTag. We restore the value if control shares the same address and tag.
        [SerializeField] internal string rememberTag = "";
        /// IF NOT ADVANCED MENU MODE: If set to true, this value will be sent to other users whenever it is changed, or when the avatar loads.
        [SerializeField] internal bool networked = true;
    }

    [Serializable]
    public enum P12VixxyControlMode
    {
        Simplified,
        Advanced
    }

    [Serializable]
    public struct P12VixxyActivation
    {
        public Component component; // To toggle a GameObject, provide the Transform instead. It makes things easier as GameObject is not a component.
        public ActivationThreshold threshold;
        public bool whenActive;
    }

    [Serializable]
    public enum ActivationThreshold
    {
        /// When there's a transition, it is ON during that transition.<br/>
        /// In technical terms, it is considered to be ON when the absolute difference to the target is strictly smaller than 1.
        /// This is the best choice for stuff like material dissolves, where the object appears before it is even complete, and therefore the default.
        Blended,
        /// Is considered to be ON when the current value is equal to the target value.
        Strict,
    }

    [Serializable]
    public class P12VixxySubject
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
        [NonSerialized] internal P12VixxySubjectsBakeResult BakeResult;
    }

    [Serializable]
    public enum P12VixxySelection
    {
        Normal,
        RecursiveSearch,
        Everything
    }

    [Serializable]
    public enum P12VixxyRememberScope
    {
        /// When the avatar loads, the value is always the default.
        DoNotRemember,
        /// We remember the value for this address, only in this specific avatar.
        RememberInThisAvatar,
        /// We remember the value for this address, only across controls which share the same rememberTag value.
        RememberInThisTag,
        /// We remember the value for this address across all avatars.
        RememberAcrossAvatars
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
        public P12VixxyPropertyVariant variant;
        public string propertyName;

        public bool flip;

        // Runtime only
        [NonSerialized] internal bool IsApplicable;
        [NonSerialized] internal P12VixxyPropertyBakeResult BakeResult;
        [NonSerialized] internal Type FoundType;
        [NonSerialized] internal List<Component> FoundComponents;
        [NonSerialized] internal P12KindMarker KindMarker;
        [NonSerialized] internal int ShaderMaterialProperty;
        [NonSerialized] internal FieldInfo FieldIfMarkedAsFieldAccess; // null if SpecialMarker is not FieldAccess
        [NonSerialized] internal PropertyInfo TPropertyIfMarkedAsTPropertyAccess; // null if SpecialMarker is not PropertyAccess
        [NonSerialized] internal Dictionary<SkinnedMeshRenderer, int> SmrToBlendshapeIndex; // null if SpecialMarker is not BlendShape
    }

    [Serializable]
    public enum P12VixxyPropertyVariant
    {
        Standard,
        MaterialProperty,
        BlendShape
    }

    interface I12VixxyProperty
    {
    }
}
