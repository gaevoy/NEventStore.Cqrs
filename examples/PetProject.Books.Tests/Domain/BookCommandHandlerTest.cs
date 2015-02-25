using NEventStore.Cqrs.Tests;
using NUnit.Framework;
using PetProject.Books.Domain;
using PetProject.Books.Shared.Commands;
using PetProject.Books.Shared.Events;
using PetProject.Books.Tests.Mocks;
using System;

namespace PetProject.Books.Tests.Domain
{
    public class BookCommandHandlerTest : InfrastructureAwareTest
    {
        public BookCommandHandlerTest() : base(new[] { typeof(BookRegistered).Assembly, typeof(BookCommandHandler).Assembly }) { }

        [Test]
        public void RegisterBook()
        {
            // Given
            RegisterHandlers(() => new BookCommandHandler(Repository, new BookUniquenessCheckerMock()));

            // When
            CommandBus.Publish(new RegisterBook { Id = 1.Guid(), ISBN = "2", Title = "3" });

            // Then
            AssertEvent(new BookRegistered { Id = 1.Guid(), ISBN = "2", Title = "3" });
        }

        [Test]
        public void CorrectBook()
        {
            // Given
            RegisterHandlers(() => new BookCommandHandler(Repository, new BookUniquenessCheckerMock()));
            CommandBus.Publish(new RegisterBook { Id = 1.Guid(), ISBN = "2", Title = "3" });

            // When
            CommandBus.Publish(new CorrectBook { Id = 1.Guid(), Title = "33" });

            // Then
            AssertEvent(new BookCorrected { Id = 1.Guid(), Title = "33" });
        }

        [Test, ExpectedException(typeof(ApplicationException), ExpectedMessage = "Title is already exist")]
        public void CorrectBookByNotUniqueName()
        {
            // Given
            RegisterHandlers(() => new BookCommandHandler(Repository, new BookUniquenessCheckerMock().MockIsTitleExist(true)));
            CommandBus.Publish(new RegisterBook { Id = 1.Guid(), ISBN = "2", Title = "3" });

            // When
            CommandBus.Publish(new CorrectBook { Id = 1.Guid(), Title = "33" });
        }

        [Test]
        public void DeleteBook()
        {
            // Given
            RegisterHandlers(() => new BookCommandHandler(Repository, new BookUniquenessCheckerMock()));
            CommandBus.Publish(new RegisterBook { Id = 1.Guid(), ISBN = "2", Title = "3" });

            // When
            CommandBus.Publish(new DeleteBook { Id = 1.Guid() });

            // Then
            AssertEvent(new BookDeleted { Id = 1.Guid() });
        }
    }
}
