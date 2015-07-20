using System;
using EventStream.Projector;

namespace PetProject.Books.Projections
{
    public interface IBookProjection : IProjection
    {
        BookDto Load(Guid id);
        BookDto[] ListAll();
    }
}
