using System;

namespace x2.Samples.Capitalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string line = Console.ReadLine();
                if (line == "exit" || line == "quit")
                {
                    break;
                }

                Console.WriteLine(Capitalize(line));
            }
        }

        static string Capitalize(string s)
        {
            return s.ToUpper();
        }
    }
}
