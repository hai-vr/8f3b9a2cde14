#if UNITY_EDITOR
using Hai.Project12.HaiSystems.Supporting;
using UnityEditor;
using UnityEngine;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    public class P12AutoDeleteEOInScene : MonoBehaviour
    {
        private void Awake()
        {
            var eoObjs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in eoObjs)
            {
                if (obj && obj.CompareTag("EditorOnly"))
                {
                    if (!PrefabUtility.IsPartOfPrefabInstance(obj))
                    {
                        Destroy(obj);
                    }
                    else
                    {
                        // This seems to happen if we have additive scenes, but this is weird.
                        BasisDebug.Log($"{nameof(P12AutoDeleteEOInScene)} tried to delete {obj.name} (at {H12Utilities.ResolveAbsolutePath(obj.transform)})," +
                                       $" but it is part of a prefab instance (how??? is this caused by additive scenes?)." +
                                       $" Application.isPlaying returns {Application.isPlaying}");
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
#endif
