using System;
using System.IO;
using Nancy;

namespace PetProject.Books.Host.Impl
{
#if DEBUG
    public class NancyDebugRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName;
        }
    }
#endif
}
