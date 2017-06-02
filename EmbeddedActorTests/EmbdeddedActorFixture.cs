using EmbeddedActors;
using NUnit.Framework;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

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
            var subscription = new Subject<Event>();
            
            var actor = ActorSystem.ActorOf<TestActor>(Guid.NewGuid().ToString());
            
            actor.Stream.Subscribe((x) => eventsReceived.Add(x));

            await actor.Tell(new TestCommand(1));
            await actor.Tell(new TestCommand(2));

            var answer = await actor.Ask(new TestQuery());

            Assert.AreEqual(2, eventsReceived.Count);
            Assert.AreEqual(3, answer);
        }
    }
}
