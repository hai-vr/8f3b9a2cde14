using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12Utilities
    {
        /// Given a list of components that is known to have destroyed components in it, clean it.
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

        /// Given a list of GameObjects that is known to have destroyed GameObjects in it, clean it.
        public static void RemoveDestroyedFromList(List<GameObject> listToClean)
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
            switch (component)
            {
                case Transform: component.gameObject.SetActive(isOn); break;
                case Behaviour thatBehaviour: thatBehaviour.enabled = isOn; break;

                // Counter-intuitively, .enabled does not imply Behaviour.
                // The following should cover all known components.
                // (CharacterController is a Collider, and SpriteMask is a Renderer).
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
            return component switch
            {
                Transform => component.gameObject.activeSelf,
                Behaviour thatBehaviour => thatBehaviour.enabled,

                // Counter-intuitively, .enabled does not imply Behaviour.
                // The following should cover all known components.
                // (CharacterController is a Collider, and SpriteMask is a Renderer).
                Renderer thatRenderer => thatRenderer.enabled,
                Collider thatCollider => thatCollider.enabled,
                Cloth thatCloth => thatCloth.enabled,
                LODGroup thatLod => thatLod.enabled,
                _ => true
            };
        }

        public static string ResolveAbsolutePath(Transform element)
        {
            return ResolveRelativePathInternal(null, element);
        }

        /// Gets the path of child relative to root, like an animation path that ignores the presence of Animator components.
        public static string ResolveRelativePath(Transform root, Transform child)
        {
            if (root == null) throw new ArgumentNullException(nameof(root)); // Explicitly disallow null for this public method.

            return ResolveRelativePathInternal(root, child);
        }

        private static string ResolveRelativePathInternal(Transform rootNullable, Transform child)
        {
            if (rootNullable == child)
            {
                return "";
            }

            if (child.parent != rootNullable && child.parent != null)
            {
                return $"{ResolveRelativePathInternal(rootNullable, child.parent)}/{child.name}";
            }

            return child.name;
        }

        // Calculates a SHA1 hash of a string, to mimic hashes of commits. The default length is 7, to mimic the default length of
        // short hashes on git. Do not use this for cryptographic purposes, this is meant for use in the creation of unique names for
        // parameters.
        public static string SimpleSha1(string str, int length = 7)
        {
            using var sha = SHA1.Create();
            return BitConverter.ToString(sha.ComputeHash(new UTF8Encoding().GetBytes(str)))
                .Replace("-", "")
                .ToLowerInvariant()
                .Substring(0, length);
        }
    }
}
