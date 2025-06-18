using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12ComponentDictionary
    {
        public static readonly Dictionary<string, Type> ComponentDictionary = new();

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
