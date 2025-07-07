using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.Project12.TF.Runtime
{
    [AddComponentMenu("TF Behaviour")]
    public class TFBehaviour : MonoBehaviour
    {
        public List<TFField> fields = new();
        public List<TFEvent> events = new();

        public bool supportLegacyPlatform;
        public bool addObjectNamesInCode;

        [HideInInspector]
        [TextArea(3, 10)]
        public string description;
    }

    [Serializable]
    public class TFField
    {
        public string name;
        public TFValue value; // TODO: Migrate to TFTypeInfo and TFDefinedValue
        public TFTypeInfo typeInformation;
        [SerializeReference] public ITFValue val;

        public string internalGuid;
    }

    [Serializable]
    public class TFEvent
    {
        public string eventName;
        public List<TFInstruction> instructions = new();
        [SerializeReference] public List<TFPredicate> predicates = new();
    }

    [Serializable]
    public class TFInstruction
    {
        public string identifier;
        public bool hasReturnValue;
        public bool isStatic;

        // The following applicable if NOT static
        public TFParameter self;
        // FIXME: migrate to TFParameter(self)

        public TFParameter[] parameters;
        public string assignTo;

        public string fieldifiedIdentifier;

        // Editor UI
        public bool isBeingEdited;
    }

    [Serializable]
    public class TFParameter
    {
        public bool isVariableOrField;
        public string identifierInternalGuid;

        [Obsolete] public TFValue value; // TODO: Migrate to TFTypeInfo and ITFValue

        // TODO: Use this new model:
        public TFTypeInfo typeInformation;
        [SerializeReference] public ITFValue val;
        [SerializeReference] public List<ITFValue> values = new();

        public string fieldifiedIdentifier;

        // Editor metadata
        public bool isStaged;
        public bool isMultivaluable;
    }

    public interface ITFValue {}
    [Serializable] public class TFValueNonStaged : ITFValue {}
    [Serializable] public class TFValueNull : ITFValue {}
    [Serializable] public class TFValueThis : ITFValue {}
    [Serializable] public class TFValueString { public string value; }
    [Serializable] public class TFValueBool { public bool value; }
    [Serializable] public class TFValueUnityObject { public Object value; }
    [Serializable] public class TFValueField { public string refInternalGuid; }

    public interface TFPredicate {}

    [Serializable]
    public class TFTypeInfo
    {
        public string fullClassName;
        public TFParameterTargetType targetType;
    }

    [Serializable]
    [Obsolete]
    public class TFValue
    {
        [Obsolete] public string fullClassName;
        [Obsolete] public TFParameterTargetType targetType;
        //
        [Obsolete] public bool isThis; // Only applicable to this.gameObject and this.transform
        [Obsolete] public string stringValue;
        [Obsolete] public Object objectValue;
        [Obsolete] public bool boolValue;
    }

    [Serializable]
    public enum TFParameterTargetType
    {
        String,
        Object,
        Boolean,
    }
}
