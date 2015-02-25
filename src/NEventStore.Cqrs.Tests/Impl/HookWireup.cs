using System;

namespace NEventStore.Cqrs.Tests.Impl
{
    public class HookWireup : Wireup
    {
        public HookWireup(Wireup inner)
            : base(inner)
        {
        }

        public HookWireup Hook(Action<NanoContainer> method)
        {
            method(Container);

            return this;
        }
    }
}