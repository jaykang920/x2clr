using System;

using x2;

namespace x2.Examples.HeadFirst
{
    /*
    class HeadFirst0
    {
        public static void Main()
        {
            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }
                else
                {
                    var result = message.ToUpper();
                    Console.WriteLine(result);
                }
            }
        }
    }
    */

    class HeadFirst1
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new CapitalizerCase())
                    .Add(new OutputCase()));

            using (new Hub.Flows().Startup())
            {
                while (true)
                {
                    string message = Console.ReadLine();
                    if (message == "quit")
                    {
                        break;
                    }
                    else
                    {
                        new CapitalizeReq { Message = message }.Post();
                    }
                }
            }
        }
    }
}