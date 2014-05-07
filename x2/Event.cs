// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace x2
{
    /// <summary>
    /// Common base class for all events.
    /// </summary>
    public class Event : Cell
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        new protected static readonly Tag tag;

        private static Factory factory = new Factory();

        public static int TypeId { get { return tag.TypeId; } }

        private string _channel = String.Empty;
        private IntPtr _handle;

        /// <summary>
        /// Name of the hub channel which this event is assigned to.
        /// </summary>
        public string _Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        /// <summary>
        /// Link session handle associated with this event.
        /// </summary>
        public IntPtr _Handle
        {
            get { return _handle; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                _handle = value;
            }
        }

        static Event()
        {
            tag = new Tag(null, typeof(Event), 1, 0);
        }

        /// <summary>
        /// Initializes a new instance of the Event class.
        /// </summary>
        public Event() : base(tag.NumProps) { }

        /// <summary>
        /// Initializes a new instance of the Event class with the specified 
        /// Fingerprint.
        /// </summary>
        protected Event(Fingerprint fingerprint) : base(fingerprint) { }

        /// <summary>
        /// Initializes a new instance of the Event class with the specified 
        /// fingerprint length.
        /// </summary>
        /// <param name="length">
        /// The fingerprint length required to cover all the properties of the 
        /// subclasses.
        /// </param>
        protected Event(int length) : base(length + tag.NumProps) { }

        public static Event Create(int typeId)
        {
            return factory.Create(typeId);
        }

        public static Event New()
        {
            return new Event();
        }

        #region Factory registration methods

        public static void Register(int typeId, Func<Event> factoryMethod)
        {
            factory.Register(typeId, factoryMethod);
        }

        public static void Register<T>() where T : Event
        {
            Register(typeof(T));
        }

        public static void Register(Assembly assembly)
        {
            Type eventType = typeof(Event);
            var types = assembly.GetTypes();
            for (int i = 0, count = types.Length; i < count; ++i)
            {
                var type = types[i];
                if (eventType.IsAssignableFrom(type))
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

            factory.Register(typeId, factoryMethod);
        }

        #endregion

        /// <summary>
        /// Describes the immediate property values into the specified
        /// <see cref="System.Text.StringBuilder">StringBuilder</see>.
        /// </summary>
        /// Each derived class should override this method properly.
        /// <returns>
        /// The string containing the name-value pairs of the immediate 
        /// properties of this class.
        /// </returns>
        protected override void Describe(StringBuilder stringBuilder)
        {
            stringBuilder.AppendFormat(" {0}", GetTypeId());
        }

        /// <summary>
        /// Determines whether this Event object is equal to the specified Event.
        /// </summary>
        /// <param name="other">An Event object to compare with this Event.</param>
        /// <returns>
        /// <b>true</b> if this Event is equal to <c>other</c>; otherwise,
        /// <b>false</b>.
        /// </returns>
        public override bool EqualsTo(Cell other)
        {
            if (!base.EqualsTo(other))
            {
                return false;
            }

            Event o = (Event)other;
            if (_handle != o._handle)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHashCode(fingerprint, GetTypeId());
        }

        public int GetHashCode(Fingerprint fingerprint, int typeId)
        {
            Hash hash = new Hash(GetHashCode(fingerprint));
            hash.Update(typeId);
            return hash.Code;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            Hash hash = new Hash(base.GetHashCode(fingerprint));

            if (fingerprint[0])
            {
                hash.Update(_handle.GetHashCode());
            }

            return hash.Code;
        }

        public virtual int GetTypeId()
        {
            return tag.TypeId;
        }

        public override Cell.Tag GetTypeTag()
        {
            return tag;
        }

        public override bool IsEquivalent(Cell other)
        {
            if (!base.IsEquivalent(other))
            {
                return false;
            }

            Event o = (Event)other;
            if (fingerprint[0])
            {
                if (_handle != o._handle)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether this event object is a kind of the specified type
        /// identifier in the custom type hierarchy.
        /// </summary>
        public bool IsKindOf(int typeId)
        {
            Tag tag = (Tag)GetTypeTag();
            while (tag != null)
            {
                if (tag.TypeId == typeId)
                {
                    return true;
                }
                tag = (Tag)tag.Base;
            }
            return false;
        }

        /// <summary>
        /// Loads the instance members of this object from the specified Buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        public override void Load(Buffer buffer)
        {
            base.Load(buffer);
        }

        public override void Serialize(Buffer buffer)
        {
            buffer.Write(tag.TypeId);
            this.Dump(buffer);
        }

        /// <summary>
        /// Serializes the instance members of this object into the specified 
        /// Buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        protected override void Dump(Buffer buffer)
        {
            base.Dump(buffer);
        }

        #region Convenience methods

        public void Post()
        {
            Flow.Post(this);
        }

        #endregion

        /// <summary>
        /// Supports light-weight custom type hierarchy for Event and its derived 
        /// classes.
        /// </summary>
        new public class Tag : Cell.Tag
        {
            private readonly int typeId;

            /// <summary>
            /// Gets the integer type identifier.
            /// </summary>
            public int TypeId
            {
                get { return typeId; }
            }

            /// <summary>
            /// Initializes a new instance of the Event.Tag class.
            /// </summary>
            /// <param name="baseTag">The base type tag.</param>
            /// <param name="runtimeType">The associated runtime type.</param>
            /// <param name="numProps">The number of immediate properties.</param>
            /// <param name="typeId">The integer type identifier.</param>
            public Tag(Tag baseTag, Type runtimeType, int numProps, int typeId)
                : base(baseTag, runtimeType, numProps)
            {
                this.typeId = typeId;
            }
        }

        public class Factory
        {
            private IDictionary<int, Func<Event>> register;

            public Factory()
            {
                register = new Dictionary<int, Func<Event>>();
            }

            public Event Create(int typeId)
            {
                Func<Event> factoryMethod;
                if (!register.TryGetValue(typeId, out factoryMethod))
                {
                    return null;
                }
                return factoryMethod();
            }

            public void Register(int typeId, Func<Event> factoryMethod)
            {
                register[typeId] = factoryMethod;
            }
        }
    }

    public class EventEquivalent : Event
    {
        private readonly Event e;
        private readonly int typeId;

        public EventEquivalent(Event e, Fingerprint fingerprint, int typeId)
            : base(fingerprint)
        {
            this.e = e;
            this.typeId = typeId;
        }

        public override bool EqualsTo(Cell other)
        {
            return other.IsEquivalent(e);
        }

        public override int GetHashCode()
        {
            return e.GetHashCode(fingerprint, typeId);
        }
    }
}
