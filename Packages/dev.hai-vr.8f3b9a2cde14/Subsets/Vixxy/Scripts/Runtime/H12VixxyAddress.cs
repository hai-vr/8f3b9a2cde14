using System;
using System.Collections.Generic;

namespace Hai.Project12.Vixxy.Runtime
{
    public class H12VixxyAddress
    {
        private static readonly Dictionary<string, int> AddressToIdDict = new();
        private static readonly Dictionary<int, string> IdToAddressDict = new(); // TODO: Could probably make a List and stop using _nextId
        private static int _nextId = 1;

        public static int AddressToId(string address)
        {
            if (AddressToIdDict.TryGetValue(address, out var id)) return id;

            var newId = _nextId;
            AddressToIdDict.Add(address, newId);
            IdToAddressDict.Add(newId, address);
            _nextId++;

            return newId;
        }

        public static string ResolveKnownAddressFromId(int iddress)
        {
            if (IdToAddressDict.TryGetValue(iddress, out var id)) return id;
            throw new IndexOutOfRangeException();
        }
    }
}
