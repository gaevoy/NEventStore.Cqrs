using System;

namespace PetProject.Books.Projections
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public bool Deleted { get; set; }
    }
}
