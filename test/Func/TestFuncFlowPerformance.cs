using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using x2;

namespace x2.Tests.Func
{
    /// <summary>
    /// x2 model is to use Flow broadcasting of events. 
    /// There is Evnet._Channel and Flow subscription to Channel to filter. 
    /// But a recommended model is to use broadcast. 
    /// Therefore performance measure is important to feel safe and comfortable to use it.
    /// </summary>
    [TestFixture]
    public class TestFuncFlowPerformance
    {

        [Test]
        public void TestBroadcastPerf()
        {
            var pc = new PerfCase();

            Hub.Instance
                .Attach(new SingleThreadFlow().Add(pc));

            for (int i = 0; i < 3; ++i)
            { 
                Hub.Instance
                .Attach(new SingleThreadFlow())
                .Attach(new SingleThreadFlow())
                .Attach(new SingleThreadFlow());
            }

            Hub.Startup();

            const int TestCount = 1000*1000;

            Thread.Sleep(100);

            pc.Hello();

            while( pc.RunCount < TestCount)
            {
                Thread.Sleep(10);
            }

            Hub.Shutdown();

            // Case 1. new HelloCaseEvent { Bar = "A" }.Bind();  
            // 1 million / 8 seconds

            // Case 2. new HelloCase().Bind . 
            // 1 million / 8 seconds.

            // Case 3. one SingleThreafFlow  
            // 1 million / 1 second

            // Case 4. 4 SingleThreafFlows
            // 1 million / 4 ~ 5 seconds
        }

        [Test]
        public void TestChannelFilterPerf()
        {
            var pc = new PerfCase("f1");

            var f1 = new SingleThreadFlow();
            var f2 = new SingleThreadFlow();
            var f3 = new SingleThreadFlow();
            var f4 = new SingleThreadFlow();

            f1.Add(pc);


            Hub.Instance
                .Attach(f1)
                .Attach(f2)
                .Attach(f3)
                .Attach(f4);

            Hub.Startup();

            const int TestCount = 1000 * 1000;

            f1.SubscribeTo("f1");

            Thread.Sleep(100);

            pc.Hello();

            while (pc.RunCount < TestCount)
            {
                Thread.Sleep(10);
            }

            Hub.Shutdown();

            // Case 1. Posted to f1 channel only. 1 million / 4 ~ 5 seconds. 
            // The result is same. Number of threads is more important. 
        }
    }

    class PerfCase : Case
    {
        public int RunCount;
        string channel;

        public PerfCase(string channel = "")
        {
            this.channel = channel;
        }

        public void Hello()
        {
            Post(new HelloCaseEvent());
        }

        protected override void Setup()
        {
            base.Setup();

            new HelloCaseEvent().Bind(OnHello);
        }

        void Post(Event e)
        {
            e._Channel = channel;
            e.Post();
        }

        void OnHello(HelloCaseEvent evt)
        {
            ++RunCount;

            new HelloCaseEvent().Post();
        }
    }
}
