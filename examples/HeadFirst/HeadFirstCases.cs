using System;

using x2;

namespace x2.Samples.HeadFirst
{
    public class CapitalizerCase : Case
    {
        protected override void SetUp()
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
        protected override void SetUp()
        {
            new CapitalizeResp().Bind(e => Console.WriteLine(e.Result));
        }
    }

}
