using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.InteractionElements.Runtime
{
    public class P12GadgetRepository : MonoBehaviour
    {
        private readonly List<ScriptableObject> _repository = new List<ScriptableObject>();

        public event GadgetListChanged OnGadgetListChanged;
        public delegate void GadgetListChanged();

        public void Add(ScriptableObject menuItem)
        {
            if (_repository.Contains(menuItem)) return;

            _repository.Add(menuItem);
            OnGadgetListChanged?.Invoke();
        }

        public void Remove(ScriptableObject menuItem)
        {
            var removalDone = _repository.Remove(menuItem);
            if (removalDone)
            {
                OnGadgetListChanged?.Invoke();
            }
        }

        // TODO: Give a read-only view of that list
        public List<ScriptableObject> GadgetView()
        {
            return _repository;
        }
    }
}
