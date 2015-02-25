using CommonDomain.Core;
using PetProject.Books.Shared.Events;
using System;
using System.Collections.Generic;

namespace PetProject.Books.Domain
{
    class Account : AggregateBase
    {
        bool created;
        string displayName;
        decimal value;
        Dictionary<Guid, decimal> transferredValues = new Dictionary<Guid, decimal>();

        Account(Guid id)
        {
            Id = id;
            Register<AccountCreated>(Apply);
            Register<AccountValueChanged>(Apply);
            Register<AccountTransferRequested>(Apply);
        }

        public Account(Guid id, string displayName, decimal value)
            : this(id)
        {
            displayName = displayName.Trim();
            RaiseEvent(new AccountCreated { Id = id, DisplayName = displayName });
            RaiseEvent(new AccountValueChanged { Id = id, ValueDifference = value });
        }

        void Apply(AccountCreated evt)
        {
            displayName = evt.DisplayName;
            created = true;
        }

        public void TransferTo(Guid transactionId, Guid targetAccount, decimal valueToTransfer)
        {
            EnsureCreated();
            RaiseEvent(new AccountTransferRequested { From = Id, To = targetAccount, Value = valueToTransfer, SagaId = transactionId });
        }

        void Apply(AccountTransferRequested evt)
        {

        }

        public void ChangeValue(Guid transactionId, decimal difference)
        {
            EnsureCreated();
            if (value + difference < 0) throw new ApplicationException(string.Format("{0} account does not have enough value to transfer", displayName));
            RaiseEvent(new AccountValueChanged { Id = Id, ValueDifference = difference, SagaId = transactionId });
        }

        public void CancelChange(Guid transactionId, string reason)
        {
            if (transferredValues.ContainsKey(transactionId))
            {
                EnsureCreated();
                RaiseEvent(new AccountValueChanged { Id = Id, ValueDifference = -transferredValues[transactionId], SagaId = transactionId, Reason = reason });
            }
        }

        void Apply(AccountValueChanged evt)
        {
            value += evt.ValueDifference;

            if (evt.SagaId.HasValue)
            {
                Guid transactionId = evt.SagaId.Value;
                decimal transferredValue;
                transferredValues.TryGetValue(transactionId, out transferredValue);
                transferredValue += evt.ValueDifference;
                transferredValues[transactionId] = transferredValue;
            }
        }

        void EnsureCreated()
        {
            if (created == false) throw new ApplicationException(string.Format("Account {0} is not exist", Id));
        }
    }
}
