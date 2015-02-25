using CommonDomain.Persistence;
using NEventStore.Cqrs;
using NEventStore.Cqrs.Messages;
using PetProject.Books.Shared.Events;

namespace PetProject.Books.Domain
{
    public class AccountValueTranferringEventHandler : EventHandlerBase,
        IHandler<AccountTransferRequested>,
        IHandler<AccountValueChanged>,
        IHandler<CommandDispatchFailedEvent>
    {
        public AccountValueTranferringEventHandler(ISagaRepository sagas)
            : base(sagas)
        {
        }

        public void Handle(AccountTransferRequested evt)
        {
            var creating = sagas.GetById<AccountValueTranferring>(evt.SagaId.Value);
            creating.Transition(evt);
            Save(creating, @by: evt);
        }

        public void Handle(AccountValueChanged evt)
        {
            if (!evt.SagaId.HasValue) return;

            var creating = sagas.GetById<AccountValueTranferring>(evt.SagaId.Value);
            creating.Transition(evt);
            Save(creating, @by: evt);
        }

        public void Handle(CommandDispatchFailedEvent evt)
        {
            if (AccountValueTranferring.CanHandle(evt))
            {
                var creating = sagas.GetById<AccountValueTranferring>(evt.Id);
                creating.Transition(evt);
                Save(creating, @by: evt);
            }
        }
    }
}
