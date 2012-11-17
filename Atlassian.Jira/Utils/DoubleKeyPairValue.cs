
namespace Util.DoubleKeyDictionary
{
    internal class DoubleKeyPairValue<K, T, V>
    {
        public DoubleKeyPairValue(K key1, T key2, V value)
        {
            this.Key1 = key1;
            this.Key2 = key2;
            this.Value = value;
        }

        public K Key1
        {
            get;
            set;
        }

        public T Key2
        {
            get;
            set;
        }

        public V Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Key1.ToString() + " - " + Key2.ToString() + " - " + Value.ToString();
        }
    }
}
