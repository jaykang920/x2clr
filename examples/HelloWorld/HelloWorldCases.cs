using System;

using x2;

namespace x2.Examples.HelloWorld
{
    public class HelloCase : Case
    {
        protected override void Setup()
        {
            new HelloReq().Bind(req => {
                new HelloResp {
                    Result = String.Format("Hello, {0}!", req.Name)
                }.InResponseOf(req).Post();
            });
        }
    }

    public class OutputCase : Case
    {
        protected override void Setup()
        {
            new HelloResp().Bind(e => Console.WriteLine(e.Result));
        }
    }
}
