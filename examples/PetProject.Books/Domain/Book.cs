using CommonDomain.Core;
using PetProject.Books.Shared.Events;
using System;

namespace PetProject.Books.Domain
{
    class Book : AggregateBase
    {
        string title;
        bool deleted;

        Book(Guid id)
        {
            Id = id;
            Register<BookRegistered>(Apply);
            Register<BookCorrected>(Apply);
            Register<BookDeleted>(Apply);
        }

        public Book(Guid id, string title, string isbn, IBookUniquenessChecker uniquenessChecker)
            : this(id)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ApplicationException("Title is required");
            if (string.IsNullOrWhiteSpace(isbn)) throw new ApplicationException("ISBN is required");
            if (uniquenessChecker.IsTitleExist(title)) throw new ApplicationException("Title is already exist");
            if (uniquenessChecker.IsISBNExist(isbn)) throw new ApplicationException("ISBN is already exist");

            title = title.Trim();
            isbn = isbn.Trim();
            RaiseEvent(new BookRegistered { Id = id, Title = title, ISBN = isbn });
        }

        void Apply(BookRegistered evt)
        {
            title = evt.Title;
        }

        public void Correct(string title, IBookUniquenessChecker uniquenessChecker)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ApplicationException("Title is required");
            if (deleted) throw new ApplicationException("This book has already deleted");

            title = title.Trim();
            if (this.title != title)
            {
                if (uniquenessChecker.IsTitleExist(title)) throw new ApplicationException("Title is already exist");
                RaiseEvent(new BookCorrected { Id = Id, Title = title });
            }
        }

        void Apply(BookCorrected evt)
        {
            title = evt.Title;
        }

        public void Delete()
        {
            if (!deleted)
                RaiseEvent(new BookDeleted { Id = Id });
        }

        void Apply(BookDeleted evt)
        {
            deleted = true;
        }
    }
}
