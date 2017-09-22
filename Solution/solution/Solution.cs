/// Author        : Saurabh Anant Wani 
/// Date Modified : 09/19/2017
/// This program creates a Chained Hash Table using Factory and Iterator Design Patterns. 
/// It targets the .NET Framework 4.6.1.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RIT_CS
{
    /// <summary>
    ///  An exception used to indicate a problem with how
	/// a HashTable instance is being accessed
    /// </summary>
    public class NonExistentKey<Key> : Exception
    {
        /// <summary>
        /// The key that caused this exception to be raised
        /// </summary>
        public Key BadKey { get; private set; }

        /// <summary>
        /// Create a new instance and save the key that
		/// caused the problem.
        /// </summary>
        /// <param name="k">
        /// The key that was not found in the hash table
        /// </param>
        public NonExistentKey(Key k) : base("Non existent key in HashTable: " + k)
        {
            BadKey                              = k;
        }

    }

    /// <summary>
    /// An associative (key-value) data structure.
    /// A given key may not appear more than once in the table,
    /// but multiple keys may have the same value associated with them.
    /// Tables are assumed to be of limited size are expected to automatically
    /// expand if too many entries are put in them.
    /// </summary>
    /// <param name="Key">The types of the table's keys (uses Equals())</param>
    /// <param name="Value">The types of the table's values</param>
    interface Table<Key, Value> : IEnumerable<Key>
    {
        /// <summary>
        /// Add a new entry in the hash table. If an entry with the
        /// given key already exists, it is replaced without error.
        /// put() always succeeds.
        /// (Details left to implementing classes.)
        /// </summary>
        /// <param name="k">the key for the new or existing entry</param>
        /// <param name="v">the (new) value for the key</param>
        void Put(Key k, Value v);

        /// <summary>
        /// Does an entry with the given key exist?
        /// </summary>
        /// <param name="k">the key being sought</param>
        /// <returns>true iff the key exists in the table</returns>
        bool Contains(Key k);

        /// <summary>
        /// Fetch the value associated with the given key.
        /// </summary>
        /// <param name="k">The key to be looked up in the table</param>
        /// <returns>the value associated with the given key</returns>
        /// <exception cref="NonExistentKey">if Contains(key) is false</exception>
        Value Get(Key k);
    }

    class TableFactory
    {
        /// <summary>
        /// Create a Table.
        /// (The student is to put a line of code in this method corresponding to
        /// the name of the Table implementor s/he has designed.)
        /// </summary>
        /// <param name="K">the key type</param>
        /// <param name="V">the value type</param>
        /// <param name="capacity">The initial maximum size of the table</param>
        /// <param name="loadThreshold">
        /// The fraction of the table's capacity that when
        /// reached will cause a rebuild of the table to a 50% larger size
        /// </param>
        /// <returns>A new instance of Table</returns>
        public static Table<K, V> Make<K, V>(int capacity = 100, double loadThreshold = 0.75)
        {
            return new LinkedHashTable<K, V>(capacity, loadThreshold);
        }
    }

    /// <summary>
    /// This class will create nodes to contain the Key-Value pairs in the table. 
    /// </summary>
    /// <typeparam name="Key">The key of the node.</typeparam>
    /// <typeparam name="Value">The value of the node.</typeparam>
    class Node<Key, Value>
    {
        /// <summary>
        /// Contains the key of a node. 
        /// </summary>
        public Key k{ get; set;}
        /// <summary>
        /// Contains the value of a node. 
        /// </summary>
        public Value v { get; set; }
        /// <summary>
        /// The constructor to create the new node. 
        /// </summary>
        /// <param name="k">The key to be assigned to the node.</param>
        /// <param name="v">The value to be assigned to the node.</param>
        public Node(Key k, Value v)
        {
            /// Sets the key of the current node.
            this.k                                      = k;
            /// Sets the value of the current node. 
            this.v                                      = v;
        }

        
    }

    /// <summary>
    /// The chained hash table is created by this class. 
    /// </summary>
    class LinkedHashTable<Key, Value>: Table<Key,Value>
    {
        /// <summary>
        /// Contains the size of the hash table. 
        /// </summary>
        private int capacity;

        /// <summary>
        /// Contains the current number of nodes stored in the hash table. 
        /// </summary>
        public int currentSize;

        /// <summary>
        /// Contains the load threshold value. If the load factor exceeds this 
        /// load threshold, then rehashing is performed. 
        /// </summary>
        private double loadfactorthreshold;

        /// <summary>
        /// Saves the position of a key, if it's value is to be replaced.
        /// Saves iterating through the hash table again.
        /// </summary>
        public int duplicateKeyIndex { get; set; }

        /// <summary>
        /// List to hold the hash table. 
        /// </summary>
        public List<List<Node<Key,Value>>> table;

        /// <summary>
        /// Constructor to create new hash table with pre-defined capacity and 
        /// load threshold value. 
        /// </summary>
        /// <param name="capacity"> 
        /// The size of the hash table. 
        /// </param>
        /// <param name="loadFactorThreshold">
        /// The threshold value to be considered for rehashing. 
        /// </param>
        public LinkedHashTable(int capacity, double loadFactorThreshold )
        {
            /// Sets the initial size of the hashtable, if the value is greaater than 
            /// zero, else throws an exception. 
            if (capacity > 0)
                this.capacity                           = capacity;
            else
                throw new ArgumentOutOfRangeException();

            /// Sets the threshold value of the hash table, if value greater than zero
            /// else throws an exception. 
            if (loadFactorThreshold > 0)
                this.loadfactorthreshold                = loadFactorThreshold;
            else
                throw new ArgumentOutOfRangeException();

            /// Initializes the hash table. 
            table                                       = new List<List<Node<Key, Value>>>(this.capacity);

            /// Initializes the size of the hash table to zero. 
            currentSize = 0;

            /// Initializing the list positions to null values.
            for (int index = 0; index < capacity; index++)
                table.Add(null);
        }

        /// <summary>
        /// Calculates and return the integer position of a given key in the 
        /// hash table. 
        /// </summary>
        /// <param name="k">
        /// Key whose position in the hash table needs to be calculated. 
        /// </param>
        /// <returns>
        /// Integer position of the given key in the hash table.
        /// </returns>
        public int hashTablePosition(Key k)
        {
            /// Calculate the position of the given key in the hash table. 
            int hashcode                                = k.GetHashCode();
            return Math.Abs(hashcode % capacity);
        }

        /// <summary>
        /// Checks if a given key exists in the hash table. 
        /// </summary>
        /// <param name="currentk">
        /// Key that needs to be chcked in the hash table for its presence. 
        /// </param>
        /// <returns>
        /// True if the key exists or false if the key does not exist in the 
        /// hash table. 
        /// </returns>
        public bool Contains(Key currentk)
        {
            /// Calculate the position of the key in the hash table. 
            int position = hashTablePosition(currentk);

            /// Retrieve the list at that position of the hash table. 
            List<Node<Key, Value>> chain                = table[position];

            /// Check if the list is empty, in which case that key is not present 
            /// in the hash table. 
            if (chain == null)
                return false;

            /// If the list at that position is not empty, then iterate through it 
            /// and check if the given key exists there. 
            for (int index= 0; index< table[position].Count; index++)
            {
                if (table[position][index].k.Equals(currentk))
                {
                    duplicateKeyIndex                   = index;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Inserts the given key-value pair in the hash table. If an entry with the same key exists, 
        /// then its value with the new one. 
        /// Also if the given key is null, then exception is thrown. 
        /// </summary>
        /// <param name="currentk">
        /// Key that needs to be stored in the hash table. 
        /// </param>
        /// <param name="currentv">
        /// Value that needs to be stored in the hash table, against the corresponding key.
        /// </param>
        public void Put( Key currentk, Value currentv)
        {
            /// If the given key is null throw ArgumnentNullException error.
            if (currentk == null)
                throw new ArgumentNullException();

            /// Calculates the position of the given key in the hash table. 
            int position                                = hashTablePosition(currentk);

            /// If the given key does not exist in the hashtable, then check if the table needs 
            /// to be rehashed and insert the key-value pair in the hash table. 
            if (Contains(currentk) == false)
            {
                /// Check if the hash table needs to be rehashed. 
                if (((double)currentSize / capacity) > loadfactorthreshold)
                    rehash();

                /// Get the position of the key in the hash table (updates it if the hash table was rehashed).  
                position = hashTablePosition(currentk);

                /// Retrieve the list at the position in the hash table. 
                List<Node<Key, Value>> chain            = table[position];

                /// If there is no list present at that position, then create a new list.
                if (chain == null)
                {
                    chain                               = new List<Node<Key, Value>>();
                    table[position]                     = chain;
                }

                /// Add the key-value pair in the list at the given position of the hash table. 
                chain.Add(new Node<Key, Value>(currentk, currentv));

                /// Update the total number of nodes in the hash table. 
                currentSize++;
            }
            /// If the given key exists in the hash table, then update its value. 
            else
            {
                Node<Key,Value> existingKey             = table[position][duplicateKeyIndex];
                existingKey.v                           = currentv;
            }
        }

        /// <summary>
        /// Performs rehashing of the table by expanding its size by 50% of the 
        /// existing size. 
        /// </summary>
        public void rehash()
        {
            /// Calculates the new capacity of the rehashed hash table. 
            int newCapacity                             = (int)(1.5 * capacity);

            /// Updating the capacity field of the hash table with the new value. 
            capacity = newCapacity;

            /// Storing the existing hash table in a temporary one. 
            List<List<Node<Key, Value>>> intermediate   = table;
            table = new List<List<Node<Key, Value>>>(newCapacity);

            /// Creating a list to store all the nodes from the existing hash table. 
            List<Node<Key, Value>> allNodes             = new List<Node<Key, Value>>();

            /// Initializing the new hash tables with null values. 
            for (int index = 0; index < newCapacity; index++)
                table.Add(null);

            /// Storing all the nodes from the existing hash table in a list. 
            for (int index = 0; index < intermediate.Count; index++)
            {
                if (intermediate[index] != null)
                    allNodes.AddRange(intermediate[index]);
            }

            /// Inserting all the nodes in the new hash table. 
            foreach (var node in allNodes)
                Put(node.k, node.v);
        }

        /// <summary>
        /// Retrieves the value corresponding to the given key from the hash table. 
        /// Aso, first checks if the given key is present in the table or not. 
        /// If the given key is not present in the hash table, then exception is 
        /// thrown. 
        /// </summary>
        /// <param name="currentk">
        /// The key whose value needs to be retrieved.
        /// </param>
        /// <returns>
        /// Return the value corresponding the given key. 
        /// </returns>
        public Value Get(Key currentk)
        {
            /// Checks if the given key is present in the hash table. 
            if (Contains(currentk))
            {
                /// Calculates the positon of the given key in the hash table, to be looked at. 
                int position = hashTablePosition(currentk);
                return table[position][duplicateKeyIndex].v;
            }
            else
                throw new NonExistentKey<Key>(currentk);
        }

        /// <summary>
        /// Enumerates through all the keys of the hash table. 
        /// </summary>
        /// <returns>
        /// Returns a key from the hash table. 
        /// </returns>
        public IEnumerator<Key> GetEnumerator()
        {
            /// Iterates over the positions of the hash table. 
            for (int index = 0; index < capacity; index++)
            {
                /// Checks if the position in the hash table has a list or is empty. 
                if (table[index] != null)
                {
                    /// Iterates over the list at given position 
                    /// of the hash table. 
                    foreach (var node in table[index])
                    {
                        yield return node.k;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gives the enumerator of the hash table. 
        /// </summary>
        /// <returns>
        /// Returns the enumerator of the hash table.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return table.GetEnumerator();
        }

    }

    /// <summary>
    /// Main class. 
    /// </summary>
    class MainClass
    {
        /// <summary>
        /// Main method from where the execution starts. 
        /// </summary>
        public static void Main(string[] args)
        {
            Table<String, String> ht = TableFactory.Make<String, String>(4, 0.5);
            ht.Put("Joe", "Doe");
            ht.Put("Jane", "Brain");
            ht.Put("Chris", "Swiss");
            try
            {
                foreach (String first in ht)
                {
                    Console.WriteLine(first + " -> " + ht.Get(first));
                }
                Console.WriteLine("=========================");

                ht.Put("Wavy", "Gravy");
                ht.Put("Chris", "Bliss");
                foreach (String first in ht)
                {
                    Console.WriteLine(first + " -> " + ht.Get(first));
                }
                Console.WriteLine("=========================");

                Console.Write("Jane -> ");
                Console.WriteLine(ht.Get("Jane"));
                Console.Write("John -> ");
                Console.WriteLine(ht.Get("John"));
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }
            TestTable.test();
            Console.ReadLine();
        }
    }
    
    /// <summary>
    /// This class tests out the functionalities of the hash table. 
    /// </summary>
    class TestTable
    {
        /// <summary>
        /// Test method, which contains the different test scenarios for testing the hash table. 
        /// </summary>
        public static void test()
        {
            /// Create a hash table to store integer type key-value pair.
            Table<int, int> t1 = TableFactory.Make<int, int>(10, 0.75);

            Console.WriteLine("\n------------------1. Adding large number of keys in the HashTable (Test Rehashing)------------------");
            /// Add 2000 elements in the hash table
            for (int index = 0; index < 2000; index++)
                t1.Put(index, index + 100);
            Console.WriteLine("2000 elements added.");
            Console.WriteLine("----------------------------------------------------------------------------------------------------\n");

            /// Create hash table to store string type key-value pair. 
            Table<String, String> t2 = TableFactory.Make<String, String>(10, 0.75);

            Console.WriteLine("------------------2. Add a null key-----------------------------------------------------------------");
            /// Trying to insert a null key in the hash table. 
            try
            {
                t2.Put(null, "Null");
            }
            catch (ArgumentNullException exc)
            {
                Console.WriteLine("Invalid key: 'null' !");
                Console.WriteLine(exc.StackTrace);
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");

            /// Inserting and printing out the key-value pairs in the hash table using the Put() and Get() methods. 
            Console.WriteLine("------------------3. Insert/Print Key-Value pair---------------------------------------------------");
            t2.Put("Mumbai", "India");
            t2.Put("London", "United Kingdom");
            t2.Put("New York", "United States");
            t2.Put("Toronto", "Canada");
            t2.Put("Glasgow", "Scotland");
            foreach (String key in t2)
            {
                Console.WriteLine(key + " -> " + t2.Get(key));
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");

            /// Check if a given key exists in the hash table or not.
            Console.WriteLine("------------------4. Check if a key exists? (Valid/Invalid key)------------------------------------");
            Console.WriteLine("Toronto   ?  : " + t2.Contains("Toronto"));
            Console.WriteLine("Singapore ?  : " + t2.Contains("Singapore"));
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");

            /// Check to retrieve the value of a key which does not exist in the hash table. 
            Console.WriteLine("------------------5. Try to get value of non-existent key------------------------------------------");
            try
            {
                Console.WriteLine("Dubai -> " + t2.Get("Dubai"));
            }
            catch (NonExistentKey<String> nek)
            {
                Console.WriteLine(nek.Message);
                Console.WriteLine(nek.StackTrace);
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");

            Console.WriteLine("-----------------6. Invalid Capacity---------------------------------------------------------------");
            try
            {
                Table<int, int> t3 = TableFactory.Make<int, int>(0, 0.75);
            }catch (ArgumentOutOfRangeException argExc)
            {
                Console.WriteLine("Capacity cannot be zero or less than that!");
                Console.WriteLine(argExc.StackTrace);
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");

            Console.WriteLine("-----------------7. Invalid Load Threshold---------------------------------------------------------");
            try
            {
                Table<int, int> t4 = TableFactory.Make<int, int>(10, 0);
            }catch (ArgumentOutOfRangeException argExc)
            {
                Console.WriteLine("Load threshold cannot be zero or less than that!");
                Console.WriteLine(argExc.StackTrace);
            }
            Console.WriteLine("---------------------------------------------------------------------------------------------------\n");
        }
    }
    
}
