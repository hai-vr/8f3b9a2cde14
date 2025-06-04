using UnityEngine;

namespace Hai.Project12.UserInterfaceElements.Runtime
{
    public class P12AutoDeleteEOInScene : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (obj && obj.CompareTag("EditorOnly"))
                    Destroy(obj);
            Destroy(gameObject);
        }
    }
}
