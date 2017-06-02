namespace EmbeddedActors
{
    public interface Command<T> where T : Actor
    {
    }

    public interface Query<TActor,TReturnType> where TActor : Actor
    {

    }

    public interface Event
    {

    }

}
