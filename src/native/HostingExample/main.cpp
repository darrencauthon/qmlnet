#include <QGuiApplication>
#include <QQmlApplicationEngine>
#include <Hosting/CoreHost.h>
#include <Hosting/coreruncommon.h>

int main(int argc, char *argv[])
{
    QCoreApplication::setAttribute(Qt::AA_EnableHighDpiScaling);
    QGuiApplication app(argc, argv);

    QQmlApplicationEngine engine;
    engine.load(QUrl(QStringLiteral("qrc:/main.qml")));
    if (engine.rootObjects().isEmpty())
        return -1;

    QList<QByteArray> args;
    args.push_back("test");
    args.push_back("test2");
    return CoreHost::run(app, engine, "/Users/pknopf/git/net-core-qml/src/net/Qml.Net.Hosting/bin/Debug/netcoreapp2.1/Qml.Net.Hosting.dll", args);
}
