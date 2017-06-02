using System;
using System.Collections.Generic;

namespace EmbeddedActors
{
    public class ActorSystem
    {
        static Dictionary<Type, Dictionary<string, object>> _cachedActors = new Dictionary<Type, Dictionary<string, object>>();

        public static ActorRef<T> ActorOf<T>(string id) where T : Actor
        {
            if (!_cachedActors.ContainsKey(typeof(T)))
            {
                _cachedActors.Add(typeof(T), new Dictionary<string, object>());
            }

            if (_cachedActors[typeof(T)].ContainsKey(id))
            {
                return (ActorRef<T>)_cachedActors[typeof(T)][id];
            }
            else
            {
                var aref = new ActorRef<T>(id);
                _cachedActors[typeof(T)].Add(id, aref);
                return aref;
            }
        }
    }
}
