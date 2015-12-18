// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace x2
{
    /// <summary>
    /// Common base class for all events.
    /// </summary>
    public class Event : Cell, IDisposable
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        new protected static readonly Tag tag;

        public static int TypeId { get { return tag.TypeId; } }

        private string _channel;
        private int _handle;
        private bool _transform = true;
        private int _waitHandle;

        /// <summary>
        /// Gets or sets the name of the hub channel which this event is
        /// assigned to.
        /// </summary>
        public string _Channel
        {
            get { return _channel; }
            set { _channel = value; }
        }

        /// <summary>
        /// Gets or sets the link session handle associated with this event.
        /// </summary>
        public int _Handle
        {
            get { return _handle; }
            set
            {
                fingerprint.Touch(tag.Offset + 0);
                _handle = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether this event is to be
        /// transformed or not when it is transferred through a link.
        /// </summary>
        public bool _Transform
        {
            get { return _transform; }
            set { _transform = value; }
        }

        /// <summary>
        /// Gets or sets the coroutine wait handle associated with this event.
        /// </summary>
        public int _WaitHandle
        {
            get { return _waitHandle; }
            set
            {
                fingerprint.Touch(tag.Offset + 1);
                _waitHandle = value;
            }
        }

        static Event()
        {
            tag = new Tag(null, typeof(Event), 2, 0);
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

        public static Event New()
        {
            return new Event();
        }

        /// <summary>
        /// Implements IDisposable interface only to support guarded posting.
        /// </summary>
        public void Dispose()
        {
            Hub.Post(this);
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
            stringBuilder
                .Append(' ')
                .Append(GetTypeId());
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
            if (_waitHandle != o._waitHandle)
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
            hash.Update(-1);
            hash.Update(typeId);
            return hash.Code;
        }

        public override int GetHashCode(Fingerprint fingerprint)
        {
            Hash hash = new Hash(base.GetHashCode(fingerprint));
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                hash.Update(tag.Offset + 0);
                hash.Update(_handle);
            }
            if (touched[1])
            {
                hash.Update(tag.Offset + 1);
                hash.Update(_waitHandle);
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

        public override bool IsEquivalent(Cell other, Fingerprint fingerprint)
        {
            if (!base.IsEquivalent(other, fingerprint))
            {
                return false;
            }
            Event o = (Event)other;
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[0])
            {
                if (_handle != o._handle)
                {
                    return false;
                }
            }
            if (touched[1])
            {
                if (_waitHandle != o._waitHandle)
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

        public override void Deserialize(Deserializer deserializer)
        {
            base.Deserialize(deserializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                deserializer.Read(out _waitHandle);
            }
        }

        public override void Deserialize(VerboseDeserializer deserializer)
        {
            base.Deserialize(deserializer);
            deserializer.Read("WaitHandle", out _waitHandle);
        }
        
        public override int GetEncodedLength()
        {
            int length = Serializer.GetEncodedLength(GetTypeId());
            length += base.GetEncodedLength();
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                length += Serializer.GetEncodedLength(_waitHandle);
            }
            return length;
        }

        public override void Serialize(Serializer serializer)
        {
            serializer.Write(GetTypeId());
            base.Serialize(serializer);
            var touched = new Capo<bool>(fingerprint, tag.Offset);
            if (touched[1])
            {
                serializer.Write(_waitHandle);
            }
        }
        public override void Serialize(VerboseSerializer serializer)
        {
            base.Serialize(serializer);
            serializer.Write("WaitHandle", _waitHandle);
        }

        /// <summary>
        /// Supports light-weight custom type hierarchy for Event and its derived 
        /// classes.
        /// </summary>
        new public class Tag : Cell.Tag
        {
            /// <summary>
            /// Gets the integer type identifier.
            /// </summary>
            public int TypeId { get; private set; }

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
                TypeId = typeId;
            }
        }
    }

    public class EventEquivalent : Event
    {
        public Event InnerEvent { get; set; }
        public int InnerTypeId { get; set; }

        public override bool EqualsTo(Cell other)
        {
            return other.Equivalent(InnerEvent, fingerprint);
        }

        public override int GetHashCode()
        {
            return InnerEvent.GetHashCode(fingerprint, InnerTypeId);
        }
    }
}
