using System;

namespace Hai.Project12.HaiSystems.Supporting
{
    /// Denotes a field that can always be injected before Play Mode is even entered,
    /// because the instance always comes alongside it with the same scene or prefab.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EarlyInjectable : Attribute
    {
    }

    /// Denotes a field that can only be injected late, because the instance comes from
    /// another scene or an unconnected prefab.
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LateInjectable : Attribute
    {
    }
}
