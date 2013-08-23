// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    /// <summary>
    /// Common base class for all custom type classes.
    /// </summary>
    public abstract class Cell
    {
        /// <summary>
        /// Per-class type tag to support custom type hierarchy.
        /// </summary>
        protected static readonly Tag tag;

        /// <summary>
        /// Fingerprint to keep track of property assignment.
        /// </summary>
        protected readonly Fingerprint fingerprint;

        static Cell()
        {
            tag = new Tag(null, typeof(Cell), 0);
        }

        /// <summary>
        /// Initializes a new instance of the Cell class with the specified 
        /// Fingerprint.
        /// </summary>
        protected Cell(Fingerprint fingerprint)
        {
            this.fingerprint = fingerprint;
        }

        /// <summary>
        /// Initializes a new instance of the Cell class with the specified 
        /// fingerprint length.
        /// </summary>
        /// <param name="length">
        /// The fingerprint length required to cover all the properties of this
        /// <see cref="x2.Cell">Cell</see>-derived object.
        /// </param>
        protected Cell(int length)
        {
            fingerprint = new Fingerprint(length);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this Cell object. 
        /// </summary>
        /// <param name="obj">An object to compare with this Cell.</param>
        /// <returns>
        /// <b>true</b> if <c>obj</c> is equal to this Cell; otherwise, 
        /// <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            Cell other = obj as Cell;
            if (other == null)
            {
                return false;
            }
            return other.EqualsTo(this);
        }

        /// <summary>
        /// Determines whether this Cell object is equal to the specified Cell.
        /// </summary>
        /// <param name="other">A Cell object to compare with this Cell.</param>
        /// <returns>
        /// <b>true</b> if this Cell is equal to <c>other</c>; otherwise,
        /// <b>false</b>.
        /// </returns>
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

        /// <summary>
        /// Gets the Fingerprint that reflects the property assignment of this
        /// <see cref="x2.Cell">Cell</see>-derived object.
        /// </summary>
        /// <returns>
        /// The fingerprint that reflects the property assignment.
        /// </returns>
        public Fingerprint GetFingerprint()
        {
            return fingerprint;
        }

        public override int GetHashCode()
        {
            return GetHashCode(fingerprint);
        }

        public virtual int GetHashCode(Fingerprint fingerprint)
        {
            return Hash.Update(Hash.Seed, fingerprint.GetHashCode());
        }

        /// <summary>
        /// Returns the custom type tag of this object.
        /// </summary>
        /// <returns>The custom type tag of this object.</returns>
        public virtual Tag GetTypeTag()
        {
            return tag;
        }

        /// <summary>
        /// Determines whether the specified Cell object is equivalent to this Cell.
        /// </summary>
        /// A Cell is said to be <i>equivalent</i> to the other when its fingerprint
        /// is equivalent to the other's, and all the fingerprinted properties of
        /// the other exactly matches with their counterparts.
        /// <remarks>
        /// Given two Cell objects x and y, x.IsEquivalent(y) returns
        /// <b>true</b> if:
        ///   <list type="bullet">
        ///     <item>x.fingerprint.IsEquivalent(y.fingerprint) returns <b>true</b>.
        ///     </item>
        ///     <item>All the fingerprinted properties in x are equal to those in y.
        ///     </item>
        ///   </list>
        /// </remarks>
        /// <param name="other">
        /// A Cell object to compare with this Cell.
        /// </param>
        /// <returns>
        /// <b>true</b> if <c>other</c> is equivalent to this Cell; otherwise,
        /// <b>false</b>.
        /// </returns>
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
        /// Determines whether this Cell object is a kind of the specified Cell in
        /// the custom type hierarchy.
        /// </summary>
        /// <param name="other">A Cell object to check against.</param>
        /// <returns>
        /// <b>true</b> if this Cell is a kind of <c>other</c>; otherwise,
        /// <b>false</b>.
        /// </returns>
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
        /// Loads the instance members of this object from the specified Buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from.</param>
        public virtual void Load(Buffer buffer)
        {
            fingerprint.Load(buffer);
        }

        public virtual void Serialize(Buffer buffer)
        {
            this.Dump(buffer);
        }

        /// <summary>
        /// Returns a string that describes the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(GetTypeTag().RuntimeType.FullName);
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

        /// <summary>
        /// Serializes the instance members of this object into the specified 
        /// Buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write to.</param>
        protected virtual void Dump(Buffer buffer)
        {
            fingerprint.Dump(buffer);
        }

        /// <summary>
        /// Supports light-weight custom type hierarchy for Cell and its derived 
        /// classes.
        /// </summary>
        public class Tag
        {
            private readonly Tag baseTag;
            private readonly Type runtimeType;
            private readonly int numProps;
            private readonly int offset = 0;

            /// <summary>
            /// Gets the base type tag. (Returns <c>null</c> if this is a root tag.)
            /// </summary>
            /// 
            public Tag Base
            {
                get { return baseTag; }
            }

            /// <summary>
            /// Gets the associated runtime type.
            /// </summary>
            public Type RuntimeType
            {
                get { return runtimeType; }
            }

            /// <summary>
            /// Gets the number of immediate (directly-defined) properties in this
            /// type.
            /// </summary>
            public int NumProps
            {
                get { return numProps; }
            }

            /// <summary>
            /// Gets the fingerprint offset for immediate properties in this type.
            /// </summary>
            public int Offset
            {
                get { return offset; }
            }

            /// <summary>
            /// Initializes a new instance of the Cell.Tag class.
            /// </summary>
            /// <param name="baseTag">The base type tag.</param>
            /// <param name="runtimeType">The associated runtime type.</param>
            /// <param name="numProps">The number of immediate properties.</param>
            public Tag(Tag baseTag, Type runtimeType, int numProps)
            {
                this.baseTag = baseTag;
                this.runtimeType = runtimeType;
                this.numProps = numProps;
                if (baseTag != null)
                {
                    offset = baseTag.Offset + baseTag.NumProps;
                }
            }
        }
    }

    public class ListCell<T> : Cell, IEnumerable<T>
    {
        private readonly List<T> list;

        public ListCell()
            : base(0)
        {
            list = new System.Collections.Generic.List<T>();
        }

        public void Add(T item)
        {
            list.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        // Explicit implementation for non-generic System.Collections.IEnumerable
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void Describe(StringBuilder stringBuilder)
        {
            Type itemType = typeof(T);
            stringBuilder.Append(" {");
            foreach (T item in list)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(item.ToString());
            }
            stringBuilder.Append(" }");
            return;
        }
    }

}
