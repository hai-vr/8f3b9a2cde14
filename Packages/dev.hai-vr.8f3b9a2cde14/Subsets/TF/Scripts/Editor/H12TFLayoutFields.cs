using System;
using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.HaiSystems.Editor;
using Hai.Project12.TF.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.TF.Editor
{
    internal class H12TFLayoutFields
    {
        internal const float TargetTypeWidth = 100;

        private const string FieldsLabel = "Fields";

        private readonly TFBehaviour my;
        private readonly SerializedObject serializedObject;
        private readonly TFBehaviourEditor editor;

        private bool _initialized;
        private GUIStyle _richText;
        private string _colorVariableOrField;
        private Color _colorVariableOrFieldCol;
        private GUIStyle _coloredTextField;

        public H12TFLayoutFields(TFBehaviourEditor editor)
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
                _colorVariableOrFieldCol = new Color(0.48f, 0.92f, 0.48f);
                _coloredTextField = new GUIStyle(EditorStyles.textField) { normal = { textColor = _colorVariableOrFieldCol } };
                _colorVariableOrField = ColorUtility.ToHtmlStringRGB(_colorVariableOrFieldCol);
                _initialized = true;
            }

            EditorGUILayout.LabelField($"<b>{FieldsLabel}</b> <color=#{_colorVariableOrField}>(@)</color>", _richText);
            EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
            var fieldsSp = serializedObject.FindProperty(nameof(TFBehaviour.fields));
            for (var i = 0; i < fieldsSp.arraySize; i++)
            {
                var fieldSp = fieldsSp.GetArrayElementAtIndex(i);
                var valueSp = fieldSp.FindPropertyRelative(nameof(TFField.value));
                var targetTypeSp = valueSp.FindPropertyRelative(nameof(TFValue.targetType));
                var guidSp = fieldSp.FindPropertyRelative(nameof(TFField.internalGuid));
                if (string.IsNullOrWhiteSpace(guidSp.stringValue))
                {
                    guidSp.stringValue = Guid.NewGuid().ToString();
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(targetTypeSp, GUIContent.none, GUILayout.Width(TargetTypeWidth));
                EditorGUILayout.PropertyField(fieldSp.FindPropertyRelative(nameof(TFField.name)), GUIContent.none);
                ValueObjectField(targetTypeSp, valueSp);
                if (editor._developerViewFoldout)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(guidSp, GUIContent.none, GUILayout.Width(80));
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private static void ValueObjectField(SerializedProperty targetTypeSp, SerializedProperty valueSp)
        {
            var targetType = (TFParameterTargetType)targetTypeSp.intValue;
            switch (targetType)
            {
                case TFParameterTargetType.String:
                    EditorGUILayout.PropertyField(valueSp.FindPropertyRelative(nameof(TFValue.stringValue)), GUIContent.none);
                    break;
                case TFParameterTargetType.Object:
                    EditorGUILayout.PropertyField(valueSp.FindPropertyRelative(nameof(TFValue.objectValue)), GUIContent.none);
                    break;
                case TFParameterTargetType.Boolean:
                    EditorGUILayout.PropertyField(valueSp.FindPropertyRelative(nameof(TFValue.boolValue)), GUIContent.none);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
