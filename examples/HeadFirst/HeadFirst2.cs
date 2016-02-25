using System;

using x2;

namespace x2.Examples.HeadFirst
{
    // Multithreaded
    class HeadFirst2
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadFlow("CapitalizerFlow")
                    .Add(new CapitalizerCase()))
                .Attach(new SingleThreadFlow("OutputFlow")
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