using System;

namespace x2.Samples.Capitalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }

                Console.WriteLine(Capitalize(message));
            }
        }

        static string Capitalize(string s)
        {
            return s.ToUpper();
        }
    }
}
