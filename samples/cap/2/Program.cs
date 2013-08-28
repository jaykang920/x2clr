using System;
using System.Text;
using System.Threading;

using x2;
using x2.Flows;

namespace x2.Samples.Capitalizer
{
    class CapitalizerFlow : SingleThreadedFlow
    {
        static void OnCapitalizeReq(CapitalizeReq req)
        {
            var resp = CapitalizeResp.New();
            resp.Result = req.Message.ToUpper();
            Flow.PostAway(resp);
        }

        protected override void SetUp()
        {
            Subscribe(CapitalizeReq.New(), OnCapitalizeReq);
        }
    }

    class OutputFlow : SingleThreadedFlow
    {
        static void OnCapitalizeResp(CapitalizeResp e)
        {
            Console.WriteLine(e.Result);
        }

        protected override void SetUp()
        {
            Subscribe(CapitalizeResp.New(), OnCapitalizeResp);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Hub.Get()
                .Attach(new CapitalizerFlow())
                .Attach(new OutputFlow());

            Flow.StartAll();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "quit")
                {
                    break;
                }

                var e = CapitalizeReq.New();
                e.Message = message;
                Hub.Get().Post(e);
            }

            Flow.StopAll();
        }
    }
}
