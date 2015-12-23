using System;

namespace ChromeListener
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-Q to exit");
            var listener = new WebListener();
            listener.Start();
            var key = Console.ReadKey();
            if(key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                listener.Stop();
        }
    }
}
