using Hai.EmbeddedFunctions.Editor.ExternalLicense;
using Hai.Project12.Vixxy.Runtime;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.Vixxy.Editor
{
    [CustomEditor(typeof(P12VixxyControl))]
    public class P12VixxyControlEditor : UnityEditor.Editor
    {
        internal static readonly Color RuntimeColorOK = Color.cyan;
        internal static readonly Color RuntimeColorKO = new Color(1f, 0.72f, 0f);
        private const string MsgCannotEditInPlayMode = "Editing this component during Play Mode can lead to different visual and scene results than editing the component in Edit Mode.";

        internal const float DeleteButtonWidth = 40;

        private const string UserViewLabel = "User View";
        private const string CreatorViewLabel = "Creator View";
        private const string DeveloperViewLabel = "Developer View";

        private static bool _userViewFoldout;
        private static bool _creatorViewFoldout;
        private static bool _developerViewFoldout;

        private H12VixxyLayoutChangeProperties _changeProperties;
        private H12VixxyLayoutDeveloperView _developerView;

        private void OnEnable()
        {
            _changeProperties = new H12VixxyLayoutChangeProperties(this);
            _developerView = new H12VixxyLayoutDeveloperView(this);
        }

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
            if (_creatorViewFoldout)
            {
                if (_changeProperties.Layout()) return;
            }
            _developerViewFoldout = HaiEFCommon.LilFoldout(DeveloperViewLabel, "", _developerViewFoldout, ref anyChanged);
            if (_developerViewFoldout)
            {
                if (_developerView.Layout()) return;
            }

            var wasModified = serializedObject.hasModifiedProperties;
            serializedObject.ApplyModifiedProperties();
            if (wasModified && Application.isPlaying)
            {
                my.DebugOnly_ReBakeControl();
            }

            if (_developerViewFoldout)
            {
                DrawDefaultInspector();
            }
        }
    }
}
