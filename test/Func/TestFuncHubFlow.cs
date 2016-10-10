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
        [Test]
        public void TestStartupTearDown()
        {
            // Move Example to a functional test to have diverse quick experiments

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(new MyCase(0, 0))
                );

            Hub.Startup(); 

            // MyCase.Setup called

            Hub.Shutdown(); 

            // MyCase.Teardown called
        }

        [Test]
        public void TestEventEcho()
        {
            var mc = new MyCase(0, 0);

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(mc)
                );

            Hub.Startup(); 

            // Wait till MyCase shutdown

            while (mc.Shutdown == false)
            {
                Thread.Sleep(10);
            }

            Hub.Shutdown();

            // Performance : 1 million echo between Hub / Case, 12 seconds.
        }

        [Test]
        public void TestEventBetweenCaseInstances()
        {
            var mc1 = new MyCase(1, 2); 
            var mc2 = new MyCase(2, 1); 

            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(mc1).Add(mc2)
                );

            Hub.Startup(); 

            // Wait till MyCase shutdown

            while (mc1.Shutdown == false)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(mc1.HelloCount == MyCase.TestCount || mc2.HelloCount == MyCase.TestCount);

            Hub.Shutdown();
        }
    }

    public class MyCase : Case
    {
        public volatile bool Shutdown = false;
        public const int TestCount = 1;
        private readonly int _me;
        private readonly int _other;

        public MyCase(int me, int other)
        {
            _me = me;
            _other = other;
        }

        public int HelloCount { get; private set; } = 0;

        protected override void Setup()
        {
            // If an event is bound with a field value, it can affect event dispatching.
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
