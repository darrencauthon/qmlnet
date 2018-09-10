#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <Hosting/CoreHost.h>
#include <Hosting/coreruncommon.h>

static int runCallback(QGuiApplication* app, QQmlApplicationEngine* engine)
{
    engine->load(QUrl(QStringLiteral("qrc:/main.qml")));
    if (engine->rootObjects().isEmpty())
        return -1;

    return app->exec();
}

int main(int argc, char *argv[])
{
    QCoreApplication::setAttribute(Qt::AA_EnableHighDpiScaling);
    QGuiApplication app(argc, argv);

    QQmlApplicationEngine engine;

    return CoreHost::run(app,
        engine,
        runCallback,
        "/Users/pknopf/git/net-core-qml/src/net/Qml.Net.Hosting/bin/Debug/netcoreapp2.1/Qml.Net.Hosting.dll");
}
