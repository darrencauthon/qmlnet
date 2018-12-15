using System.IO;
using System.Linq;

namespace Qml.Net.Internal.Platform.PathResolver
{
    internal static class Extensions
    {
        public static bool IsValidPath(this string @this)
        {
            if (string.IsNullOrEmpty(@this))
            {
                return false;
            }

            if (@this.Any(c => Path.GetInvalidPathChars().Contains(c)))
            {
                return false;
            }

            var parentDirectory = Path.GetDirectoryName(@this);
            if (string.IsNullOrEmpty(parentDirectory))
            {
                return false;
            }

            return true;
        }
    }
}