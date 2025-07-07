using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.TF.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.TF.Editor
{
    [CustomEditor(typeof(TFBehaviour))]
    public class TFBehaviourEditor : UnityEditor.Editor
    {
        private const string ApplyAndRefreshLabel = "Apply and Refresh";
        private const string ApplyWithoutRefreshing = "Apply";
        private const string RefillFieldsLabel = "Refill Fields";
        private const string DeveloperViewLabel = "Developer View";
        private const string AdvancedViewLabel = "Advanced View";
        private const string MsgObjectNamesAffectsEditing = "Adding object names in code can slow down editing, as it makes the code depend on the object name.";

        private H12TFLayoutFields _layoutFields;
        private H12TFLayoutEvents _layoutEvents;
        internal bool _developerViewFoldout;
        internal bool _advancedViewFoldout;

        private bool _initialized;
        private GUIStyle _richTextWrapItalic;

        private void OnEnable()
        {
            _layoutFields = new H12TFLayoutFields(this);
            _layoutEvents = new H12TFLayoutEvents(this);
        }

        public override void OnInspectorGUI()
        {
            if (!_initialized)
            {
                _richTextWrapItalic = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
                _initialized = true;
            }

            var descriptionSp = serializedObject.FindProperty(nameof(TFBehaviour.description));
            if (!string.IsNullOrWhiteSpace(descriptionSp.stringValue))
            {
                EditorGUILayout.LabelField($"<i>{descriptionSp.stringValue}</i>", _richTextWrapItalic);
                EditorGUILayout.Separator();
            }

            _layoutFields.Layout();
            _layoutEvents.Layout();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(ApplyAndRefreshLabel))
            {
                var compiler = new H12TFCodeGen((TFBehaviour)target);
                compiler.Generate();
            }

            if (GUILayout.Button(ApplyWithoutRefreshing, GUILayout.Width(100)))
            {
                var compiler = new H12TFCodeGen((TFBehaviour)target);
                compiler.Generate(false);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(RefillFieldsLabel))
            {
                var compiler = new H12TFCodeGen((TFBehaviour)target);
                compiler.RefillFields();
            }

            EditorGUILayout.Separator();

            //

            var anyChanged = false;

            _advancedViewFoldout = HaiEFCommon.LilFoldout(AdvancedViewLabel, "", _advancedViewFoldout, ref anyChanged);
            if (_advancedViewFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TFBehaviour.description)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TFBehaviour.supportLegacyPlatform)));
                var addObjectNamesInCodeSp = serializedObject.FindProperty(nameof(TFBehaviour.addObjectNamesInCode));
                EditorGUILayout.PropertyField(addObjectNamesInCodeSp);
                if (addObjectNamesInCodeSp.boolValue)
                {
                    EditorGUILayout.HelpBox(MsgObjectNamesAffectsEditing, MessageType.Warning);
                }
            }

            _developerViewFoldout = HaiEFCommon.LilFoldout(DeveloperViewLabel, "", _developerViewFoldout, ref anyChanged);
            if (_developerViewFoldout)
            {
            }

            var wasModified = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();

            if (_developerViewFoldout)
            {
                DrawDefaultInspector();
            }
        }
    }
}
