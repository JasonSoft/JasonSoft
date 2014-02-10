#region License
//
// HashSet.cs
//
// Authors:
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace JasonSoft.Components.Quartz.Collection
{
    // HashSet is basically implemented as a reduction of Dictionary<K, V>


    [Serializable, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
    public class HashSet<T> : ICollection<T>, ISerializable, IDeserializationCallback
    {

        const int INITIAL_SIZE = 10;
        const float DEFAULT_LOAD_FACTOR = (90f / 100);
        const int NO_SLOT = -1;

        struct Link
        {
            public int HashCode;
            public int Next;
        }

        // The hash table contains indices into the "links" array
        int[] table;

        Link[] links;
        T[] slots;

        // The number of slots in "links" and "slots" that
        // are in use (i.e. filled with data) or have been used and marked as
        // "empty" later on.
        int touched;

        // The index of the first slot in the "empty slots chain".
        // "Remove ()" prepends the cleared slots to the empty chain.
        // "Add ()" fills the first slot in the empty slots chain with the
        // added item (or increases "touched" if the chain itself is empty).
        int empty_slot;

        // The number of items in this set.
        int count;

        // The number of items the set can hold without
        // resizing the hash table and the slots arrays.
        int threshold;

        IEqualityComparer<T> comparer;
        SerializationInfo si;

        // The number of changes made to this set. Used by enumerators
        // to detect changes and invalidate themselves.
        int generation;

        public int Count
        {
            get { return count; }
        }

        public HashSet()
        {
            Init(INITIAL_SIZE, null);
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            Init(INITIAL_SIZE, comparer);
        }

        public HashSet(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            int capacity = 0;
            ICollection<T> col = collection as ICollection<T>;
            if (col != null)
                capacity = col.Count;

            Init(capacity, comparer);
            foreach (T item in collection)
                Add(item);
        }

        protected HashSet(SerializationInfo info, StreamingContext context)
        {
            si = info;
        }

        void Init(int capacity, IEqualityComparer<T> comparer)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            this.comparer = comparer ?? EqualityComparer<T>.Default;
            if (capacity == 0)
                capacity = INITIAL_SIZE;

            /* Modify capacity so 'capacity' elements can be added without resizing */
            capacity = (int)(capacity / DEFAULT_LOAD_FACTOR) + 1;

            InitArrays(capacity);
            generation = 0;
        }

        void InitArrays(int size)
        {
            table = new int[size];

            links = new Link[size];
            empty_slot = NO_SLOT;

            slots = new T[size];
            touched = 0;

            threshold = (int)(table.Length * DEFAULT_LOAD_FACTOR);
            if (threshold == 0 && table.Length > 0)
                threshold = 1;
        }

        bool SlotsContainsAt(int index, int hash, T item)
        {
            int current = table[index] - 1;
            while (current != NO_SLOT)
            {
                Link link = links[current];
                if (link.HashCode == hash && comparer.Equals(item, slots[current]))
                    return true;

                current = link.Next;
            }

            return false;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0, count);
        }

        public void CopyTo(T[] array, int index)
        {
            CopyTo(array, index, count);
        }

        public void CopyTo(T[] array, int index, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (index > array.Length)
                throw new ArgumentException("index larger than largest valid index of array");
            if (array.Length - index < count)
                throw new ArgumentException("Destination array cannot hold the requested elements!");

            for (int i = 0; i < table.Length && index < count; i++)
            {
                int current = table[i] - 1;
                while (current != NO_SLOT)
                {
                    array[index++] = slots[current];
                    current = links[current].Next;
                }
            }
        }

        void Resize()
        {
            int newSize = PrimeHelper.ToPrime((table.Length << 1) | 1);

            // allocate new hash table and link slots array
            int[] newTable = new int[newSize];
            Link[] newLinks = new Link[newSize];

            for (int i = 0; i < table.Length; i++)
            {
                int current = table[i] - 1;
                while (current != NO_SLOT)
                {
                    int hashCode = newLinks[current].HashCode = comparer.GetHashCode(slots[current]);
                    int index = (hashCode & int.MaxValue) % newSize;
                    newLinks[current].Next = newTable[index] - 1;
                    newTable[index] = current + 1;
                    current = links[current].Next;
                }
            }

            table = newTable;
            links = newLinks;

            // allocate new data slots, copy data
            T[] newSlots = new T[newSize];
            Array.Copy(slots, 0, newSlots, 0, touched);
            slots = newSlots;

            threshold = (int)(newSize * DEFAULT_LOAD_FACTOR);
        }

        public bool Add(T item)
        {
            int hashCode = comparer.GetHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;

            if (SlotsContainsAt(index, hashCode, item))
                return false;

            if (++count > threshold)
            {
                Resize();
                index = (hashCode & int.MaxValue) % table.Length;
            }

            // find an empty slot
            int current = empty_slot;
            if (current == NO_SLOT)
                current = touched++;
            else
                empty_slot = links[current].Next;

            // store the hash code of the added item,
            // prepend the added item to its linked list,
            // update the hash table
            links[current].HashCode = hashCode;
            links[current].Next = table[index] - 1;
            table[index] = current + 1;

            // store item
            slots[current] = item;

            generation++;

            return true;
        }

        public IEqualityComparer<T> Comparer
        {
            get { return comparer; }
        }

        public void Clear()
        {
            count = 0;
            // clear the hash table and the slots
            Array.Clear(table, 0, table.Length);
            Array.Clear(slots, 0, slots.Length);

            // empty the "empty slots chain"
            empty_slot = NO_SLOT;

            touched = 0;
            generation++;
        }

        public bool Contains(T item)
        {
            int hashCode = comparer.GetHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;

            return SlotsContainsAt(index, hashCode, item);
        }

        public bool Remove(T item)
        {
            // get first item of linked list corresponding to given key
            int hashCode = comparer.GetHashCode(item);
            int index = (hashCode & int.MaxValue) % table.Length;
            int current = table[index] - 1;

            // if there is no linked list, return false
            if (current == NO_SLOT)
                return false;

            // walk linked list until right slot (and its predecessor) is
            // found or end is reached
            int prev = NO_SLOT;
            do
            {
                Link link = links[current];
                if (link.HashCode == hashCode && comparer.Equals(slots[current], item))
                    break;

                prev = current;
                current = link.Next;
            } while (current != NO_SLOT);

            // if we reached the end of the chain, return false
            if (current == NO_SLOT)
                return false;

            count--;

            // remove slot from linked list
            // is slot at beginning of linked list?
            if (prev == NO_SLOT)
                table[index] = links[current].Next + 1;
            else
                links[prev].Next = links[current].Next;

            // mark slot as empty and prepend it to "empty slots chain"
            links[current].Next = empty_slot;
            empty_slot = current;

            // clear slot
            slots[current] = default(T);

            generation++;

            return true;
        }

        public int RemoveWhere(Predicate<T> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");

            int counter = 0;

            T[] copy = new T[count];
            CopyTo(copy, 0);

            foreach (T item in copy)
            {
                if (predicate(item))
                {
                    Remove(item);
                    counter++;
                }
            }

            return counter;
        }

        public void TrimExcess()
        {
            Resize();
        }

        // set operations


        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
                Remove(item);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
                if (Contains(item))
                    return true;

            return false;
        }


        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (T item in other)
            {
                if (Contains(item))
                    Remove(item);
                else
                    Add(item);
            }
        }



        public static IEqualityComparer<HashSet<T>> CreateSetComparer()
        {
            throw new NotImplementedException();
        }


        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }


        public virtual void OnDeserialization(object sender)
        {
            if (si == null)
                return;

            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<T>.CopyTo(T[] array, int index)
        {
            CopyTo(array, index);
        }

        void ICollection<T>.Add(T item)
        {
            if (!Add(item))
                throw new ArgumentException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Serializable]
        public struct Enumerator : IEnumerator<T>
        {

            HashSet<T> hashset;
            int index, current;
            int stamp;

            internal Enumerator(HashSet<T> hashset)
            {
                this.hashset = hashset;
                this.stamp = hashset.generation;

                index = -1;
                current = NO_SLOT;
            }

            public bool MoveNext()
            {
                CheckState();

                do
                {
                    if (current != NO_SLOT)
                    {
                        current = hashset.links[current].Next;
                        continue;
                    }

                    if (index + 1 >= hashset.table.Length)
                        return false;

                    current = hashset.table[++index] - 1; ;
                } while (current == NO_SLOT);

                return true;
            }

            public T Current
            {
                get
                {
                    CheckCurrent();

                    return hashset.slots[current];
                }
            }

            object global::System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            void global::System.Collections.IEnumerator.Reset()
            {
                index = -1;
                current = NO_SLOT;
            }

            public void Dispose()
            {
                hashset = null;
            }

            void CheckState()
            {
                if (hashset == null)
                    throw new ObjectDisposedException(null);
                if (hashset.generation != stamp)
                    throw new InvalidOperationException("HashSet have been modified while it was iterated over");
            }

            void CheckCurrent()
            {
                CheckState();

                if (current == NO_SLOT)
                    throw new InvalidOperationException("Current is not valid");
            }
        }

        // borrowed from System.Collections.HashTable
        static class PrimeHelper
        {

            static readonly int[] primes_table = {
				11,
				19,
				37,
				73,
				109,
				163,
				251,
				367,
				557,
				823,
				1237,
				1861,
				2777,
				4177,
				6247,
				9371,
				14057,
				21089,
				31627,
				47431,
				71143,
				106721,
				160073,
				240101,
				360163,
				540217,
				810343,
				1215497,
				1823231,
				2734867,
				4102283,
				6153409,
				9230113,
				13845163
			};

            static bool TestPrime(int x)
            {
                if ((x & 1) != 0)
                {
                    int top = (int)System.Math.Sqrt(x);

                    for (int n = 3; n < top; n += 2)
                    {
                        if ((x % n) == 0)
                            return false;
                    }

                    return true;
                }

                // There is only one even prime - 2.
                return x == 2;
            }

            static int CalcPrime(int x)
            {
                for (int i = (x & (~1)) - 1; i < Int32.MaxValue; i += 2)
                    if (TestPrime(i))
                        return i;

                return x;
            }

            public static int ToPrime(int x)
            {
                for (int i = 0; i < primes_table.Length; i++)
                    if (x <= primes_table[i])
                        return primes_table[i];

                return CalcPrime(x);
            }
        }
    }
}
