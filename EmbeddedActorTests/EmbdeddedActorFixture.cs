using EmbeddedActors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace EmbeddedActorTests
{
    [TestFixture]
    public class EmbdeddedActorFixture
    {
        [Test]
        [Repeat(1000)]
        public async Task EnsureHandlersWorkAsPCL()
        {
            List<Event> eventsReceived = new List<Event>();
            Subject<Event> subscription = new Subject<Event>();

            ActorRef<TestActor> actor = ActorSystem.ActorOf<TestActor>(Guid.NewGuid().ToString());

            actor.Stream.Subscribe((x) => eventsReceived.Add(x));

            await actor.Tell(new TestCommand(1));
            await actor.Tell(new TestCommand(2));

            int answer = await actor.Ask(new TestQuery());

            Assert.AreEqual(2, eventsReceived.Count);
            Assert.AreEqual(3, answer);
        }
    }
}
