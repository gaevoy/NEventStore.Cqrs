using NEventStore.Cqrs.Tests;
using NUnit.Framework;
using PetProject.Books.Domain;
using PetProject.Books.Shared.Commands;
using PetProject.Books.Shared.Events;

namespace PetProject.Books.Tests.Domain
{
    public class AccountValueTranferringTest : InfrastructureAwareTest
    {
        public AccountValueTranferringTest() : base(new[] { typeof(BookRegistered).Assembly, typeof(BookCommandHandler).Assembly }) { }

        [Test]
        public void TransferMoney()
        {
            // Given
            RegisterHandlers(() => new AccountCommandHandler(Repository));
            RegisterHandlers(() => new AccountValueTranferringEventHandler(SagaRepository));
            CommandBus.Publish(new CreateAccount { Id = 1.Guid(), DisplayName = "Company", Value = 1000m });
            CommandBus.Publish(new CreateAccount { Id = 2.Guid(), DisplayName = "My", Value = 100m });
            ClearEvents();

            // When
            CommandBus.Publish(new TransferAccountValue { From = 1.Guid(), To = 2.Guid(), Value = 40m, SagaId = 123.Guid() });

            // Then
            AssertEvent(new AccountValueChanged { Id = 1.Guid(), ValueDifference = -40m, SagaId = 123.Guid() }, evt => evt.Id == 1.Guid());
            AssertEvent(new AccountValueChanged { Id = 2.Guid(), ValueDifference = +40m, SagaId = 123.Guid() }, evt => evt.Id == 2.Guid());
        }

        [Test]
        public void NotTransferMoneyIfLackOfCompanyMoney()
        {
            // Given
            RegisterHandlers(() => new AccountCommandHandler(Repository));
            RegisterHandlers(() => new AccountValueTranferringEventHandler(SagaRepository));
            CommandBus.Publish(new CreateAccount { Id = 1.Guid(), DisplayName = "Company", Value = 1000m });
            CommandBus.Publish(new CreateAccount { Id = 2.Guid(), DisplayName = "My", Value = 100m });
            ClearEvents();

            // When
            CommandBus.Publish(new TransferAccountValue { From = 1.Guid(), To = 2.Guid(), Value = 2000m, SagaId = 123.Guid() });

            // Then
            AssertEventIsNotRaised<AccountValueChanged>();
        }      
        
        [Test]
        public void NotTransferMoneyIfLackOfMyMoney()
        {
            // Given
            RegisterHandlers(() => new AccountCommandHandler(Repository));
            RegisterHandlers(() => new AccountValueTranferringEventHandler(SagaRepository));
            CommandBus.Publish(new CreateAccount { Id = 1.Guid(), DisplayName = "Company", Value = 1000m });
            CommandBus.Publish(new CreateAccount { Id = 2.Guid(), DisplayName = "My", Value = 100m });
            ClearEvents();

            // When
            CommandBus.Publish(new TransferAccountValue { From = 1.Guid(), To = 2.Guid(), Value = -200m, SagaId = 123.Guid() });

            // Then
            AssertEvent(new AccountValueChanged { Id = 1.Guid(), ValueDifference = +200m, SagaId = 123.Guid() }, evt => evt.Id == 1.Guid() && evt.Reason == null);
            AssertEvent(new AccountValueChanged { Id = 1.Guid(), ValueDifference = -200m, SagaId = 123.Guid(), Reason = "My account does not have enough value to transfer" }, evt => evt.Id == 1.Guid() && evt.Reason != null);
        }

        [Test]
        public void NotTransferMoneyIfNotExistedAccount()
        {
            // Given
            RegisterHandlers(() => new AccountCommandHandler(Repository));
            RegisterHandlers(() => new AccountValueTranferringEventHandler(SagaRepository));
            CommandBus.Publish(new CreateAccount { Id = 1.Guid(), DisplayName = "Company", Value = 1000m });
            ClearEvents();

            // When
            CommandBus.Publish(new TransferAccountValue { From = 1.Guid(), To = 2.Guid(), Value = 40m, SagaId = 123.Guid() });

            // Then
            AssertEvent(new AccountValueChanged { Id = 1.Guid(), ValueDifference = -40m, SagaId = 123.Guid() }, evt => evt.Id == 1.Guid() && evt.Reason == null);
            AssertEvent(new AccountValueChanged { Id = 1.Guid(), ValueDifference = +40m, SagaId = 123.Guid(), Reason = "Account 00000000-0000-0000-0000-000000000002 is not exist" }, evt => evt.Id == 1.Guid() && evt.Reason != null);
        }
    }
}
