using PetProject.Books.Domain;

namespace PetProject.Books.Tests.Mocks
{
    class BookUniquenessCheckerMock : IBookUniquenessChecker
    {
        private bool isTitleExistResult;

        public bool IsISBNExist(string isbn)
        {
            return false;
        }

        public bool IsTitleExist(string title)
        {
            return isTitleExistResult;
        }

        public BookUniquenessCheckerMock MockIsTitleExist(bool result)
        {
            isTitleExistResult = result;
            return this;
        }
    }
}
