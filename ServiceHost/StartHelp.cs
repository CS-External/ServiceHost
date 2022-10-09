using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHost
{
    public static class StartHelp
    {
        public static async Task Run()
        {
            Console.Write("  _________                  .__               ___ ___                 __   \r\n /   _____/ ______________  _|__| ____  ____  /   |   \\  ____  _______/  |_ \r\n \\_____  \\_/ __ \\_  __ \\  \\/ /  |/ ___\\/ __ \\/    ~    \\/  _ \\/  ___/\\   __\\\r\n /        \\  ___/|  | \\/\\   /|  \\  \\__\\  ___/\\    Y    (  <_> )___ \\  |  |  \r\n/_______  /\\___  >__|    \\_/ |__|\\___  >___  >\\___|_  / \\____/____  > |__|  \r\n        \\/     \\/                    \\/    \\/       \\/            \\/        ");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Commands");
            Console.WriteLine("");
            Console.WriteLine("'install {ServiceName}' - Install the current Instance as Service");
            Console.WriteLine("'uninstall {ServiceName}' - Uninstall the current Instance as Service");
            Console.WriteLine("");

            Console.WriteLine("");
        }
    }
}
