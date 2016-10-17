using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework;
using x2;
using Events.Login;

namespace Test
{
    [TestFixture]
    public class TestLogin
    {
        [Test]
        public void TestLoginLogout()
        {
            // -LoginCase : { Session, MasterNetClient | SessionNetServer}
            // -DirectoryUser : { User, MasterNetServer}
            // -ClientCase : { SessionNetServer, SessionNetClient}

            var sessionFlow = new SingleThreadFlow();
            var masterFlow = new SingleThreadFlow();
            var clientFlow = new SingleThreadFlow();

            var loginCase = new Server.Session.LoginCase(); 
            var dirUserCase  = new Server.Master.DirectoryUser();
            var clientCase = new TestClientLoginCase();

            loginCase.AddFilter(EventMasterLoginReq.TypeId, "Master");
            loginCase.AddFilter(EventLoginResp.TypeId, "Client");

            dirUserCase.AddFilter(EventMasterLoginResp.TypeId, "Session");

            // Client Posts LoginReq to Session channel
            clientCase.AddFilter(EventLoginReq.TypeId, "Session");


            Hub.Instance
                .Attach(sessionFlow)
                .Attach(masterFlow)
                .Attach(clientFlow);

            // SessionFlow gets Session channel events
            sessionFlow.Add(loginCase);
            sessionFlow.SubscribeTo("Session");

            masterFlow.Add(dirUserCase);
            masterFlow.SubscribeTo("Master");

            clientFlow.Add(clientCase);
            clientFlow.SubscribeTo("Client");

            Hub.Startup();


            clientCase.ReqLogin("test1", "test1");

            while ( clientCase.Login == false )
            {
                Thread.Sleep(10);
            }

            Hub.Shutdown();
        }
    }

    class TestClientLoginCase : Server.Core.ChannelCase 
    {
        public bool Login = false;

        public void ReqLogin(string account, string password)
        {
            Post(
                new EventLoginReq
                {
                    Account = account, 
                    Password = password
                }
            );
        }

        protected override void Setup()
        {
            base.Setup();

            new EventLoginResp().Bind(OnLoginResp);
        }

        void OnLoginResp(EventLoginResp resp)
        {
            // Just flow test.
            Login = true;
        }
    }
}


