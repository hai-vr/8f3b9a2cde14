#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;

namespace Hai.Project12.HaiSystems.Editor
{
    [InitializeOnLoad]
    public class H12EditorMenuOptions
    {
        /// Sometimes the Basis project just gets borked if we entered Play Mode twice.
        /// Add a button to force recompile even if nothing changed.
        [MenuItem("Tools/Project12/Force Recompile even with no changes")]
        public static void ForceRecompile()
        {
            EditorApplication.isPlaying = false;
            AssetDatabase.Refresh();
            if (!EditorApplication.isCompiling) CompilationPipeline.RequestScriptCompilation();
        }
    }
}
#endif
