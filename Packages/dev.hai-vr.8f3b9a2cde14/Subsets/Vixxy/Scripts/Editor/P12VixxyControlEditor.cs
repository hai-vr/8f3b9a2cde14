using System;
using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.Vixxy.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.Vixxy.Editor
{
    [CustomEditor(typeof(P12VixxyControl))]
    public class P12VixxyControlEditor : UnityEditor.Editor
    {
        private const string MsgCannotEditInPlayMode = "Editing this component currently has no effect during Play Mode.";

        private const string CrossSymbol = "×";
        private const float DeleteButtonWidth = 40;

        public override void OnInspectorGUI()
        {
            var my = (P12VixxyControl)target;

            var isPlaying = Application.isPlaying;
            if (isPlaying)
            {
                EditorGUILayout.HelpBox(MsgCannotEditInPlayMode, MessageType.Warning);
            }

            EditorGUI.BeginDisabledGroup(isPlaying);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(P12VixxyControl.address)));
            EditorGUI.EndDisabledGroup();

            var subjectsSp = serializedObject.FindProperty(nameof(P12VixxyControl.subjects));
            for (var subjectIndex = 0; subjectIndex < subjectsSp.arraySize; subjectIndex++)
            {
                var subjectSp = subjectsSp.GetArrayElementAtIndex(subjectIndex);
                EditorGUILayout.BeginVertical("GroupBox");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{subjectIndex}] Subject", EditorStyles.boldLabel);
                if (HaiEFCommon.ColoredBackground(true, Color.red, () => GUILayout.Button($"{CrossSymbol}", GUILayout.Width(DeleteButtonWidth))))
                {
                    subjectsSp.DeleteArrayElementAtIndex(subjectIndex);

                    serializedObject.ApplyModifiedProperties();
                    return; // Workaround array size change error
                }
                EditorGUILayout.EndHorizontal();

                var selectionSp = subjectSp.FindPropertyRelative(nameof(P12VixxySubject.selection));
                EditorGUILayout.PropertyField(selectionSp);
                var selection = (P12VixxySelection)selectionSp.intValue;
                if (selection == P12VixxySelection.Normal)
                {
                    EditorGUILayout.PropertyField(subjectSp.FindPropertyRelative(nameof(P12VixxySubject.targets)));
                }
                else if (selection == P12VixxySelection.RecursiveSearch)
                {
                    EditorGUILayout.PropertyField(subjectSp.FindPropertyRelative(nameof(P12VixxySubject.childrenOf)));
                    EditorGUILayout.PropertyField(subjectSp.FindPropertyRelative(nameof(P12VixxySubject.exceptions)));
                }

                if (selection != P12VixxySelection.Normal)
                {
                    EditorGUILayout.LabelField("Sample from");
                    EditorGUILayout.PropertyField(subjectSp.FindPropertyRelative(nameof(P12VixxySubject.targets))); // TODO: Only show 0th value
                }

                var propertiesSp = subjectSp.FindPropertyRelative(nameof(P12VixxySubject.properties));
                EditorGUILayout.LabelField($"Properties array size is {propertiesSp.arraySize}");
                for (var propertyIndex = 0; propertyIndex < propertiesSp.arraySize; propertyIndex++)
                {
                    var propertySp = propertiesSp.GetArrayElementAtIndex(propertyIndex);
                    if (DrawPropertyOrReturn(propertySp, propertyIndex, propertiesSp, isPlaying)) return;
                }

                if (GUILayout.Button("+ Add Property of type FLOAT"))
                {
                    var indexToPutData = propertiesSp.arraySize;
                    propertiesSp.arraySize = indexToPutData + 1;
                    propertiesSp.GetArrayElementAtIndex(indexToPutData).managedReferenceValue = new P12VixxyProperty<float>();
                }
                if (GUILayout.Button("+ Add Property of type VECTOR4"))
                {
                    var indexToPutData = propertiesSp.arraySize;
                    propertiesSp.arraySize = indexToPutData + 1;
                    propertiesSp.GetArrayElementAtIndex(indexToPutData).managedReferenceValue = new P12VixxyProperty<Vector4>();
                }
                if (GUILayout.Button("+ Add Property of type VECTOR3"))
                {
                    var indexToPutData = propertiesSp.arraySize;
                    propertiesSp.arraySize = indexToPutData + 1;
                    propertiesSp.GetArrayElementAtIndex(indexToPutData).managedReferenceValue = new P12VixxyProperty<Vector3>();
                }

                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Add Subject"))
            {
                subjectsSp.arraySize += 1;
            }

            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();
        }

        private bool DrawPropertyOrReturn(SerializedProperty propertySp, int propertyIndex, SerializedProperty propertiesSp, bool isPlaying)
        {
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            var managedReferenceValue = propertySp.managedReferenceValue;
            var managedReferenceValueType = managedReferenceValue.GetType();

            var isGenericVixxyProperty = false;
            Type genericType = null;
            if (managedReferenceValueType.IsGenericType && managedReferenceValueType.GetGenericTypeDefinition() == typeof(P12VixxyProperty<>))
            {
                genericType = managedReferenceValueType.GetGenericArguments()[0];
                isGenericVixxyProperty = true;
            }

            if (isGenericVixxyProperty)
            {
                EditorGUILayout.LabelField($"[{propertyIndex}] Property of type {genericType.Name}", EditorStyles.boldLabel);
            }
            else if (managedReferenceValue is P12VixxyPropertyBase)
            {
                EditorGUILayout.LabelField($"[{propertyIndex}] Specialized Property of type {managedReferenceValueType.Name}", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"[{propertyIndex}] CAUTION: Not a P12VixxyPropertyBase, type is {managedReferenceValueType.FullName}", EditorStyles.boldLabel);
            }

            if (HaiEFCommon.ColoredBackground(true, Color.red, () => GUILayout.Button($"{CrossSymbol}", GUILayout.Width(DeleteButtonWidth))))
            {
                propertiesSp.DeleteArrayElementAtIndex(propertyIndex);

                serializedObject.ApplyModifiedProperties();
                return true; // Workaround array size change error
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyPropertyBase.fullClassName)));
            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyPropertyBase.propertyName)));
            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyPropertyBase.flip)));
            if (isGenericVixxyProperty)
            {
                EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyProperty<object>.unbound)));
                EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyProperty<object>.bound)));
            }

            if (isPlaying)
            {
                var propertyBase = (P12VixxyPropertyBase)managedReferenceValue;
                if (propertyBase.IsApplicable)
                {
                    foreach (var found in propertyBase.FoundComponents)
                    {
                        if (null != found)
                        {
                            EditorGUILayout.ObjectField(found, found.GetType(), true);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("This property has failed to resolve.", MessageType.Error);
                }
            }

            EditorGUILayout.EndVertical();
            return false;
        }
    }
}
