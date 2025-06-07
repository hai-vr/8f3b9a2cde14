using System.Collections.Generic;
using UnityEngine;

namespace Hai.Project12.HaiSystems.Supporting
{
    public static class H12Utilities
    {
        public static void RemoveNullsFromList(List<Component> listToClean)
        {
            for (var i = listToClean.Count - 1; i >= 0; i--)
            {
                if (!listToClean[i])
                {
                    listToClean.RemoveAt(i);
                }
            }
        }
    }
}
