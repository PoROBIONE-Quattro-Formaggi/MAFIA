using System;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DataStorage.ObservableDictionary
{
    public sealed class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public delegate void DictionaryChangedEventHandler(object sender,
            DictionaryChangedEventArgs<TKey, TValue> e);

        public event DictionaryChangedEventHandler DictionaryChanged;

        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                var oldValue = ContainsKey(key) ? base[key] : default;
                base[key] = value;

                if (ContainsKey(key))
                {
                    OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(key, value,
                        DictionaryChangeType.Update, oldValue));
                }
                else
                {
                    OnDictionaryChanged(
                        new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryChangeType.Add));
                }
            }
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnDictionaryChanged(new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryChangeType.Add));
        }

        public new bool Remove(TKey key)
        {
            if (!TryGetValue(key, out var value)) return false;
            var result = base.Remove(key);
            OnDictionaryChanged(
                new DictionaryChangedEventArgs<TKey, TValue>(key, value, DictionaryChangeType.Remove));
            return result;
        }

        private void OnDictionaryChanged(DictionaryChangedEventArgs<TKey, TValue> e)
        {
            DictionaryChanged?.Invoke(this, e);
        }
    }

    public enum DictionaryChangeType
    {
        Add,
        Remove,
        Update
    }

    public class DictionaryChangedEventArgs<TKey, TValue> : EventArgs
    {
        public TKey Key { get; }
        public TValue Value { get; }
        public DictionaryChangeType ChangeType { get; }
        public TValue OldValue { get; }

        public DictionaryChangedEventArgs(TKey key, TValue value, DictionaryChangeType changeType,
            TValue oldValue = default)
        {
            Key = key;
            Value = value;
            ChangeType = changeType;
            OldValue = oldValue;
        }
    }
}