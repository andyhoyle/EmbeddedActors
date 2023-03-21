using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EmbeddedActors
{
    public static class Dispatcher<T> where T : Actor
    {
        private static Type _type;

        static readonly string[] conventions = { "On", "Handle" };

        readonly static Dictionary<Type, Func<Actor, Command<T>, IEnumerable<Event>>> _commandHandlers = new Dictionary<Type, Func<Actor, Command<T>, IEnumerable<Event>>>();
        readonly static Dictionary<Type, Action<Actor, Event>> _eventHandlers = new Dictionary<Type, Action<Actor, Event>>();
        readonly static Dictionary<Type, Func<Actor, Query<T, object>, object>> _queryHandlers = new Dictionary<Type, Func<Actor, Query<T, object>, object>>();

        static Dispatcher()
        {
            _type = typeof(T);

            foreach (MethodInfo method in GetMethods(_type))
                Register(method);
        }

        public static IEnumerable<Event> Dispatch(Actor target, Command<T> command)
        {
            Func<Actor, Command<T>, IEnumerable<Event>> handler = _commandHandlers.Find(command.GetType());

            if (handler != null)
                return handler(target, command);

            throw new InvalidOperationException($"Handler for {command} is not registered for {_type}");
        }

        public static void Dispatch(Actor target, Event evt)
        {
            Action<Actor, Event> handler = _eventHandlers.Find(evt.GetType());

            if (handler != null)
            {
                handler(target, evt);
            }
            else
            {
                throw new InvalidOperationException($"Handler for {evt} is not registered for {_type}");
            }
        }

        public static Task<U> Dispatch<U>(Actor target, Query<T, U> query)
        {
            Func<Actor, Query<T, object>, object> handler = _queryHandlers.Find(query.GetType());

            if (handler != null)
            {
                Query<T, object> q = query as Query<T, object>;

                return (Task<U>)handler(target, q);
            }

            throw new InvalidOperationException($"Handler for {query} is not registered for {_type}");
        }

        public static void Register(MethodInfo method)
        {
            Type message = method.GetParameters()[0].ParameterType;

            IEnumerable<Type> interfaces = message.GetTypeInfo().ImplementedInterfaces;

            if (interfaces.Any(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(Command<>) && x.GenericTypeArguments.First() == typeof(T)))
            {
                _commandHandlers.Add(message, MethodToCommmandHandler(method));
            }
            else if (interfaces.Any(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(Query<,>) && x.GenericTypeArguments.First() == typeof(T)))
            {
                _queryHandlers.Add(message, MethodToQueryHandler(method));
            }
            else if (typeof(Event).GetTypeInfo().IsAssignableFrom(message.GetTypeInfo()))
            {
                _eventHandlers.Add(message, MethodToEventHandler(method));
            }
            else
            {
                throw new InvalidOperationException("Supported public methods are Handle, On with supported parameters of Command, Query, Event");
            }
        }

        static Func<Actor, Command<T>, IEnumerable<Event>> MethodToCommmandHandler(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            UnaryExpression targetConversion = Expression.Convert(target, method.DeclaringType);
            UnaryExpression requestConversion = Expression.Convert(request, method.GetParameters()[0].ParameterType);

            MethodCallExpression call = Expression.Call(targetConversion, method, requestConversion);
            Func<Actor, Command<T>, IEnumerable<Event>> func = Expression.Lambda<Func<Actor, Command<T>, IEnumerable<Event>>>(call, target, request).Compile();

            return (t, r) => func(t, r);
        }

        static Func<Actor, Query<T, object>, object> MethodToQueryHandler(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            UnaryExpression targetConversion = Expression.Convert(target, method.DeclaringType);
            UnaryExpression requestConversion = Expression.Convert(request, method.GetParameters()[0].ParameterType);

            MethodCallExpression call = Expression.Call(targetConversion, method, requestConversion);
            Func<Actor, Query<T, object>, object> func = Expression.Lambda<Func<Actor, Query<T, object>, object>>(call, target, request).Compile();

            return (t, r) => func(t, r);
        }

        static Action<Actor, Event> MethodToEventHandler(MethodInfo method)
        {
            ParameterExpression target = Expression.Parameter(typeof(object));
            ParameterExpression request = Expression.Parameter(typeof(object));

            UnaryExpression targetConversion = Expression.Convert(target, method.DeclaringType);
            UnaryExpression requestConversion = Expression.Convert(request, method.GetParameters()[0].ParameterType);

            MethodCallExpression call = Expression.Call(targetConversion, method, requestConversion);
            Action<Actor, Event> func = Expression.Lambda<Action<Actor, Event>>(call, target, request).Compile();

            return (t, r) => func(t, r);
        }

        static IEnumerable<MethodInfo> GetMethods(Type actor)
        {
            while (true)
            {
                if (actor == typeof(Actor))
                    yield break;

                IEnumerable<MethodInfo> methods = actor
                    .GetRuntimeMethods()
                    .Where(m =>
                            m.GetParameters().Length == 1 &&
                            !m.GetParameters()[0].IsOut &&
                            !m.GetParameters()[0].IsRetval &&
                            !m.IsGenericMethod && !m.ContainsGenericParameters &&
                            conventions.Contains(m.Name));

                foreach (MethodInfo method in methods)
                    yield return method;

                actor = actor.GetTypeInfo().BaseType;
            }
        }
    }
}

