using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AdvancedDLSupport;
using Qml.Net.Internal.Platform.Loader;
using Qml.Net.Internal.Platform.PathResolver;
using Qml.Net.Internal.Qml;
using Qml.Net.Internal.Types;

[assembly: InternalsVisibleTo("DLSupportDynamicAssembly")]

namespace Qml.Net.Internal
{
    internal static class Interop
    {
        static readonly CallbacksImpl DefaultCallbacks = new CallbacksImpl(new DefaultCallbacks());
        
        static Interop()
        {
            Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/home/pknopf/git/x3/abra/app/src/net/submodules/qmlnet/src/native/build-QmlNet-Desktop_Qt_5_12_0_GCC_64bit2-Debug");
            //Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", "/home/pknopf/git/x3/abra/app/src/net/submodules/qmlnet/src/native/build-QmlNet-Desktop_Qt_5_10_1_GCC_64bit-Debug");
            string pluginsDirectory = null;
            string qmlDirectory = null;
            string libDirectory = null;

            ILibraryPathResolver pathResolver = null;
            
            if (Host.GetExportedSymbol != null)
            {
                // We are loading exported functions from the currently running executable.
                var member = (FieldInfo)typeof(NativeLibraryBase).GetMember("PlatformLoader", BindingFlags.Static | BindingFlags.NonPublic).First();
                member.SetValue(null, new Host.Loader());
                pathResolver = new Host.Loader();
            }
            else
            {
                var internalType = Type.GetType("AdvancedDLSupport.DynamicLinkLibraryPathResolver, AdvancedDLSupport");
                if (internalType != null)
                {
                    pathResolver = (ILibraryPathResolver) Activator.CreateInstance(internalType, new object[] {true});

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // This custom path resolver attempts to do a DllImport to get the path that .NET decides.
                        // It may load a special dll from a NuGet package.
                        pathResolver = new WindowsDllImportLibraryPathResolver(pathResolver);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        pathResolver = new MacDllImportLibraryPathResolver(pathResolver);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        pathResolver = new LinuxDllImportLibraryPathResolver(pathResolver);
                    }

                    var resolveResult = pathResolver.Resolve("QmlNet");

                    if (resolveResult.IsSuccess)
                    {
                        libDirectory = Path.GetDirectoryName(resolveResult.Path);
                        if (!string.IsNullOrEmpty(libDirectory))
                        {
                            // If this library has a plugins/qml directory below it, set it.
                            var potentialPlugisDirectory = Path.Combine(libDirectory, "plugins");
                            if (Directory.Exists(potentialPlugisDirectory))
                            {
                                pluginsDirectory = potentialPlugisDirectory;
                            }

                            var potentialQmlDirectory = Path.Combine(libDirectory, "qml");
                            if (Directory.Exists(potentialQmlDirectory))
                            {
                                qmlDirectory = potentialQmlDirectory;
                            }
                        }
                    }
                }
            }


            var builder = new NativeLibraryBuilder(pathResolver: pathResolver);
            
            var interop = builder.ActivateInterface<ICombined>("QmlNet");

            Callbacks = interop;
            NetTypeInfo = interop;
            NetMethodInfo = interop;
            NetPropertyInfo = interop;
            NetTypeManager = interop;
            QGuiApplication = interop;
            QQmlApplicationEngine = interop;
            NetVariant = interop;
            NetReference = interop;
            NetVariantList = interop;
            NetTestHelper = interop;
            NetSignalInfo = interop;
            QResource = interop;
            NetDelegate = interop;
            NetJsValue = interop;
            QQuickStyle = interop;
            QtInterop = interop;
            Utilities = interop;
            QtWebEngine = interop;
            
            if(!string.IsNullOrEmpty(pluginsDirectory))
            {
                Qt.PutEnv("QT_PLUGIN_PATH", pluginsDirectory);
            }
            if(!string.IsNullOrEmpty(qmlDirectory))
            {
                Qt.PutEnv("QML2_IMPORT_PATH", qmlDirectory);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!string.IsNullOrEmpty(libDirectory) && Directory.Exists(libDirectory))
                {
                    // Even though we opened up the native dll correctly, we need to add
                    // the folder to the path. The reason is because QML plugins aren't
                    // in the same directory and have trouble finding dependencies
                    // that are within our lib folder.
                    Environment.SetEnvironmentVariable("PATH",
                        Environment.GetEnvironmentVariable("PATH") + $";{libDirectory}");
                }
            }

            var cb = DefaultCallbacks.Callbacks();
            Callbacks.RegisterCallbacks(ref cb);


            {
                var resolver = new DynamicLinkLibraryPathResolver();
                var result = resolver.Resolve("QmlNet");
                var loader = PlatformLoaderBase.SelectPlatformLoader();
                var lib = loader.LoadLibrary(result.Path);
                var s = loader.LoadSymbol(lib, "net_js_value_isCallable");
                IsCallable = Marshal.GetDelegateForFunctionPointer<IsCallableDel>(s);
            }
        }

        // ReSharper disable PossibleInterfaceMemberAmbiguity
        // ReSharper disable MemberCanBePrivate.Global
        internal interface ICombined :
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore PossibleInterfaceMemberAmbiguity
            ICallbacksIterop,
            INetTypeInfoInterop,
            INetMethodInfoInterop,
            INetPropertyInfoInterop,
            INetTypeManagerInterop,
            IQGuiApplicationInterop,
            IQQmlApplicationEngine,
            INetVariantInterop,
            INetReferenceInterop,
            INetVariantListInterop,
            INetTestHelperInterop,
            INetSignalInfoInterop,
            IQResourceInterop,
            INetDelegateInterop,
            INetJsValueInterop,
            IQQuickStyleInterop,
            IQtInterop,
            IUtilities,
            IQtWebEngine
        {

        }

        public delegate bool IsCallableDel(IntPtr test);
        
        public static IsCallableDel IsCallable { get; set; }
        
        public static ICallbacksIterop Callbacks { get; }

        public static INetTypeInfoInterop NetTypeInfo { get; }
        
        public static INetMethodInfoInterop NetMethodInfo { get; }
        
        public static INetPropertyInfoInterop NetPropertyInfo { get; }
        
        public static INetTypeManagerInterop NetTypeManager { get; }
        
        public static IQGuiApplicationInterop QGuiApplication { get; }
        
        public static IQQmlApplicationEngine QQmlApplicationEngine { get; }
        
        public static INetVariantInterop NetVariant { get; }
        
        public static INetReferenceInterop NetReference { get; }
        
        public static INetVariantListInterop NetVariantList { get; }
        
        public static INetTestHelperInterop NetTestHelper { get; }
        
        public static INetSignalInfoInterop NetSignalInfo { get; }
        
        public static IQResourceInterop QResource { get; }
        
        public static INetDelegateInterop NetDelegate { get; }
        
        public static INetJsValueInterop NetJsValue { get; }
        
        public static IQQuickStyleInterop QQuickStyle { get; }

        public static IQtInterop QtInterop { get; }
        
        public static IUtilities Utilities { get; }
        
        public static IQtWebEngine QtWebEngine { get; }
    }
}