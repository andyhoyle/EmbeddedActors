using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace EmbeddedActors
{
    public class ActorRef<T> where T : Actor
    {
        private T _actor;
        private string _id;

        public ActorRef(string id)
        {
            _id = id;
        }

        public Subject<Event> Stream
        {
            get
            {
                Activate();
                return _actor.Stream;
            }
        }

        public async Task Tell(Command<T> command)
        {
            await Activate();
            var events = Dispatcher<T>.Dispatch(_actor, command);

            events.ForEach(e => {
                Dispatcher<T>.Dispatch(_actor, e);
                _actor.Stream.OnNext(e);
            });
        }

        public async Task<U> Ask<U>(Query<T,U> query)
        {
            await Activate();
            return await Dispatcher<T>.Dispatch<U>(_actor, query);
        }

        private Task Activate()
        {
            if(_actor == null)
            {
                _actor = (T)Activator.CreateInstance(typeof(T), _id);
                _actor.Initialise();
            }

            return Task.FromResult(0);
        }
    }
}
