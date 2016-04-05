using System;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    /// <summary>
    /// Holds a map of retrievable events and their factory method delegates.
    /// </summary>
    public static class EventFactory
    {
        private static IDictionary<int, Func<Event>> map;

        static EventFactory()
        {
            map = new Dictionary<int, Func<Event>>();
        }

        /// <summary>
        /// Creates a new event instance of the specified type idendifier.
        /// </summary>
        public static Event Create(int typeId)
        {
            Func<Event> factoryMethod;
            if (!map.TryGetValue(typeId, out factoryMethod))
            {
                Log.Error("EventFactory.Create : unknown event type id {0}", typeId);
                return null;
            }
            return factoryMethod();
        }

        /// <summary>
        /// Registers the specified type parameter as a retrievable event.
        /// </summary>
        public static void Register<T>() where T : Event
        {
            Register(typeof(T));
        }

        /// <summary>
        /// Registers all the Event subclasses in the specified assembly as
        /// retrievable events.
        /// </summary>
        public static void Register(Assembly assembly)
        {
            var eventType = typeof(Event);
            var types = assembly.GetTypes();
            for (int i = 0, count = types.Length; i < count; ++i)
            {
                var type = types[i];
                if (type.IsSubclassOf(eventType))
                {
                    Register(type);
                }
            }
        }

        /// <summary>
        /// Registers the specified type as a retrievable event.
        /// </summary>
        public static void Register(Type type)
        {
            PropertyInfo prop = type.GetProperty("TypeId",
                BindingFlags.Public | BindingFlags.Static);
            MethodInfo method = type.GetMethod("New",
                BindingFlags.Public | BindingFlags.Static);

            int typeId = (int)prop.GetValue(null, null);
            Func<Event> factoryMethod = (Func<Event>)
                Delegate.CreateDelegate(typeof(Func<Event>), method);

            Register(typeId, factoryMethod);
        }

        /// <summary>
        /// Registers a retrievable event type identifier with its factory
        /// method.
        /// </summary>
        public static void Register(int typeId, Func<Event> factoryMethod)
        {
            Func<Event> existing;
            if (map.TryGetValue(typeId, out existing))
            {
                if (!existing.Equals(factoryMethod))
                {
                    throw new Exception(
                        String.Format("Event typeid {0} conflicted", typeId));
                }
                return;
            }
            map.Add(typeId, factoryMethod);
        }
    }
}
