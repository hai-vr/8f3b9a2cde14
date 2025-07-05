using System;
using System.Collections.Generic;
using System.Linq;
using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.HaiSystems.Editor;
using Hai.Project12.HaiSystems.Supporting;
using Hai.Project12.TF.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.TF.Editor
{
    internal class H12TFLayoutEvents
    {
        private const string EventsLabel = "Events";
        private const string MsgDescribeAwake = "Only ever called once, before this component becomes enabled";
        private const string MsgDescribeOnEnable = "Every time this component becomes enabled";
        private const string MsgDescribeOnDisable = "Every time this component becomes disabled";
        private const string MsgDescribeOnUpdate = "Every frame while this component remains enabled";
        private const string MsgDescribeOnPress = "When the interaction button starts pressing this object";
        private const string MsgDescribeOnRelease = "When the interaction button stops pressing this object";

        private const float ComponentWidth = 80;

        private readonly TFBehaviour my;
        private readonly TFBehaviourEditor editor;
        private readonly SerializedObject serializedObject;

        private bool _initialized;
        private GUIStyle _richText;
        private GUIStyle _richTextWrap;
        private string _colorSource;
        private string _colorFunction;
        private string _colorVariableOrField;

        public H12TFLayoutEvents(TFBehaviourEditor editor)
        {
            my = (TFBehaviour)editor.target;
            this.editor = editor;
            serializedObject = editor.serializedObject;
        }

        public void Layout()
        {
            if (!_initialized)
            {
                _richText = new GUIStyle(EditorStyles.label) { richText = true };
                _richTextWrap = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
                _colorSource = ColorUtility.ToHtmlStringRGB(new Color(1f, 0.83f, 0.49f));
                _colorFunction = ColorUtility.ToHtmlStringRGB(Color.cyan);
                _colorVariableOrField = ColorUtility.ToHtmlStringRGB(new Color(0.48f, 0.92f, 0.48f));
                _initialized = true;
            }

            EditorGUILayout.LabelField(EventsLabel, EditorStyles.boldLabel);
            var eventsSp = serializedObject.FindProperty(nameof(TFBehaviour.events));
            for (var i = 0; i < eventsSp.arraySize; i++)
            {
                var eventSp = eventsSp.GetArrayElementAtIndex(i);
                var eventNameSp = eventSp.FindPropertyRelative(nameof(TFEvent.eventName));
                var instructionsSp = eventSp.FindPropertyRelative(nameof(TFEvent.instructions));

                EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
                var eventName = eventNameSp.stringValue;
                if (TryDescribeEvent(eventName, out var description))
                {
                    EditorGUILayout.LabelField($"<b>{eventName}</b>    <i>{description}</i>", _richTextWrap);
                }
                else
                {
                    EditorGUILayout.LabelField($"<b>{eventName}</b>", _richTextWrap);
                }
                for (var j = 0; j < instructionsSp.arraySize; j++)
                {
                    var instructionSp = instructionsSp.GetArrayElementAtIndex(j);
                    var identifierSp = instructionSp.FindPropertyRelative(nameof(TFElement.identifier));
                    var parametersSp = instructionSp.FindPropertyRelative(nameof(TFElement.parameters));
                    var isBeingEditedSp = instructionSp.FindPropertyRelative(nameof(TFElement.isBeingEdited));

                    var uiParameters = AsUIParameters(parametersSp);

                    var isStatic = instructionSp.FindPropertyRelative(nameof(TFElement.isStatic)).boolValue;
                    if (isStatic)
                    {
                        // Method name
                        EditorGUILayout.BeginHorizontal();
                        var toDecompose = identifierSp.stringValue;
                        var lastDot = toDecompose.LastIndexOf(".", StringComparison.Ordinal);
                        if (lastDot != -1)
                        {
                            var first = toDecompose.Substring(0, lastDot);
                            var second = toDecompose.Substring(lastDot + 1);
                            EditorGUILayout.LabelField($"<color=#{_colorSource}>{first}</color>.<color=#{_colorFunction}>{second}</color>({uiParameters})", _richText);
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"<color=#{_colorFunction}>{toDecompose}</color>({uiParameters})", _richText);
                        }
                        MakeEditButton(isBeingEditedSp);
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        // Method name
                        EditorGUILayout.BeginHorizontal();
                        var selfSp = instructionSp.FindPropertyRelative(nameof(TFElement.self));
                        var valueSp = selfSp.FindPropertyRelative(nameof(TFParameter.value));
                        var fullClassNameSp = valueSp.FindPropertyRelative(nameof(TFValue.fullClassName));

                        var name = AsObjectTypeName(fullClassNameSp.stringValue);

                        var isCollapsed = !isBeingEditedSp.boolValue;
                        if (isCollapsed)
                        {
                            EditorGUILayout.LabelField($"<color=#{_colorSource}>({name})</color>.<color=#{_colorFunction}>{identifierSp.stringValue}</color>({uiParameters})", _richText);
                            EditorGUILayout.LabelField($"<color=#{_colorSource}>(#)</color>", _richText, GUILayout.Width(20));
                            EditorGUI.BeginDisabledGroup(true);
                            ShowParameter(selfSp, -1, true);
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"<color=#{_colorSource}>({name})</color>.<color=#{_colorFunction}>{identifierSp.stringValue}</color>({uiParameters})", _richText);
                        }
                        MakeEditButton(isBeingEditedSp);
                        EditorGUILayout.EndHorizontal();
                    }

                    // Parameters
                    if (isBeingEditedSp.boolValue)
                    {
                        // Instance param
                        if (!isStatic)
                        {
                            ShowParameter(instructionSp.FindPropertyRelative(nameof(TFElement.self)), -1);
                        }

                        // Other params
                        for (var k = 0; k < parametersSp.arraySize; k++)
                        {
                            var parameterSp = parametersSp.GetArrayElementAtIndex(k);
                            ShowParameter(parameterSp, k);
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        private string AsUIParameters(SerializedProperty parametersSp)
        {
            var collapsedParameters = new List<string>();
            for (var k = 0; k < parametersSp.arraySize; k++)
            {
                var parameterSp = parametersSp.GetArrayElementAtIndex(k);
                collapsedParameters.Add(UIParameter(parameterSp));
            }
            var uiParameters = string.Join(", ", collapsedParameters);
            return uiParameters;
        }

        private string UIParameter(SerializedProperty parameterSp)
        {
            var isVariableOrFieldSp = parameterSp.FindPropertyRelative(nameof(TFParameter.isVariableOrField));
            if (isVariableOrFieldSp.boolValue)
            {
                var guidSp = parameterSp.FindPropertyRelative(nameof(TFParameter.identifierInternalGuid));
                return $"<color=#{_colorVariableOrField}>{ConnectGuid(guidSp.stringValue)}</color>";
            }
            else
            {
                var valueSp = parameterSp.FindPropertyRelative(nameof(TFParameter.value));
                var targetTypeSp = valueSp.FindPropertyRelative(nameof(TFValue.targetType));
                var targetType = (TFParameterTargetType)targetTypeSp.intValue;
                switch (targetType)
                {
                    case TFParameterTargetType.String: return "<i>string</i>";
                    case TFParameterTargetType.Object:
                    {
                        var fullClassNameSp = valueSp.FindPropertyRelative(nameof(TFValue.fullClassName));
                        return AsObjectTypeName(fullClassNameSp.stringValue);
                    }
                    case TFParameterTargetType.Boolean: return $"<color=white>{(valueSp.FindPropertyRelative(nameof(TFValue.boolValue)).boolValue ? "true" : "false")}</color>";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private string AsObjectTypeName(string objectTypeName)
        {
            var lastIndex = objectTypeName.LastIndexOf(".", StringComparison.Ordinal);
            if (lastIndex == -1) return objectTypeName;
            return objectTypeName.Substring(lastIndex + 1);
        }

        private string ConnectGuid(string guidOrEmpty)
        {
            if (guidOrEmpty == "") return "null";
            return my.fields.First(field => field.internalGuid == guidOrEmpty).name;
        }

        private void ShowParameter(SerializedProperty parameterSp, int index0orMinusOne, bool brief = false)
        {
            var isSelf = index0orMinusOne == -1;

            var isVariableOrFieldSp = parameterSp.FindPropertyRelative(nameof(TFParameter.isVariableOrField));
            var valueSp = parameterSp.FindPropertyRelative(nameof(TFParameter.value));
            var fullClassNameSp = valueSp.FindPropertyRelative(nameof(TFValue.fullClassName));

            EditorGUILayout.BeginHorizontal();
            if (!brief)
            {
                EditorGUILayout.LabelField($"", GUILayout.Width(15));
                if (isSelf)
                {
                    EditorGUILayout.LabelField($"<color=#{_colorSource}>(#)</color>", _richText, GUILayout.Width(30));
                    EditorGUILayout.LabelField($"<color=#{_colorSource}>{AsObjectTypeName(fullClassNameSp.stringValue)}</color>", _richText, GUILayout.Width(ComponentWidth));
                }
                else
                {
                    EditorGUILayout.LabelField($"#{index0orMinusOne + 1}", GUILayout.Width(30));
                    EditorGUILayout.LabelField($"{AsObjectTypeName(fullClassNameSp.stringValue)}", GUILayout.Width(ComponentWidth));
                }
            }

            var isVariableOrField = isVariableOrFieldSp.boolValue;
            if (isVariableOrField)
            {
                var identifierInternalGuidSp = parameterSp.FindPropertyRelative(nameof(TFParameter.identifierInternalGuid));
                ParameterSelector(parameterSp, identifierInternalGuidSp);

                if (editor._developerViewFoldout)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(identifierInternalGuidSp, GUIContent.none, GUILayout.Width(80));
                    EditorGUI.EndDisabledGroup();
                }
            }
            else
            {
                var targetTypeSp = valueSp.FindPropertyRelative(nameof(TFValue.targetType));

                // EditorGUI.BeginDisabledGroup(true);
                // EditorGUILayout.PropertyField(targetTypeSp, GUIContent.none, GUILayout.Width(H12TFLayoutFields.TargetTypeWidth));
                // EditorGUI.EndDisabledGroup();

                var targetType = (TFParameterTargetType)targetTypeSp.intValue;
                switch (targetType)
                {
                    case TFParameterTargetType.String:
                        EditorGUILayout.PropertyField(valueSp.FindPropertyRelative(nameof(TFValue.stringValue)), GUIContent.none);
                        break;
                    case TFParameterTargetType.Object:
                    {
                        var objectValueSp = valueSp.FindPropertyRelative(nameof(TFValue.objectValue));
                        if (H12ComponentDictionary.TryGetComponentType(fullClassNameSp.stringValue, out var componentType))
                        {
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUILayout.ObjectField(objectValueSp.objectReferenceValue, componentType, true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                objectValueSp.objectReferenceValue = newValue;
                            }
                        }
                        else
                        {
                            // TODO: Show a warning here, as the component is not found in this project
                            EditorGUILayout.PropertyField(objectValueSp, GUIContent.none);
                        }
                    }
                        break;
                    case TFParameterTargetType.Boolean:
                        var boolValueSp = valueSp.FindPropertyRelative(nameof(TFValue.boolValue));
                        EditorGUILayout.PropertyField(boolValueSp, GUIContent.none, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                        EditorGUILayout.LabelField(boolValueSp.boolValue ? "true" : "false");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ParameterSelector(SerializedProperty parameterSp, SerializedProperty identifierInternalGuidSp)
        {
            var applicableFields = my.fields
                .Where(field => field.value.targetType == ((TFParameter)parameterSp.boxedValue).value.targetType)
                .ToList();
            var choices = new[] { "null" }
                .Concat(applicableFields.Select(field => field.name))
                .ToArray();
            var currentValue = 0;
            for (var index = 0; index < applicableFields.Count; index++)
            {
                var applicableField = applicableFields[index];
                if (applicableField.internalGuid == identifierInternalGuidSp.stringValue)
                {
                    currentValue = index + 1;
                    break;
                }
            }

            var newValue = EditorGUILayout.Popup(currentValue, choices);
            if (currentValue != newValue)
            {
                if (newValue == 0) identifierInternalGuidSp.stringValue = "";
                else identifierInternalGuidSp.stringValue = applicableFields[newValue - 1].internalGuid;
            }
        }

        private static void MakeEditButton(SerializedProperty isBeingEditedSp)
        {
            var clicked = HaiEFCommon.ColoredBackground(isBeingEditedSp.boolValue, Color.cyan, () =>
                GUILayout.Button("Edit", GUILayout.Width(50), GUILayout.Height(EditorGUIUtility.singleLineHeight * 0.9f))
            );
            if (clicked)
            {
                isBeingEditedSp.boolValue = !isBeingEditedSp.boolValue;
            }
        }

        private static bool TryDescribeEvent(string eventName, out string description)
        {
            description = eventName switch
            {
                "Awake" => MsgDescribeAwake,
                "OnEnable" => MsgDescribeOnEnable,
                "OnDisable" => MsgDescribeOnDisable,
                "Update" => MsgDescribeOnUpdate,
                "OnPress" => MsgDescribeOnPress,
                "OnRelease" => MsgDescribeOnRelease,
                _ => null
            };
            return description != null;
        }
    }
}
