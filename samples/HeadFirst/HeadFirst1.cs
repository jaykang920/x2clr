using System;

using x2;
using x2.Flows;

namespace x2.Samples.HeadFirst
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
        class CapitalizerCase : Case
        {
            protected override void SetUp()
            {
                new CapitalizeReq().Bind((req) => {
                    new CapitalizeResp {
                        Result = req.Message.ToUpper()
                    }.InResponseOf(req).Post();
                });
            }
        }

        class OutputCase : Case
        {
            protected override void SetUp()
            {
                new CapitalizeResp().Bind((e) => { Console.WriteLine(e.Result); });
            }
        }

        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadedFlow()
                    .Add(new CapitalizerCase())
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