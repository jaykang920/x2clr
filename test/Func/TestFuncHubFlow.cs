using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using x2;
using NUnit.Framework;

namespace x2.Tests.Func
{
    [TestFixture]
    public class TestFuncHubFlow
    {
        /// <summary>
        /// Shows a simple process of Hub and Flow startup and shutdown 
        /// </summary>
        [Test]
        public void TestStartupTearDown()
        {
            // Move Example to a functional test to have diverse quick experiments

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(new SimpleCase(0, 0))
                );

            Hub.Startup(); 

            // SimpleCase.Setup called

            Hub.Shutdown(); 

            // SimpleCase.Teardown called
        }

        /// <summary>
        /// Case post and gets callback with a simple event
        /// XXX: This test fails when run with run tests all. 
        /// Hub singleton seems to be the cause of fail.
        /// </summary>
        [Test]
        public void TestEventEcho()
        {
            var mc = new SimpleCase(0, 0);

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(mc)
                );

            Hub.Startup(); 

            // Wait till SimpleCase shutdown

            while (mc.Shutdown == false)
            {
                Thread.Sleep(10);
            }

            Hub.Shutdown();

            // Performance : 1 million echo between Hub / Case, 12 seconds.
        }

        /// <summary>
        /// Shows Case instances can post and callback each other with a same event type
        /// </summary>
        [Test]
        public void TestEventBetweenCaseInstances()
        {
            var mc1 = new SimpleCase(1, 2); 
            var mc2 = new SimpleCase(2, 1); 

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(mc1).Add(mc2)
                );

            Hub.Startup(); 

            // Wait till SimpleCase shutdown

            while (mc1.Shutdown == false)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(mc1.HelloCount == SimpleCase.TestCount || mc2.HelloCount == SimpleCase.TestCount);

            Hub.Shutdown();
        }
    }

    public class SimpleCase : Case
    {
        public volatile bool Shutdown = false;
        public const int TestCount = 1;

        private readonly int _me;
        private readonly int _other;

        public SimpleCase(int me, int other)
        {
            _me = me;
            _other = other;
        }

        public int HelloCount { get; private set; } = 0;

        protected override void Setup()
        {
            // If an event is bound with a field value, it can affect event dispatching.
            // Reason: 
            //  - Binder::BuildHandlerChain() has handlerMap.TryGetValue(equivalent, out handlers) 
            //  - Then TryGetValue uses HashCode of equivalent which reflects hash value of Fingerprint. 
            //  - Then Fingerprint reflects value of assigend field value. 
            //  - This is an ingenious structure. But it is rather difficult to understand at first and debug. 

            new HelloCaseEvent() { Foo = _other }.Bind(OnHelloCase);

            new HelloCaseEvent() { Foo = _me, Bar = "Hello"}.Post();
        }

        protected override void Teardown()
        {
           
        }

        private void OnHelloCase(Event e)
        {
            ++HelloCount;

            if (HelloCount < TestCount )
            {
                // Post event 
                new HelloCaseEvent() { Foo = _other, Bar = "Hello"}.Post();
            }
            else
            {
                Shutdown = true;
            }
        }
    }
}
