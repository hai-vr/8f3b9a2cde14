using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public class P12Permanence : MonoBehaviour
    {
        private void Awake()
        {
            Object.DontDestroyOnLoad(this);
        }
    }
}
