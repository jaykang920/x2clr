using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using x2;
using NUnit.Framework;

namespace x2.Tests.Func
{
    /// <summary>
    /// Game needs to have active logic that runs periodically. 
    /// Usually update loop has been the choicee. 
    /// But other schems can be possible. 
    /// The answer to the scheduling of work from x2 is the TimeFlow.
    /// </summary>
    [TestFixture]
    public class TestFuncTimeFlow
    {

        [Test]
        public void TestScheduling()
        {
            var tc = new TestTimeFlowCase();

            // FrameBaseFlow has its own thread. 

            Hub.Instance
                .Attach(
                    TimeFlow.Default
                )
                .Attach(
                    new SingleThreadFlow().Add(tc)
                );

            Hub.Startup();

            // runs till tc calls back n times

            while ( tc.TimerCallCount < 10)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(tc.TimerCallCount == 10);

            Hub.Shutdown();
        }
    }

    public class TestTimeFlowCase : Case
    {
        public int TimerCallCount { get; set; }

        public TestTimeFlowCase()
            : base()
        {

        }

        protected override void Setup()
        {
            // Reserve callback with TimeFlow.Default.Reserve

            new TimerFlowCaseEvent().Bind(OnTimerEvent);
            new FlowStart().Bind(OnFlowStart);
        }

        protected override void Teardown()
        {

        }

        void OnTimerEvent(TimerFlowCaseEvent e)
        {
            // Can have a final type.

            TimerCallCount++;
        }

        void OnFlowStart(FlowStart e)
        {
            TimeFlow.Default.ReserveRepetition(new TimerFlowCaseEvent(), TimeSpan.FromMilliseconds(10)); 
        }
    }
}