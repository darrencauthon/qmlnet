#include <Hosting/CoreHost.h>
#include <Hosting/coreclrhost.h>
#include <QFileInfo>
#include <QDir>
#include <dlfcn.h>

hostfxr_main_ptr CoreHost::hostfxr_main = nullptr;
CoreHost::LoadHostFxrResult CoreHost::loadResult = CoreHost::Failed;

CoreHost::CoreHost(QList<QByteArray> args) :
    args(args)
{

}

CoreHost::~CoreHost()
{

}

CoreHost::LoadHostFxrResult CoreHost::loadHostFxr()
{
    if(loadResult == CoreHost::LoadHostFxrResult::Loaded) {
        return LoadHostFxrResult::AlreadyLoaded;
    }

    QString hostFxrPath = findHostFxrPath();

    void* dll = dlopen(qPrintable(hostFxrPath), RTLD_NOW | RTLD_LOCAL);
    if(dll == nullptr) {
        qCritical("Couldn't load lib at %s", qPrintable(hostFxrPath));
        return LoadHostFxrResult::Failed;
    }

    hostfxr_main = reinterpret_cast<hostfxr_main_ptr>(dlsym(dll, "hostfxr_main"));

    if(hostfxr_main == nullptr) {
        qCritical("Couldn't load 'hostfxr_main' from %s", qPrintable(hostFxrPath));
        return LoadHostFxrResult::Failed;
    }

    loadResult = LoadHostFxrResult::Loaded;
    return loadResult;
}

bool CoreHost::isHostFxrLoaded()
{
    return loadResult == LoadHostFxrResult::Loaded;
}

int CoreHost::run(QGuiApplication& app, QQmlApplicationEngine& engine, QString dotnetAssembly, QList<QByteArray> args)
{
    loadHostFxr();

    QList<QByteArray> execArgs;
    execArgs.push_back("/usr/local/share/dotnet/dotnet");
    execArgs.push_back("exec");
    execArgs.push_back(dotnetAssembly.toLocal8Bit());

    QString appPtr;
    appPtr.sprintf("%llu", (quintptr)&app);
    QString enginePtr;
    enginePtr.sprintf("%llu", (quintptr)&engine);

    execArgs.push_back(appPtr.toLocal8Bit());
    execArgs.push_back(enginePtr.toLocal8Bit());

    for (QByteArray arg : args) {
        execArgs.push_back(arg);
    }

    std::vector<const char*> hostFxrArgs;
    for (QByteArray arg : execArgs) {
        hostFxrArgs.push_back(arg);
    }
    int size = static_cast<int>(hostFxrArgs.size());

    return hostfxr_main(size, &hostFxrArgs[0]);
}

QString CoreHost::findClrPath()
{
    return "/usr/local/share/dotnet/shared/Microsoft.NETCore.App/2.1.1/libcoreclr.dylib";
}

QString CoreHost::findHostFxrPath()
{
    return "/usr/local/share/dotnet/host/fxr/2.1.3/libhostfxr.dylib";
}

QString CoreHost::findDotNetRoot()
{
    return "/usr/local/share/dotnet";
}
