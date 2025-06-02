using UnityEngine;
using Object = UnityEngine.Object;

namespace Subsets.GameRoutine.Scripts.Runtime
{
    public class P12TriggerChangelevel : MonoBehaviour
    {
        [SerializeField] private Object asset;
        [SerializeField] private P12GameLevelManagement gameLevelManagement;

        private void Start()
        {
            // FIXME: We need gameLevelManagement to be injected on load.
            // Maybe we need to use some service locator/injector right when the scene loads.
            gameLevelManagement = Object.FindAnyObjectByType<P12GameLevelManagement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsColliderPlayer(other))
            {
                // FIXME: The loading screen UI should be managed; not handled by the caller
                gameLevelManagement.Load($"Assets/_Hai_BasisCyanKey/{asset.name}.unity", (id, progress, description) => { });
            }
        }

        private static bool IsColliderPlayer(Collider other)
        {
            return other is CharacterController;
        }
    }
}
