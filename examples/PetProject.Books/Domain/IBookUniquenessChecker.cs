namespace PetProject.Books.Domain
{
    public interface IBookUniquenessChecker
    {
        bool IsISBNExist(string isbn);
        bool IsTitleExist(string title);
    }
}
