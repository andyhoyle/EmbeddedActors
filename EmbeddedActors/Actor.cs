using System.Reactive.Subjects;

namespace EmbeddedActors
{
    public abstract class Actor 
    {
        public readonly string Id;

        public Subject<Event> Stream = new Subject<Event>();

        public Actor(string id)
        {
            Id = id;
        }

        public abstract void Initialise();
    }
}
