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
        private static readonly Color RuntimeColorOK = Color.cyan;
        private static readonly Color RuntimeColorKO = new Color(1f, 0.72f, 0f);
        private const string MsgCannotEditInPlayMode = "Editing this component during Play Mode can lead to different visual and scene results than editing the component in Edit Mode.";

        private const string CrossSymbol = "×";
        private const float DeleteButtonWidth = 40;

        private const string CreatorViewLabel = "Creator View";
        private const string DeveloperViewLabel = "Developer View";
        private const string UserViewLabel = "User View";

        private static bool _userViewFoldout;
        private static bool _creatorViewFoldout;
        private static bool _developerViewFoldout;

        public override void OnInspectorGUI()
        {
            var my = (P12VixxyControl)target;

            var isPlaying = Application.isPlaying;
            if (isPlaying)
            {
                EditorGUILayout.HelpBox(MsgCannotEditInPlayMode, MessageType.Warning);
            }

            var anyChanged = false;
            _userViewFoldout = HaiEFCommon.LilFoldout(UserViewLabel, "", _userViewFoldout, ref anyChanged);
            _creatorViewFoldout = HaiEFCommon.LilFoldout(CreatorViewLabel, "", _creatorViewFoldout, ref anyChanged);
            _developerViewFoldout = HaiEFCommon.LilFoldout(DeveloperViewLabel, "", _developerViewFoldout, ref anyChanged);
            if (_developerViewFoldout)
            {
                if (DeveloperView(my)) return; // Workaround array size change error
            }
        }

        private bool DeveloperView(P12VixxyControl my)
        {
            var isPlaying = Application.isPlaying;

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
                    return true;
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

                if (isPlaying)
                {
                    var it = my.subjects[subjectIndex];
                    HaiEFCommon.ColoredBackgroundVoid(true, it.IsApplicable ? RuntimeColorOK : RuntimeColorKO, () =>
                    {
                        // var it = (P12VixxySubject)subjectSp.boxedValue; // This doesn't work. It returns a default struct
                        EditorGUILayout.BeginVertical("GroupBox");
                        EditorGUILayout.LabelField("Runtime Baked Data", EditorStyles.boldLabel);
                        EditorGUILayout.Toggle(nameof(P12VixxySubject.IsApplicable), it.IsApplicable);
                        EditorGUILayout.LabelField(nameof(P12VixxySubject.BakedObjects));
                        foreach (var found in it.BakedObjects)
                        {
                            EditorGUILayout.ObjectField(found, found.GetType(), true);
                        }
                        EditorGUILayout.EndVertical();
                    });
                }

                var propertiesSp = subjectSp.FindPropertyRelative(nameof(P12VixxySubject.properties));
                EditorGUILayout.LabelField($"Properties ({propertiesSp.arraySize})", EditorStyles.boldLabel);
                for (var propertyIndex = 0; propertyIndex < propertiesSp.arraySize; propertyIndex++)
                {
                    var propertySp = propertiesSp.GetArrayElementAtIndex(propertyIndex);
                    if (DrawPropertyOrReturn(propertySp, propertyIndex, propertiesSp, isPlaying)) return true;
                }

                AddProperty(propertiesSp, "float", () => new P12VixxyProperty<float>());
                AddProperty(propertiesSp, "Color", () => new P12VixxyProperty<Color>());
                AddProperty(propertiesSp, "Vector4", () => new P12VixxyProperty<Vector4>());
                AddProperty(propertiesSp, "Vector3", () => new P12VixxyProperty<Vector3>());

                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("+ Add Subject"))
            {
                subjectsSp.arraySize += 1;
            }

            var wasModified = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();
            if (wasModified)
            {
                my.DebugOnly_ReBakeControl();
            }

            DrawDefaultInspector();

            return false;
        }

        private static void AddProperty(SerializedProperty propertiesSp, string name, Func<object> factoryFn)
        {
            if (GUILayout.Button($"+ Add Property of type {name}"))
            {
                var indexToPutData = propertiesSp.arraySize;
                propertiesSp.arraySize = indexToPutData + 1;
                propertiesSp.GetArrayElementAtIndex(indexToPutData).managedReferenceValue = factoryFn.Invoke();
            }
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
                var it = (P12VixxyPropertyBase)managedReferenceValue;
                HaiEFCommon.ColoredBackgroundVoid(true, it.IsApplicable ? RuntimeColorOK : RuntimeColorKO, () =>
                {
                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.LabelField("Runtime Baked Data", EditorStyles.boldLabel);
                    EditorGUILayout.Toggle(nameof(P12VixxyPropertyBase.IsApplicable), it.IsApplicable);
                    if (it.IsApplicable)
                    {
                        EditorGUILayout.ObjectField(nameof(P12VixxyPropertyBase.FoundType), null, it.FoundType, false);
                        EditorGUILayout.EnumPopup(nameof(P12VixxyPropertyBase.SpecialMarker), it.SpecialMarker);
                        EditorGUILayout.TextField(nameof(P12VixxyPropertyBase.PropertySuffix), it.PropertySuffix);
                        if (it.SpecialMarker == P12SpecialMarker.BlendShape)
                        {
                            EditorGUILayout.LabelField(nameof(P12VixxyPropertyBase.SmrToBlendshapeIndex), EditorStyles.boldLabel);
                            foreach (var smrToBlendshapeIndex in it.SmrToBlendshapeIndex)
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.ObjectField(smrToBlendshapeIndex.Key, typeof(SkinnedMeshRenderer), false);
                                EditorGUILayout.TextField($"{smrToBlendshapeIndex.Value}");
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.LabelField(nameof(P12VixxyPropertyBase.FoundComponents));
                        foreach (var found in it.FoundComponents)
                        {
                            EditorGUILayout.ObjectField(found, found.GetType(), true);
                        }
                    }
                    else
                    {

                        HaiEFCommon.ColoredBackgroundVoid(true, Color.white, () => { EditorGUILayout.HelpBox("This property has failed to resolve.", MessageType.Error); });
                    }
                    EditorGUILayout.EndVertical();
                });
            }

            EditorGUILayout.EndVertical();
            return false;
        }
    }
}
