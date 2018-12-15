using System;
using System.IO;
using System.Linq;

namespace Qml.Net.Internal.Platform.PathResolver
{
    internal class LinuxPathResolver : IPathResolver
    {
        public ResolvePathResult Resolve(string library)
        {
            var libraryPaths = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")?.Split(':').Where(p => !string.IsNullOrEmpty(p));

            string libraryLocation;

            if (!(libraryPaths is null))
            {
                foreach (var path in libraryPaths)
                {
                    libraryLocation = Path.GetFullPath(Path.Combine(path, library));
                    if (File.Exists(libraryLocation))
                    {
                        return ResolvePathResult.FromSuccess(libraryLocation);
                    }
                }
            }

            if (File.Exists("/etc/ld.so.cache"))
            {
                var cachedLibraries = File.ReadAllText("/etc/ld.so.cache").Split('\0');
                var cachedMatch = cachedLibraries.FirstOrDefault
                (
                    l =>
                        l.EndsWith(library) &&
                        Path.GetFileName(l) == Path.GetFileName(library)
                );

                if (!(cachedMatch is null))
                {
                    return ResolvePathResult.FromSuccess(cachedMatch);
                }
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/lib", library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            libraryLocation = Path.GetFullPath(Path.Combine("/usr/lib", library));
            if (File.Exists(libraryLocation))
            {
                return ResolvePathResult.FromSuccess(libraryLocation);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }
    }
}