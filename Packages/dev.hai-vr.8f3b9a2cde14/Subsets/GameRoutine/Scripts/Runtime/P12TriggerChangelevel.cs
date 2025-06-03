using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Subsets.GameRoutine.Scripts.Runtime
{
    public class P12TriggerChangelevel : MonoBehaviour
    {
        [SerializeField] private Object asset;

        [LateInjectable] [SerializeField] private P12GameLevelManagement gameLevelManagement;

        private void Awake()
        {
            H12LateInjector.InjectDependenciesInto(this);
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
