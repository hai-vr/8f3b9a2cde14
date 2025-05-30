using UnityEngine;

public class P12AutoDeleteEOInScene : MonoBehaviour
{
    void Start()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("EditorOnly"))
        {
            if (obj)
            {
                Object.Destroy(obj);
            }
        }
        Object.Destroy(gameObject);
    }
}
