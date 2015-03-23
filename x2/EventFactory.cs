using System;
using System.Collections.Generic;
using System.Reflection;

namespace x2
{
    public static class EventFactory
    {
        private static IDictionary<int, Func<Event>> register;

        static EventFactory()
        {
            register = new Dictionary<int, Func<Event>>();
        }

        public static Event Create(int typeId)
        {
            Func<Event> factoryMethod;
            if (!register.TryGetValue(typeId, out factoryMethod))
            {
                return null;
            }
            return factoryMethod();
        }

        public static Event Create(Deserializer deserializer)
        {
            int typeId;
            deserializer.Read(out typeId);
            Event result = Create(typeId);
            if (Object.ReferenceEquals(result, null))
            {
                Log.Error("Event.Create: unknown event type id {0}", typeId);
            }
            return result;
        }

        public static void Register<T>() where T : Event
        {
            Register(typeof(T));
        }

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

        private static void Register(Type type)
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

        public static void Register(int typeId, Func<Event> factoryMethod)
        {
            Func<Event> existing;
            if (register.TryGetValue(typeId, out existing))
            {
                if (!existing.Equals(factoryMethod))
                {
                    throw new Exception(
                        String.Format("Event typeid {0} conflicted", typeId));
                }
                return;
            }
            register.Add(typeId, factoryMethod);
        }
    }
}
