using CommonDomain.Persistence;
using NEventStore.Cqrs;
using PetProject.Books.Shared.Commands;

namespace PetProject.Books.Domain
{
    public class AccountCommandHandler : CommandHandlerBase,
        IHandler<CreateAccount>,
        IHandler<TransferAccountValue>,
        IHandler<ChangeAccountValue>,
        IHandler<CancelAccountChange>
    {
        public AccountCommandHandler(IRepository repo) : base(repo) { }

        public void Handle(CreateAccount cmd)
        {
            var account = new Account(cmd.Id, cmd.DisplayName, cmd.Value);
            Save(account, by: cmd);
        }

        public void Handle(TransferAccountValue cmd)
        {
            var account = repo.GetById<Account>(cmd.From);
            account.TransferTo(cmd.SagaId.Value, cmd.To, cmd.Value);
            Save(account, by: cmd);
        }

        public void Handle(ChangeAccountValue cmd)
        {
            var account = repo.GetById<Account>(cmd.Id);
            account.ChangeValue(cmd.SagaId.Value, cmd.Value);
            Save(account, by: cmd);
        }

        public void Handle(CancelAccountChange cmd)
        {
            var account = repo.GetById<Account>(cmd.Id);
            account.CancelChange(cmd.SagaId.Value, cmd.Reason);
            Save(account, by: cmd);
        }
    }
}
