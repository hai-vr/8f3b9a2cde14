using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.InteractionElements
{
    public class P12GadgetRepository : MonoBehaviour
    {
        private readonly List<P12GadgetMenuItem> _repository = new List<P12GadgetMenuItem>();

        public event GadgetListChanged OnGadgetListChanged;
        public delegate void GadgetListChanged();

        public void Add(P12GadgetMenuItem menuItem)
        {
            if (_repository.Contains(menuItem)) return;

            _repository.Add(menuItem);
            OnGadgetListChanged?.Invoke();
        }

        public void Remove(P12GadgetMenuItem menuItem)
        {
            var removalDone = _repository.Remove(menuItem);
            if (removalDone)
            {
                OnGadgetListChanged?.Invoke();
            }
        }

        // TODO: Give a read-only view of that list
        public List<P12GadgetMenuItem> GadgetView()
        {
            return _repository;
        }
    }
}
