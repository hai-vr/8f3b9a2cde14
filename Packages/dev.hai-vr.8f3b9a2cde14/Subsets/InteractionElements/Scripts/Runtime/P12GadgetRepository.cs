using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.InteractionElements
{
    public class P12GadgetRepository : MonoBehaviour
    {
        private readonly List<P12GadgetMenuItem> _repository = new List<P12GadgetMenuItem>();

        public void Add(P12GadgetMenuItem menuItem)
        {
            if (_repository.Contains(menuItem)) return;

            _repository.Add(menuItem);
        }

        public void Remove(P12GadgetMenuItem menuItem)
        {
            _repository.Remove(menuItem);
        }
    }
}
