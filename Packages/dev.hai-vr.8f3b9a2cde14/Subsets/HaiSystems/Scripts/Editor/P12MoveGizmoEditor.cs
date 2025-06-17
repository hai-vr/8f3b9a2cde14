using Hai.Project12.HaiSystems.Supporting;
using UnityEditor;

namespace Hai.Project12.HaiSystems.Editor
{
    /// Helper class, so that we can move an object in the scene while continuing to select a MeshCollider
    /// hierarchy to make them visible in the scene.
    [CustomEditor(typeof(P12MoveGizmo))]
    public class P12MoveGizmoEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var my = (P12MoveGizmo)target;

            var transformPosition = my.transform.position;
            var transformRotation = my.transform.rotation;
            EditorGUI.BeginChangeCheck();
            Handles.TransformHandle(ref transformPosition, ref transformRotation);
            if (EditorGUI.EndChangeCheck())
            {
                my.transform.position = transformPosition;
                my.transform.rotation = transformRotation;
            }
        }
    }
}
