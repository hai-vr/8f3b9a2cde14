using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12Utilities
    {
        public static void RemoveDestroyedFromList(List<Component> listToClean)
        {
            for (var i = listToClean.Count - 1; i >= 0; i--)
            {
                if (null == listToClean[i])
                {
                    listToClean.RemoveAt(i);
                }
            }
        }

        /// Enables or disables a component, when applicable. If the component is a transform, then by convention, its GameObject is set active or inactive.
        /// If the component does not have a .enabled state, it is a no-op.
        public static void SetToggleState(Component component, bool isOn)
        {
            // (counter-intuitively, .enabled does not imply Behaviour)
            switch (component)
            {
                case Transform: component.gameObject.SetActive(isOn); break;
                case Behaviour thatBehaviour: thatBehaviour.enabled = isOn; break;
                case Renderer thatRenderer: thatRenderer.enabled = isOn; break;
                case Collider thatCollider: thatCollider.enabled = isOn; break;
                case Cloth thatCloth: thatCloth.enabled = isOn; break;
                case LODGroup thatLod: thatLod.enabled = isOn; break;
                // else, there is no effect on other components as they may not have a .enabled property.
            }
        }

        /// Get the enabled state of a component. If the component is a transform, then by convention, we get the activeSelf of its GameObject.
        /// If the component does not have a .enabled state, this returns true.
        public static bool GetToggleState(Component component)
        {
            // (counter-intuitively, .enabled does not imply Behaviour)
            return component switch
            {
                Transform => component.gameObject.activeSelf,
                Behaviour thatBehaviour => thatBehaviour.enabled,
                Renderer thatRenderer => thatRenderer.enabled,
                Collider thatCollider => thatCollider.enabled,
                Cloth thatCloth => thatCloth.enabled,
                LODGroup thatLod => thatLod.enabled,
                _ => true
            };
        }
    }
}
