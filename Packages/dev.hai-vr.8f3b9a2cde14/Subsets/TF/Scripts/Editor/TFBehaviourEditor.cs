using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.TF.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.TF.Editor
{
    [CustomEditor(typeof(TFBehaviour))]
    public class TFBehaviourEditor : UnityEditor.Editor
    {
        private const string ApplyLabel = "Apply";
        private const string ApplyWithoutRefreshing = "Apply without refreshing";
        private const string RefillFieldsLabel = "Refill Fields";
        private const string DeveloperViewLabel = "Developer View";
        private const string AdvancedViewLabel = "Advanced View";

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

            if (GUILayout.Button(ApplyLabel))
            {
                var compiler = new TFCodeGen((TFBehaviour)target);
                compiler.Generate();
            }

            if (GUILayout.Button(ApplyWithoutRefreshing))
            {
                var compiler = new TFCodeGen((TFBehaviour)target);
                compiler.Generate(false);
            }

            if (GUILayout.Button(RefillFieldsLabel))
            {
                var compiler = new TFCodeGen((TFBehaviour)target);
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TFBehaviour.addObjectNamesInCode)));
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
