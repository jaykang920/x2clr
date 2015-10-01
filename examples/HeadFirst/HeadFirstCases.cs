using System;

using x2;

namespace x2.Examples.HeadFirst
{
    public class CapitalizerCase : Case
    {
        protected override void Setup()
        {
            new CapitalizeReq().Bind(req => {
                new CapitalizeResp {
                    Result = req.Message.ToUpper()
                }.InResponseOf(req).Post();
            });
        }
    }

    public class OutputCase : Case
    {
        protected override void Setup()
        {
            new CapitalizeResp().Bind(e => Console.WriteLine(e.Result));
        }
    }

}
