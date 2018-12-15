using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Qml.Net.Internal.Platform.PathResolver
{
    internal class DynamicLinkLibraryPathResolver : IPathResolver
    {
        private static readonly IPathResolver LocalPathResolver;

        private static readonly IPathResolver PathResolver;

        private bool SearchLocalFirst { get; }

        static DynamicLinkLibraryPathResolver()
        {
            LocalPathResolver = new LocalPathResolver();
            PathResolver = SelectPathResolver();
        }

        public DynamicLinkLibraryPathResolver(bool searchLocalFirst = true)
        {
            SearchLocalFirst = searchLocalFirst;
        }

        private static IPathResolver SelectPathResolver()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPathResolver();
            }

            /*
                Temporary hack until BSD is added to RuntimeInformation. OSDescription should contain the output from
                "uname -srv", which will report something along the lines of FreeBSD or OpenBSD plus some more info.
            */
            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD)
            {
                return new LinuxPathResolver();
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOSPathResolver();
            }

            throw new PlatformNotSupportedException($"Cannot resolve linker paths on this platform: {RuntimeInformation.OSDescription}");
        }

        public ResolvePathResult Resolve(string library)
        {
            return ResolveAbsolutePath(library, SearchLocalFirst);
        }

        private ResolvePathResult ResolveAbsolutePath(string library, bool localFirst)
        {
            var candidates = GenerateLibraryCandidates(library).ToList();

            if (library.IsValidPath())
            {
                foreach (var candidate in candidates)
                {
                    if (File.Exists(candidate))
                    {
                        return ResolvePathResult.FromSuccess(Path.GetFullPath(candidate));
                    }
                }
            }

            // Check the native probing paths (.NET Core defines this, Mono doesn't. Users can set this at runtime, too)
            if (AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") is string directories)
            {
                var paths = directories.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var path in paths)
                {
                    foreach (var candidate in candidates)
                    {
                        var candidatePath = Path.Combine(path, candidate);
                        if (File.Exists(candidatePath))
                        {
                            return ResolvePathResult.FromSuccess(Path.GetFullPath(candidatePath));
                        }
                    }
                }
            }

            if (localFirst)
            {
                foreach (var candidate in candidates)
                {
                    var result = LocalPathResolver.Resolve(candidate);
                    if (result.IsSuccess)
                    {
                        return result;
                    }
                }
            }

            foreach (var candidate in candidates)
            {
                var result = PathResolver.Resolve(candidate);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            if (library == "__Internal")
            {
                // Mono extension: Search the main program. Allowed for all runtimes
                return ResolvePathResult.FromSuccess(null);
            }

            return ResolvePathResult.FromError(new FileNotFoundException("The specified library was not found in any of the loader search paths.", library));
        }

        private static IEnumerable<string> GenerateLibraryCandidates(string library)
        {
            bool doesLibraryContainPath = false;
            var parentDirectory = Path.GetDirectoryName(library) ?? string.Empty;
            if (library.IsValidPath())
            {
                library = Path.GetFileName(library);
                doesLibraryContainPath = true;
            }

            var candidates = new List<string>
            {
                library
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !library.EndsWith(".dll"))
            {
                candidates.AddRange(GenerateWindowsCandidates(library));
            }

            bool isBSD = RuntimeInformation.OSDescription.ToUpperInvariant().Contains("BSD");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || isBSD || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                candidates.AddRange(GenerateUnixCandidates(library));
            }

            // If we have a parent path we're looking at, mutate the candidate list to include the parent path
            if (doesLibraryContainPath)
            {
                candidates = candidates.Select(c => Path.Combine(parentDirectory, c)).ToList();
            }

            return candidates;
        }

        private static IEnumerable<string> GenerateWindowsCandidates(string library)
        {
            yield return $"{library}.dll";
        }

        private static IEnumerable<string> GenerateUnixCandidates(string library)
        {
            const string prefix = "lib";
            var suffix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";

            var noSuffix = !library.EndsWith(suffix);
            var noPrefix = !Path.GetFileName(library).StartsWith(prefix);
            if (noSuffix)
            {
                yield return $"{library}{suffix}";
            }

            if (noPrefix)
            {
                yield return $"{prefix}{library}";
            }

            if (noPrefix && noSuffix)
            {
                yield return $"{prefix}{library}{suffix}";
            }
        }
    }
}