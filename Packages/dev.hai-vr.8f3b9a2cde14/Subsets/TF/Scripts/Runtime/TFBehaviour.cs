using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.Project12.TF.Runtime
{
    [AddComponentMenu("TFBehaviour")]
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
        public TFValue value;
        public string internalGuid;
    }

    [Serializable]
    public class TFEvent
    {
        public string eventName;
        public List<TFElement> instructions = new();
    }

    [Serializable]
    public class TFElement
    {
        public string identifier;
        public bool hasReturnValue;
        public bool isStatic;

        // The following applicable if NOT static
        public TFParameter self;
        // FIXME: migrate to TFParameter(self)
        // [Obsolete] public Object instance; // FIXME: Need the ability to target a field.

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

        public TFValue value;

        public string fieldifiedIdentifier;
    }

    [Serializable]
    public class TFValue
    {
        public string fullClassName; // FIXME: This data is not actually used during generation
        public bool isThis; // Only applicable to this.gameObject and this.transform
        public TFParameterTargetType targetType;
        //
        public string stringValue;
        public Object objectValue;
        public bool boolValue;
    }

    [Serializable]
    public enum TFParameterTargetType
    {
        String,
        Object,
        Boolean,
    }
}
