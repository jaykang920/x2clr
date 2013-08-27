// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Threading;

namespace x2
{
    /// <summary>
    /// Manages a fixed-length compact array of bit values.
    /// </summary>
    /// <para>
    /// A bit array manages an array of bit values which are represented as
    /// Booleans, where <b>true</b> indicates that the bit is on (1) and
    /// <b>false</b> indicates the bit is off (0).
    /// </para>
    /// <para>
    /// Fingerprint is a special-purpose variant of an ordinary bit array,
    /// dedicated to keep track of the property assignment of
    /// <see cref="x2.Cell">Cell</see>-derived objects.
    /// </para>
    public class Fingerprint : IComparable<Fingerprint>
    {
        private readonly int[] blocks;
        private readonly int length;

        /// <summary>
        /// Gets the number of bits contained in the Fingerprint.
        /// </summary>
        public int Length
        {
            get { return length; }
        }

        /// <summary>
        /// Initializes a new instance of the Fingerprint class that contains bit
        /// values copied from the specified Fingerprint.
        /// </summary>
        /// <param name="other">A Fingerprint object to copy from.</param>
        public Fingerprint(Fingerprint other)
        {
            blocks = (int[])other.blocks.Clone();
            length = other.length;
        }

        /// <summary>
        /// Initializes a new instance of the Fingerprint class that can hold the
        /// specified number of bit values, which are initially set to <b>false</b>.
        /// </summary>
        /// <param name="length">
        /// The number of bit values in the new Fingerprint.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <c>length</c> is less than 0.
        /// </exception>
        public Fingerprint(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            blocks = new int[((length - 1) >> 5) + 1];
            this.length = length;
        }

        /// <summary>
        /// Clears all the bits in the Fingerprint, making them <b>false</b>.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < blocks.Length; ++i)
            {
                blocks[i] = 0;
            }
        }

        /// <summary>
        /// Compares this Fingerprint with the specified Fingerprint object.
        /// </summary>
        /// Implements IComparable(T).CompareTo interface.
        /// <param name="other">
        /// A Fingerprint object to be compared with this.
        /// </param>
        /// <returns>
        /// A value that indicates the relative order of the Fingerprint objects
        /// being compared. Zero return value means that this is equal to
        /// <c>other</c>, while negative(positive) integer return value means that
        /// this is less(greater) than <c>other</c>.
        /// </returns>
        public int CompareTo(Fingerprint other)
        {
            if (length < other.length)
            {
                return -1;
            }
            else if (length > other.length)
            {
                return 1;
            }
            for (int i = (blocks.Length - 1); i >= 0; --i)
            {
                uint block = (uint)blocks[i];
                uint otherBlock = (uint)other.blocks[i];
                if (block < otherBlock)
                {
                    return -1;
                }
                else if (block > otherBlock)
                {
                    return 1;
                }
            }
            return 0;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this Fingerprint.
        /// </summary>
        /// <param name="obj">An object to compare with this Fingerprint.</param>
        /// <returns>
        /// <b>true</b> if <c>obj</c> is equal to this Fingerprint; otherwise,
        /// <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }
            Fingerprint other = obj as Fingerprint;
            if (other == null || length != other.length)
            {
                return false;
            }
            for (int i = 0; i < blocks.Length; ++i)
            {
                if (blocks[i] != other.blocks[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the bit value at the specified position in the Fingerprint.
        /// </summary>
        /// <param name="index">The zero-based index of the value to get.</param>
        /// <returns>The bit value at the position <c>index</c>.</returns>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when <c>index</c> is less than 0, or when <c>index</c> is
        /// greater than or equal to the length of the Fingerprint.
        /// </exception>
        public bool Get(int index)
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException();
            }
            return (blocks[index >> 5] & (1 << index)) != 0;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>
        /// An integer that can serve as the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            Hash hash = new Hash(Hash.Seed);
            hash.Update(length);
            for (int i = 0; i < blocks.Length; ++i)
            {
                hash.Update(blocks[i]);
            }
            return hash.Code;
        }

        /// <summary>
        /// Determines whether the specified Fingerprint object is equivalent to 
        /// this Fingerprint.
        /// </summary>
        /// A Fingerprint is said to be <i>equivalent</i> to the other when it 
        /// covers all the bits set in the other.
        /// <remarks>
        /// Given two Fingerprint objects x and y, x.IsEquivalent(y) returns
        /// <b>true</b> if:
        ///   <list type="bullet">
        ///     <item>x.Length is less than or equal to y.Length</item>
        ///     <item>All the bits set in x are also set in y</item>
        ///   </list>
        /// </remarks>
        /// <param name="other">
        /// A Fingerprint object to compare with this Fingerprint.
        /// </param>
        /// <returns>
        /// <b>true</b> if <c>other</c> is equivalent to this Fingerprint;
        /// otherwise, <b>false</b>.
        /// </returns>
        public bool IsEquivalent(Fingerprint other)
        {
            if (length > other.length)
            {
                return false;
            }
            for (int i = 0; i < blocks.Length; ++i)
            {
                int block = blocks[i];
                if ((block & other.blocks[i]) != block)
                {
                    return false;
                }
            }
            return true;
        }

        public void Dump(Buffer buffer)
        {
            //buffer.WriteUInt29(length);
            int numBytes = ((length - 1) >> 3) + 1;
            int count = 0;
            foreach (int block in blocks)
            {
                for (int j = 0; (j < 4) && (count < numBytes); ++j, ++count)
                {
                    buffer.Write((byte)(block >> (j << 3)));
                }
            }
        }

        public void Load(Buffer buffer)
        {
            /*
            int length;
            buffer.ReadUInt29(out length);
            if (this.length != length) {
              throw new System.IO.InvalidDataException();
            }
            */
            int numBytes = ((length - 1) >> 3) + 1;
            int count = 0;
            for (int i = 0; i < blocks.Length; ++i)
            {
                blocks[i] = 0;
                for (int j = 0; (j < 4) && (count < numBytes); ++j, ++count)
                {
                    blocks[i] |= ((int)buffer.ReadByte() << (j << 3));
                }
            }
        }

        /// <summary>
        /// Sets the bit at the specified position in the Fingerprint.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to set.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when <c>index</c> is less than 0, or when <c>index</c> is
        /// greater than or equal to the length of the Fingerprint.
        /// </exception>
        public void Touch(int index)
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException();
            }
            blocks[index >> 5] |= (1 << index);
        }

