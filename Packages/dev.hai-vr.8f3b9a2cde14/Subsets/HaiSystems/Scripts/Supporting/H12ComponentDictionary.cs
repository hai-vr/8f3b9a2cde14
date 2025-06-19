using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12ComponentDictionary
    {
        private static readonly Dictionary<string, Type> ComponentDictionary = new();

        public static bool TryGetComponentType(string fullClassName, out Type foundType)
        {
            return ComponentDictionary.TryGetValue(fullClassName, out foundType);
        }

        static H12ComponentDictionary()
        {
            // This whole operation takes a non-negligible amount of time, so only do it once.
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(Component).IsAssignableFrom(type))
                    {
                        ComponentDictionary.TryAdd(type.FullName, type);
                    }
                }
            }
        }
    }
}
