INCLUDEPATH += $$PWD

HEADERS += $$PWD/Hosting/coreclrhost.h \
    $$PWD/Hosting/CoreHost.h

SOURCES += \
    $$PWD/Hosting/CoreHost.cpp

unix {
    LIBS += -ldl
} else {
    # Windows needs this to be able to perform GetProcAddress
    # on the currently running executable.
    QMAKE_LFLAGS += /FIXED:NO
}
