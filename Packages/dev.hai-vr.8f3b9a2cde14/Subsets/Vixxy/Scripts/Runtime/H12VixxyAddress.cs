using System.Collections.Generic;

namespace Hai.Project12.Vixxy.Runtime
{
    public class H12VixxyAddress
    {
        private static readonly Dictionary<string, int> AddressToIdDict = new();
        private static int _nextId = 1;

        public static int AddressToId(string address)
        {
            if (AddressToIdDict.TryGetValue(address, out var id)) return id;

            var newId = _nextId;
            AddressToIdDict.Add(address, newId);
            _nextId++;

            return newId;
        }
    }
}
