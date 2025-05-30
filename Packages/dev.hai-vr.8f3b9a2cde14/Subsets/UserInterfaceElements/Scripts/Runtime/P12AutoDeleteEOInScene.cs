using UnityEngine;

namespace Hai.Project12.UserInterfaceElements
{
    public class P12AutoDeleteEOInScene : MonoBehaviour
    {
        private void Start()
        {
            foreach (var obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (obj && obj.CompareTag("EditorOnly"))
                    Destroy(obj);
            Destroy(gameObject);
        }
    }
}
