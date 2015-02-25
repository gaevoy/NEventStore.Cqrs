using System;

namespace NEventStore.Cqrs.Tests.Impl
{
    public static class HookWireupExtensions
    {
        public static HookWireup Hook(this Wireup wireup, Action<NanoContainer> method)
        {
            return new HookWireup(wireup).Hook(method);
        }
    }
}