using CommonDomain.Core;
using NEventStore.Cqrs.Messages;
using PetProject.Books.Shared.Commands;
using PetProject.Books.Shared.Events;
using System;

namespace PetProject.Books.Domain
{
    class AccountValueTranferring : SagaBase<IMessage>
    {
        Guid from;
        Guid to;
        decimal transferredValue;
        bool failed;

        public AccountValueTranferring()
        {
            Register<AccountTransferRequested>(Apply);
            Register<AccountValueChanged>(Apply);
            Register<CommandDispatchFailedEvent>(Apply);
        }

        public static bool CanHandle(CommandDispatchFailedEvent evt)
        {
            return evt.CommandType == typeof(ChangeAccountValue).ToString();
        }

        void Apply(AccountTransferRequested evt)
        {
            Id = evt.SagaId.Value.ToString();
            from = evt.From;
            to = evt.To;
            transferredValue = evt.Value;
            Dispatch(new ChangeAccountValue { Id = from, Value = -transferredValue });
        }

        void Apply(AccountValueChanged evt)
        {
            if (failed) return;

            if (evt.Id == from)
                Dispatch(new ChangeAccountValue { Id = to, Value = transferredValue });
            if (evt.Id == to)
            {
                // Tranferring is completed
            }
        }

        void Apply(CommandDispatchFailedEvent evt)
        {
            Dispatch(new CancelAccountChange { Id = from, Reason = evt.ErrorMessage });
            Dispatch(new CancelAccountChange { Id = to, Reason = evt.ErrorMessage });
            failed = true;
            // Tranferring is failed
        }
    }
}
