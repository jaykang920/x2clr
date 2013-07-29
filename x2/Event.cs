// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace x2
{
    /// <summary>
    /// Common base class for all event classes.
    /// </summary>
    public class Event : Cell
    {
        public delegate Event CreatorDelegate();

        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        new protected static readonly Tag tag;

        private static Factory factory = new Factory();
        public long handle;

        static Event()
        {
            tag = new Tag(null, typeof(Event), 0, 0);
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

        public static void Register(int typeId, CreatorDelegate creator)
        {
            factory.Register(typeId, creator);
        }

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
            return;
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
            return base.EqualsTo(other);
        }

        public override int GetHashCode()
        {
            return Hash.Update(base.GetHashCode(), tag.TypeId);
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            return Hash.Update(base.GetHashCode(fingerprint), tag.TypeId);
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
            return base.IsEquivalent(other);
        }

        public Event New()
        {
            return new Event();
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
            buffer.WriteUInt29(tag.TypeId);
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

        private class Factory
        {
            private IDictionary<int, CreatorDelegate> register;

            public Factory()
            {
                register = new Dictionary<int, CreatorDelegate>();
            }

            public Event Create(int typeId)
            {
                CreatorDelegate creator;
                if (!register.TryGetValue(typeId, out creator))
                {
                    return null;
                }
                return creator();
            }

            public void Register(int typeId, CreatorDelegate creator)
            {
                register[typeId] = creator;
            }

            /* consider supporting auto-registration with reflection
            public void Scan(params Assembly[] assemblies)
            {
              Type eventType = typeof(Event);
              foreach (Assembly assembly in assemblies) {
                foreach (Type type in assembly.GetTypes()) {
                  if (!eventType.IsAssignableFrom(type)) {
                    continue;
                  }
                  FieldInfotype.GetField("tag", BindingFlags.Static);
                  register.Add(type.Name, type);
                }
              }
            }
            */
        }
    }

    public class EventEquivalent : Event
    {
        private readonly Event e;

        public EventEquivalent(Event e, Fingerprint fingerprint)
            : base(fingerprint)
        {
            this.e = e;
        }

        public override bool EqualsTo(Cell other)
        {
            return other.IsEquivalent(e);
        }

        public override int GetHashCode()
        {
            return e.GetHashCode(fingerprint);
        }
    }
}
