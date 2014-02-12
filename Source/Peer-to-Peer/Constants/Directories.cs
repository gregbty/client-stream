using System.IO;

namespace ClientStream.Constants
{
    internal static class Directories
    {
        public static string Files
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "files"); }
        }

        public static string Downloads
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "downloads"); }
        }
    }
}