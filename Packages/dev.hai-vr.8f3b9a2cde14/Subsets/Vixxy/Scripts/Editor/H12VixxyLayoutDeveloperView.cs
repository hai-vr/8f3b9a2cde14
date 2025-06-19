using System;
using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.Vixxy.Runtime;
using Hai.Project12.HaiSystems.Editor;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.Vixxy.Editor
{
    public class H12VixxyLayoutDeveloperView
    {
        private const string AddPropertyOfTypeLabel = "+ Add Property of type {0}";
        private const string AddSubjectLabel = "+ Add Subject";
        private const string MsgAddressIsOptional = "Address is completely optional, we will generate one for you. If you need explicit control by external programs, then do specify one.";
        private const string MsgOnAvatarReadyNotInvoked = "OnAvatarReady was not called on the avatar of this component while we were listening.\nThis may be because this is a test scene and not a loaded avatar. If this isn't the case, this is a proper error.";
        private const string MsgPropertyFailedToResolve = "This property has failed to resolve. Reason: {0}";
        private const string RuntimeBakedDataLabel = "Runtime Baked Data";
        private const string SampleFromLabel = "Sample from";

        // ReSharper disable once InconsistentNaming
        private readonly P12VixxyControl my;
        // ReSharper disable once InconsistentNaming
        private readonly SerializedObject serializedObject;

        public H12VixxyLayoutDeveloperView(P12VixxyControlEditor editor)
        {
            my = (P12VixxyControl)editor.target;
            serializedObject = editor.serializedObject;
        }

        public bool Layout()
        {
            var isPlaying = Application.isPlaying;

            EditorGUI.BeginDisabledGroup(isPlaying);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(P12VixxyControl.address)));
            if (string.IsNullOrWhiteSpace(my.address))
            {
                EditorGUILayout.HelpBox(MsgAddressIsOptional, MessageType.Info);
            }
            EditorGUI.EndDisabledGroup();

            if (isPlaying)
            {
                HaiEFCommon.ColoredBackgroundVoid(true, P12VixxyControlEditor.RuntimeColorOK, () =>
                {
                    EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
                    EditorGUILayout.Toggle(nameof(P12VixxyControl.WasOnAvatarReadyCalled), my.WasOnAvatarReadyCalled);
                    if (!my.WasOnAvatarReadyCalled)
                    {
                        HaiEFCommon.ColoredBackgroundVoid(true, Color.white, () => { EditorGUILayout.HelpBox(MsgOnAvatarReadyNotInvoked, MessageType.Error); });
                    }
                    EditorGUILayout.Toggle(nameof(P12VixxyControl.IsWearer), my.IsWearer);
                    EditorGUILayout.TextField(nameof(P12VixxyControl.Address), my.Address);
                    EditorGUILayout.EndVertical();
                });
            }

            var subjectsSp = serializedObject.FindProperty(nameof(P12VixxyControl.subjects));
            for (var subjectIndex = 0; subjectIndex < subjectsSp.arraySize; subjectIndex++)
            {
                var subjectSp = subjectsSp.GetArrayElementAtIndex(subjectIndex);
                EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{subjectIndex}] Subject", EditorStyles.boldLabel);
                if (HaiEFCommon.ColoredBackground(true, Color.red, () => GUILayout.Button($"{H12UiHelpers.CrossSymbol}", GUILayout.Width(P12VixxyControlEditor.DeleteButtonWidth))))
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
                    EditorGUILayout.LabelField(SampleFromLabel);
                    EditorGUILayout.PropertyField(subjectSp.FindPropertyRelative(nameof(P12VixxySubject.targets))); // TODO: Only show 0th value
                }

                if (isPlaying)
                {
                    var it = my.subjects[subjectIndex];
                    HaiEFCommon.ColoredBackgroundVoid(true, it.IsApplicable ? P12VixxyControlEditor.RuntimeColorOK : P12VixxyControlEditor.RuntimeColorKO, () =>
                    {
                        // var it = (P12VixxySubject)subjectSp.boxedValue; // This doesn't work. It returns a default struct
                        EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
                        EditorGUILayout.LabelField(RuntimeBakedDataLabel, EditorStyles.boldLabel);
                        EditorGUILayout.Toggle(nameof(P12VixxySubject.IsApplicable), it.IsApplicable);
                        EditorGUILayout.LabelField(nameof(P12VixxySubject.BakedObjects));
                        if (it.IsApplicable)
                        {
                            foreach (var found in it.BakedObjects)
                            {
                                EditorGUILayout.ObjectField(found, found.GetType(), true);
                            }
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

                ButtonToAddProperty(propertiesSp, "float", () => new P12VixxyProperty<float>());
                ButtonToAddProperty(propertiesSp, "Color", () => new P12VixxyProperty<Color>());
                ButtonToAddProperty(propertiesSp, "Vector4", () => new P12VixxyProperty<Vector4>());
                ButtonToAddProperty(propertiesSp, "Vector3", () => new P12VixxyProperty<Vector3>());

                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button(AddSubjectLabel))
            {
                subjectsSp.arraySize += 1;
            }

            return false;
        }

        private static void ButtonToAddProperty(SerializedProperty propertiesSp, string name, Func<object> factoryFn)
        {
            if (GUILayout.Button(string.Format(AddPropertyOfTypeLabel, name)))
            {
                var indexToPutData = propertiesSp.arraySize;
                propertiesSp.arraySize = indexToPutData + 1;
                propertiesSp.GetArrayElementAtIndex(indexToPutData).managedReferenceValue = factoryFn.Invoke();
            }
        }

        private bool DrawPropertyOrReturn(SerializedProperty propertySp, int propertyIndex, SerializedProperty propertiesSp, bool isPlaying)
        {
            EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
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

            if (HaiEFCommon.ColoredBackground(true, Color.red, () => GUILayout.Button($"{H12UiHelpers.CrossSymbol}", GUILayout.Width(P12VixxyControlEditor.DeleteButtonWidth))))
            {
                propertiesSp.DeleteArrayElementAtIndex(propertyIndex);

                serializedObject.ApplyModifiedProperties();
                return true; // Workaround array size change error
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyPropertyBase.fullClassName)));
            EditorGUILayout.PropertyField(propertySp.FindPropertyRelative(nameof(P12VixxyPropertyBase.variant)));
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
                HaiEFCommon.ColoredBackgroundVoid(true, it.IsApplicable ? P12VixxyControlEditor.RuntimeColorOK : P12VixxyControlEditor.RuntimeColorKO, () =>
                {
                    EditorGUILayout.BeginVertical(H12UiHelpers.GroupBoxStyle);
                    EditorGUILayout.LabelField(RuntimeBakedDataLabel, EditorStyles.boldLabel);
                    EditorGUILayout.Toggle(nameof(P12VixxyPropertyBase.IsApplicable), it.IsApplicable);
                    EditorGUILayout.EnumPopup(nameof(P12VixxyPropertyBase.BakeResult), it.BakeResult);
                    if (it.IsApplicable)
                    {
                        EditorGUILayout.ObjectField(nameof(P12VixxyPropertyBase.FoundType), null, it.FoundType, false);
                        EditorGUILayout.EnumPopup(nameof(P12VixxyPropertyBase.KindMarker), it.KindMarker);
                        if (it.KindMarker == P12KindMarker.BlendShape)
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
                        HaiEFCommon.ColoredBackgroundVoid(true, Color.white, () => { EditorGUILayout.HelpBox(string.Format(MsgPropertyFailedToResolve, it.BakeResult), MessageType.Error); });
                    }
                    EditorGUILayout.EndVertical();
                });
            }

            EditorGUILayout.EndVertical();
            return false;
        }
    }
}
