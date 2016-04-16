using System;

using x2;

namespace x2.Examples.HelloWorld
{
    // Multithreaded
    class HelloWorld2
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadFlow("HelloFlow")
                    .Add(new HelloCase()))
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
                        new HelloReq { Name = message }.Post();
                    }
                }
            }
        }
    }
}