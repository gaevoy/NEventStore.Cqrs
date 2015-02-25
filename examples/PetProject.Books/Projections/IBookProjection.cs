using System;
using NEventStore.Cqrs;

namespace PetProject.Books.Projections
{
    public interface IBookProjection : IProjection
    {
        BookDto Load(Guid id);
        BookDto[] ListAll();
    }
}
