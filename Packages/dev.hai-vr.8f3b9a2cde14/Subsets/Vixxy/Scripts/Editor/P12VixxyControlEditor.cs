using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.Vixxy.Runtime;
using UnityEditor;
using UnityEngine;

namespace Subsets.Vixxy.Scripts.Editor
{
    [CustomEditor(typeof(P12VixxyControl))]
    public class P12VixxyControlEditor : UnityEditor.Editor
    {
        private const string CrossSymbol = "×";
        private const float DeleteButtonWidth = 40;

        public override void OnInspectorGUI()
        {
            var my = (P12VixxyControl)target;

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
                    if (DrawPropertyOrReturn(propertySp, propertyIndex, propertiesSp)) return;
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

                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Add Subject"))
            {
                subjectsSp.arraySize += 1;
            }

            serializedObject.ApplyModifiedProperties();

            DrawDefaultInspector();
        }

        private bool DrawPropertyOrReturn(SerializedProperty propertySp, int propertyIndex, SerializedProperty propertiesSp)
        {
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUILayout.BeginHorizontal();
            var genericType = propertySp.managedReferenceValue.GetType().GetGenericArguments()[0];
            if (propertySp.managedReferenceValue is P12VixxyPropertyBase)
            {
                EditorGUILayout.LabelField($"[{propertyIndex}] Property of type {genericType.Name}", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"[{propertyIndex}] CAUTION: Not a P12VixxyPropertyBase, type is {propertySp.managedReferenceValue.GetType().FullName}", EditorStyles.boldLabel);
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
            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyProperty<object>.unbound)));
            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyProperty<object>.bound)));
            EditorGUILayout.EndVertical();
            return false;
        }
    }
}
