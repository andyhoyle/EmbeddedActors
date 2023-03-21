using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbeddedActors
{
    public class TestCommand : Command<TestActor>
    {
        public readonly int Number;
        public TestCommand(int number)
        {
            Number = number;
        }
    }

    public class TestEvent : Event
    {
        public readonly int Number;
        public TestEvent(int number)
        {
            Number = number;
        }
    }

    public class TestQuery : Query<TestActor, int> { }

    public class TestActor : Actor
    {
        private int _sum;

        public TestActor(string id) : base(id) { }

        public IEnumerable<Event> Handle(TestCommand command) => new[] { new TestEvent(command.Number) };

        public void On(TestEvent evt) => _sum += evt.Number;

        public Task<int> Handle(TestQuery query) => Task.FromResult(_sum);

        public override void Initialise()
        {
            // load your state
        }
    }
}
