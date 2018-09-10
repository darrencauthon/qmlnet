#ifndef COREHOST_H
#define COREHOST_H

#include <QSharedPointer>
#include <Hosting/coreclrhost.h>
#include <QGuiApplication>
#include <QQmlApplicationEngine>

class CoreHost : public QObject
{
    Q_OBJECT
public:
    enum LoadHostFxrResult
    {
        Loaded,
        AlreadyLoaded,
        Failed
    };

    CoreHost(QList<QByteArray> args);
    ~CoreHost();

    static LoadHostFxrResult loadHostFxr();
    static bool isHostFxrLoaded();

    static int run(QGuiApplication& app, QQmlApplicationEngine& engine, QString dotnetAssembly, QList<QByteArray> args = QList<QByteArray>());

    static QString findClrPath();
    static QString findHostFxrPath();
    static QString findDotNetRoot();
    static QSharedPointer<CoreHost> buildHost(QList<QByteArray> args);
private:
    // Function pointers to the exported C functions.
    static hostfxr_main_ptr hostfxr_main;
    static LoadHostFxrResult loadResult;
    QList<QByteArray> args;
signals:
};

#endif
