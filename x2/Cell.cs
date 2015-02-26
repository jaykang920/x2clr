// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// Common base class for all custom types.
    /// </summary>
    public abstract class Cell
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        protected static readonly Tag tag;

        /// <summary>
        /// Fingerprint to keep track of property assignments.
        /// </summary>
        protected readonly Fingerprint fingerprint;

        static Cell()
        {
            tag = new Tag(null, typeof(Cell), 0);
        }

        /// <summary>
        /// Initializes a new Cell instance with the specified Fingerprint.
        /// </summary>
        protected Cell(Fingerprint fingerprint)
        {
            this.fingerprint = fingerprint;
        }

        /// <summary>
        /// Initializes a new Cell instance with the specified fingerprint length.
        /// </summary>
        protected Cell(int length)
        {
            fingerprint = new Fingerprint(length);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current
        /// object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            // Any comparison except the reference equality is delegated to the
            // EqualsTo() method chain.
            Cell other = obj as Cell;
            if (other == null)
            {
                return false;
            }
            return other.EqualsTo(this);
        }

        /// <summary>
        /// Determines whether this Cell object is equal to the specified one.
        /// </summary>
        public virtual bool EqualsTo(Cell other)
        {
            if (GetType() != other.GetType())
            {
                return false;
            }
            if (!fingerprint.Equals(other.fingerprint))
            {
                return false;
            }
            return true;
        }

        public Fingerprint GetFingerprint()
        {
            return fingerprint;
        }

        /// <summary>
        /// Returns the hash code for the current object.
        /// </summary>
        public override int GetHashCode()
        {
            return GetHashCode(fingerprint);
        }

        public virtual int GetHashCode(Fingerprint fingerprint)
        {
            return Hash.Seed;
        }

        /// <summary>
        /// Returns the custom type tag of this object.
        /// </summary>
        public virtual Tag GetTypeTag()
        {
            return tag;
        }

        /// <summary>
        /// Determines whether the specified Cell object is equivalent to this
        /// one.
        /// </summary>
        /// A Cell is said to be equivalent to the other if its fingerprint is
        /// equivalent to the other's, and all the fingerprinted properties of
        /// the other exactly matches with their counterparts.
        /// <remarks>
        /// Given two Cell objects x and y, x.IsEquivalent(y) returns true if:
        ///   <list type="bullet">
        ///     <item>x.fingerprint.IsEquivalent(y.fingerprint) returns true.
        ///     </item>
        ///     <item>All the fingerprinted properties in x are equal to those
        ///     in y.</item>
        ///   </list>
        /// </remarks>
        public virtual bool IsEquivalent(Cell other)
        {
            if (!other.IsKindOf(this))
            {
                return false;
            }
            if (!fingerprint.IsEquivalent(other.fingerprint))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether this Cell object is a kind of the specified Cell
        /// in the custom type hierarchy.
        /// </summary>
        public bool IsKindOf(Cell other)
        {
            Tag tag = GetTypeTag();
            Tag otherTag = other.GetTypeTag();
            while (tag != null)
            {
                if (tag == otherTag)
                {
                    return true;
                }
                tag = tag.Base;
            }
            return false;
        }

        /// <summary>
        /// Returns a string that describes the current object.
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetTypeTag().RuntimeType.Name);
            Describe(stringBuilder);
            return stringBuilder.ToString();
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
        protected virtual void Describe(StringBuilder stringBuilder)
        {
            return;
        }

        // [SERIALIZER] test
        public virtual void Deserialize(Serializer serializer)
        {
            fingerprint.Deserialize(serializer);
        }

        public virtual int GetEncodedLength()
        {
            return fingerprint.GetEncodedLength();
        }

        public virtual void Serialize(Serializer serializer)
        {
            fingerprint.Serialize(serializer);
        }

        /// <summary>
        /// Dumps this Cell object through the specified serializer.
        /// </summary>
        public virtual void Dump(Serializer serializer)
        {
            Serialize(serializer);
        }
        public static void Load<T>(Serializer serializer, out T value) where T : Cell, new()
        {
            var type = typeof(T);
            var eventType = typeof(Event);
            if (type.IsSubclassOf(eventType) || type == eventType)
            {
                value = Event.Create(serializer) as T;
            }
            else
            {
                value = new T();
            }
            if (!Object.ReferenceEquals(value, null))
            {
                value.Deserialize(serializer);
            }
        }
        // [SERIALIZER] test

        #region Operators

        public static bool operator ==(Cell x, Cell y)
        {
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }
            if (((object)x == null) || ((object)y == null))
            {
                return false;
            }
            return x.Equals(y);
        }

        public static bool operator !=(Cell x, Cell y)
        {
            return !(x == y);
        }

        #endregion

        /// <summary>
        /// Supports light-weight custom type hierarchy for Cell and its subclasses.
        /// </summary>
        public class Tag
        {
            /// <summary>
            /// Gets the immediate base type tag.
            /// </summary>
            /// Returns null if this is a root tag.
            public Tag Base { get; private set; }

            /// <summary>
            /// Gets the correspondent runtime type.
            /// </summary>
            public Type RuntimeType { get; private set; }

            /// <summary>
            /// Gets the number of immediate (directly defined) properties in this type.
            /// </summary>
            public int NumProps { get; private set; }

            /// <summary>
            /// Gets the fingerprint offset for immediate properties in this type.
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// Initializes a new instance of the Cell.Tag class.
            /// </summary>
            public Tag(Tag baseTag, Type runtimeType, int numProps)
            {
                Base = baseTag;
                RuntimeType = runtimeType;
                NumProps = numProps;
                if (baseTag != null)
                {
                    Offset = baseTag.Offset + baseTag.NumProps;
                }
            }
        }
    }
}
