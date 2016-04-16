using System;

using x2;

namespace x2.Examples.HelloWorld
{
    /*
    class HelloWorld0
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
                    var result = String.Format("Hello, {0}!", message);
                    Console.WriteLine(result);
                }
            }
        }
    }
    */

    class HelloWorld1
    {
        public static void Main()
        {
            Hub.Instance
                .Attach(new SingleThreadFlow()
                    .Add(new HelloCase())
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