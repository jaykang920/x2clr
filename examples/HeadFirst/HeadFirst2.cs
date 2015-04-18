using System;

using x2;
using x2.Flows;

namespace x2.Examples.HeadFirst
{
    class HeadFirst2
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadedFlow("CapitalizerFlow")
                    .Add(new CapitalizerCase()))
                .Attach(new SingleThreadedFlow("OutputFlow")
                    .Add(new OutputCase()));

            using (var flows = new Hub.Flows())
            {
                flows.StartUp();

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