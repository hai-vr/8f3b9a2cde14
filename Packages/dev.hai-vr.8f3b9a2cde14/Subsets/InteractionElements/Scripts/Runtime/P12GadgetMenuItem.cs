using Hai.Project12.HaiSystems.Supporting;
using UnityEngine;

namespace Hai.Project12.InteractionElements
{
    public class P12GadgetMenuItem : MonoBehaviour
    {
        [LateInjectable] [SerializeField] private P12GadgetRepository repository;

        private void Awake()
        {
            H12LateInjector.InjectDependenciesInto(this);
        }

        private void OnEnable()
        {
            repository.Add(this);
        }

        private void OnDisable()
        {
            repository.Remove(this);
        }
    }
}
