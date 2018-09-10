using System;
using System.Linq;

namespace Qml.Net
{
    public static class Host
    {
        public static int Run(string[] args, Func<string[], QGuiApplication, QQmlApplicationEngine, int> action)
        {
            if (args.Length < 2)
            {
                throw new Exception("Args is invalid, must contain two entries which are pointers to native types.");
            }
            
            var appPtr = new IntPtr((long)ulong.Parse(args[0]));
            var enginePtr = new IntPtr((long)ulong.Parse(args[1]));

            using (var app = new QGuiApplication(appPtr))
            {
                using (var engine = new QQmlApplicationEngine(enginePtr))
                {
                    return action(args.Skip(2).ToArray(), app, engine);
                }
            }
        }
    }
}