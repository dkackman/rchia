using System;

namespace rchia.Commands
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CommandTargetAttribute : Attribute
    {
    }
}
