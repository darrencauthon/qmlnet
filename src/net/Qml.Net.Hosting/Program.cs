using System;

namespace Qml.Net.Hosting
{
    class Program
    {
        static int Main(string[] _)
        {
            return Host.Run(_, (args, app, engine) =>
            {
                Console.WriteLine("Running");
                Console.WriteLine("Args " + args.Length);
                foreach(var arg in args)
                {
                    Console.WriteLine(arg);
                }
                int result = app.Exec();
                Console.WriteLine("Ran: " + result);
                return result;
            });
        }
    }
}
