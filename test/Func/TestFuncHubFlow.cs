using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
                    new SingleThreadFlow().Add(new MyCase())
                );

            Hub.Startup(); 

            // MyCase.Setup called

            Hub.Shutdown(); 

            // MyCase.Teardown called
        }

        [Test]
        public void TestEventPostBetweenCases()
        {
            Hub.Instance
                .Attach( 
                    new SingleThreadFlow().Add(new MyCase())
                );

            Hub.Startup(); 

            // Wait till MyCase shutdown
            
        }

    }

    public class MyCase : Case
    {
        private int _helloCount = 0;

        protected override void Setup()
        {
            // Post start event


        }

        protected override void Teardown()
        {

        }

        private void OnHelloCase(Event e)
        {
            ++_helloCount;

            if (_helloCount < 10)
            {
                // Post event 
            }
            else
            {
                Hub.Shutdown();
            }
        }
    }
}
