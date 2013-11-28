using System;
using System.Collections.Generic;

using NUnit.Framework;

using x2;

namespace x2.Tests
{
    [TestFixture]
    public class BinderTests
    {
        [Test]
        public void TestBinding()
        {
            Binder binder = new Binder();

            binder.Bind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            var e1 = new SampleEvent1();
            var e2 = new SampleEvent1 { Foo = 1 };
            var e3 = new SampleEvent1 { Foo = 1, Bar = "bar" };

            List<Handler> handlerChain = new List<Handler>();

            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e1, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e2, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e3, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);

            binder.Unbind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            handlerChain.Clear();
            Assert.AreEqual(0, binder.BuildHandlerChain(e1, handlerChain));
            handlerChain.Clear();
            Assert.AreEqual(0, binder.BuildHandlerChain(e2, handlerChain));
            handlerChain.Clear();
            Assert.AreEqual(0, binder.BuildHandlerChain(e3, handlerChain));
        }

        [Test]
        public void TestHandlerChainBuilding()
        {
            Binder binder = new Binder();

            binder.Bind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));
            binder.Bind(new SampleEvent1 { Foo = 1 }, new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1));
            binder.Bind(new Event(), new MethodHandler<Event>(OnEvent));

            var e1 = new SampleEvent1();
            var e2 = new SampleEvent1 { Foo = 1 };
            var e3 = new SampleEvent1 { Foo = 1, Bar = "bar" };
            var e4 = new SampleEvent1 { Foo = 2 };
            var e5 = new SampleEvent2 { Foo = 1, Bar = "bar" };
            var e6 = new SampleEvent2 { Foo = 2, Bar = "bar" };

            List<Handler> handlerChain = new List<Handler>();

            handlerChain.Clear();
            Assert.AreEqual(2, binder.BuildHandlerChain(e1, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            handlerChain.Clear();
            Assert.AreEqual(3, binder.BuildHandlerChain(e2, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.AreEqual(3, binder.BuildHandlerChain(e3, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.AreEqual(2, binder.BuildHandlerChain(e4, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            handlerChain.Clear();
            Assert.AreEqual(3, binder.BuildHandlerChain(e5, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[1]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[2]);

            handlerChain.Clear();
            Assert.AreEqual(2, binder.BuildHandlerChain(e6, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSampleEvent1), handlerChain[0]);
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[1]);

            binder.Unbind(new SampleEvent1(), new MethodHandler<SampleEvent1>(OnSampleEvent1));

            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e1, handlerChain));
            Assert.AreEqual(new MethodHandler<Event>(OnEvent), handlerChain[0]);

            binder.Unbind(new Event(), new MethodHandler<Event>(OnEvent));

            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e2, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);

            handlerChain.Clear();
            Assert.AreEqual(1, binder.BuildHandlerChain(e3, handlerChain));
            Assert.AreEqual(new MethodHandler<SampleEvent1>(OnSpecificSampleEvent1), handlerChain[0]);

            handlerChain.Clear();
            Assert.AreEqual(0, binder.BuildHandlerChain(e4, handlerChain));
        }

        void OnEvent(Event e)
        {
        }

        void OnSampleEvent1(SampleEvent1 e)
        {
        }

        void OnSpecificSampleEvent1(SampleEvent1 e)
        {
        }
    }
}
