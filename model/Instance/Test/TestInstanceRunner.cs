using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using x2;

namespace Test
{
    [TestFixture]
    public class TestInstanceRunner
    {
        [Test]
        public void TestStartup()
        {

            Hub.Instance
                .Attach(
                    new SingleThreadFlow()
                    .Add(new Server.Game.InstanceCoordinator())
                )
                .Attach(
                    new SingleThreadFlow()
                    .Add(new Server.Game.InstanceRunner())
                )
                .Attach(
                    new SingleThreadFlow()
                    .Add(new Server.Game.InstanceRunner())
                );


        }

        [Test]
        public void TestJoin()
        {

        }
    }
}
