using CommonDomain.Persistence;
using NEventStore.Cqrs;
using PetProject.Books.Shared.Commands;

namespace PetProject.Books.Domain
{
    public class BookCommandHandler : CommandHandlerBase, 
        IHandler<RegisterBook>,
        IHandler<CorrectBook>,
        IHandler<DeleteBook>
    {
        private readonly IBookUniquenessChecker uniquenessChecker;

        public BookCommandHandler(IRepository repo, IBookUniquenessChecker uniquenessChecker)
            : base(repo)
        {
            this.uniquenessChecker = uniquenessChecker;
        }

        public void Handle(RegisterBook cmd)
        {
            var book = new Book(cmd.Id, cmd.Title, cmd.ISBN, uniquenessChecker);
            Save(book, by: cmd);
        }

        public void Handle(CorrectBook cmd)
        {
            var book = repo.GetById<Book>(cmd.Id);
            book.Correct(cmd.Title, uniquenessChecker);
            Save(book, by: cmd);
        }

        public void Handle(DeleteBook cmd)
        {
            var book = repo.GetById<Book>(cmd.Id);
            book.Delete();
            Save(book, by: cmd);
        }
    }
}
