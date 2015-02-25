using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NEventStore.Cqrs;
using NEventStore.Cqrs.Projections;
using PetProject.Books.Domain;
using PetProject.Books.Shared.Events;
using System;
using System.Linq;

namespace PetProject.Books.Projections.Impl
{
    class BookProjection : IBookProjection, IBookUniquenessChecker, ITrackStructureChanges,
        IHandler<BookRegistered>,
        IHandler<BookCorrected>,
        IHandler<BookDeleted>
    {
        private readonly MongoCollection<BookDto> books;

        public BookProjection(string connectionString)
        {
            var db = new MongoClient(connectionString).GetServer().GetDatabase(new MongoUrl(connectionString).DatabaseName);
            books = db.GetCollection<BookDto>("Books");
        }

        public int Version { get { return 0; } }
        public Type[] TrackTypes { get { return new[] { typeof(BookDto) }; } }

        public void Clear()
        {
            books.Drop();
        }

        public void Handle(BookRegistered evt)
        {
            books.Insert(new BookDto { Id = evt.Id, Title = evt.Title, ISBN = evt.ISBN });
        }

        public void Handle(BookCorrected evt)
        {
            books.Update(Query<BookDto>.EQ(c => c.Id, evt.Id), Update<BookDto>.Set(c => c.Title, evt.Title));
        }
        public void Handle(BookDeleted evt)
        {
            books.Update(Query<BookDto>.EQ(c => c.Id, evt.Id), Update<BookDto>.Set(c => c.Deleted, true));
        }

        public BookDto Load(Guid id)
        {
            return books.FindOne(Query<BookDto>.EQ(c => c.Id, id));
        }

        public BookDto[] ListAll()
        {
            return books.AsQueryable().Where(e => e.Deleted == false).OrderBy(e => e.Title).ToArray();
        }

        public bool IsISBNExist(string isbn)
        {
            return books.AsQueryable().Any(e => e.ISBN == isbn);
        }

        public bool IsTitleExist(string title)
        {
            return books.AsQueryable().Any(e => e.Title == title);
        }
    }
}