        /// <summary>
        /// Clears the bit at the specified position in the Fingerprint.
        /// </summary>
        /// <param name="index">The zero-based index of the bit to clear.</param>
        /// <exception cref="System.IndexOutOfRangeException">
        /// Thrown when <c>index</c> is less than 0, or when <c>index</c> is
        /// greater than or equal to the length of the Fingerprint.
        /// </exception>
        public void Wipe(int index)
        {
            if (index < 0 || index >= length)
            {
                throw new IndexOutOfRangeException();
            }
            blocks[index >> 5] &= ~(1 << index);
        }

        /// <summary>
        /// Provides offset-based indexer for the underlying Fingerprint object.
        /// </summary>
        /// This trivial value type offers just a syntactic sugar to get Fingerprint
        /// bits, applying predefined position offset.
        public struct View
        {
            private readonly Fingerprint fingerprint;
            private readonly int offset;

            /// <summary>
            /// Initializes a new instance of Fingerprint.View structure with the 
            /// specified Fingerprint and offset.
            /// </summary>
            /// <param name="fingerprint">A Fingerprint object to access.</param>
            /// <param name="offset">An integer offset to apply constantly.</param>
            public View(Fingerprint fingerprint, int offset)
            {
                this.fingerprint = fingerprint;
                this.offset = offset;
            }

            /// <summary>
            /// Gets the bit value at the specified position in the underlying 
            /// Fingerprint, applying the offset.
            /// </summary>
            /// This method does not throw on upper-bound overrun. If the calculated
            /// position index (<c>offset</c> + <c>index</c>) is greater than or equal
            /// to the length of the underlying Fingerprint, it simply returns 
            /// <b>false</b>.
            /// <param name="index">The zero-based index of the value to get.</param>
            /// <returns>
            /// The bit value at the position (<c>offset</c> + <c>index</c>).
            /// </returns>
            public bool this[int index]
            {
                get
                {
                    int actualIndex = offset + index;
                    if (actualIndex >= fingerprint.Length)
                    {
                        return false;
                    }
                    return fingerprint.Get(actualIndex);
                }
            }
        }
    }

    /// <summary>
    /// Extends Fingerprint class to hold an additional reference count.
    /// </summary>
    internal class Slot : Fingerprint, IComparable<Slot>
    {
        private int refCount;

        /// <summary>
        /// Initializes a new instance of the Slot class that contains bit values
        /// copied from the specified Fingerprint.
        /// </summary>
        /// <param name="fingerprint">A Fingerprint object to copy from.</param>
        public Slot(Fingerprint fingerprint)
            : base(fingerprint)
        {
            refCount = 1;
        }

        /// <summary>
        /// Increases the reference count of this Slot.
        /// </summary>
        /// <returns>The resultant reference count.</returns>
        public int IncrementRefCount()
        {
            return Interlocked.Increment(ref refCount);
        }

        /// <summary>
        /// Compares this Slot with the specified Slot object.
        /// </summary>
        /// Implements IComparable(T).CompareTo interface.
        /// <param name="other">
        /// A Slot object to be compared with this.
        /// </param>
        /// <returns>
        /// A value that indicates the relative order of the Slot objects being
        /// compared. Zero return value means that this is equal to <c>other</c>,
        /// while negative(positive) integer return value means that this is
        /// less(greater) than <c>other</c>.
        /// </returns>
        public int CompareTo(Slot other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Decreases the reference count of this Slot.
        /// </summary>
        /// <returns>The resultant reference count.</returns>
        public int DecrementRefCount()
        {
            return Interlocked.Decrement(ref refCount);
        }
    }
}
