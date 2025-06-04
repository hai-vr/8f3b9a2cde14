using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.Project12.HaiSystems.Supporting
{
    public class H12LateInjector
    {
        // TODO: Some of the requested objects might become destroyed, but this puts the object lifecycle of the dependents into question.
        private static readonly Dictionary<Type, Object> RequestedTypeToObjectDict = new Dictionary<Type, Object>();
        private static P12LateInjectorMarker _lateInjectorMarker; // May become destroyed

        /// Fills non-null fields marked with the LateInjectable attribute with any instance of it found in any scene. Call this on Awake().
        public static void InjectDependenciesInto(Component toInjectTo)
        {
            if (!_lateInjectorMarker)
            {
                H12Debug.Log("Initializing injector.");
                _lateInjectorMarker = new GameObject
                {
                    name = "P12LateInjectorMarker"
                }.AddComponent<P12LateInjectorMarker>();
                Object.DontDestroyOnLoad(_lateInjectorMarker);

                RequestedTypeToObjectDict.Clear();
            }

            var type = toInjectTo.GetType();
            var fields = GetRelevantFields(type);
            foreach (var field in fields)
            {
                var injectable = field.GetCustomAttribute<LateInjectable>();
                if (injectable != null && field.GetValue(toInjectTo) == null)
                {
                    var requestedType = field.FieldType;
                    if (RequestedTypeToObjectDict.TryGetValue(requestedType, out var storedInstance))
                    {
                        field.SetValue(toInjectTo, storedInstance);
                    }
                    else
                    {
                        var foundInstance = Object.FindAnyObjectByType(requestedType, FindObjectsInactive.Include);
                        if (foundInstance != null)
                        {
                            RequestedTypeToObjectDict[requestedType] = foundInstance;
                            field.SetValue(toInjectTo, foundInstance);
                        }
                        else
                        {
                            H12Debug.LogError($"LateInjectable {toInjectTo.name} needs an object of type {requestedType.Name} for field {field.Name}, but we cannot find one.");
                        }
                    }
                }
            }
        }

        private static FieldInfo[] GetRelevantFields(Type requestorType)
        {
            return requestorType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
