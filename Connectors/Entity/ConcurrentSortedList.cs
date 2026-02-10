using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ControllerExChanges.Entity
{


    public class ConcurrentSortedList
    {
        private readonly ConcurrentDictionary<decimal, decimal> _dictionary = new ConcurrentDictionary<decimal, decimal>();
        private readonly SortedSet<KeyValuePair<decimal, decimal>> _sortedSet;

        public ConcurrentSortedList()
        {
            _sortedSet = new SortedSet<KeyValuePair<decimal, decimal>>(new KeyValuePairComparer());
        }

        private class KeyValuePairComparer : IComparer<KeyValuePair<decimal, decimal>>
        {
            public int Compare(KeyValuePair<decimal, decimal> x, KeyValuePair<decimal, decimal> y)
            {
                return (x.Key).CompareTo(y.Key);
            }
        }

        public bool TryAdd(decimal key, decimal value)
        {
            if (_dictionary.TryAdd(key, value))
            {
                var kvp = new KeyValuePair<decimal, decimal>(key, value);
                lock (_sortedSet)
                {
                    _sortedSet.Add(kvp);
                }
                return true;
            }
            return false;
        }


        public bool TryRemove(decimal key, out decimal value)
        {
            if (_dictionary.TryRemove(key, out value))
            {
                var kvp = new KeyValuePair<decimal, decimal>(key, value);
                lock (_sortedSet)
                {
                    _sortedSet.Remove(kvp);
                }
                return true;
            }
            return false;
        }

        public IEnumerable<KeyValuePair<decimal, decimal>> GetSortedItems()
        {
            lock (_sortedSet)
            {
                return _sortedSet.ToArray();
            }
        }

        public void Clear()
        {
            lock (_sortedSet)
            {
                _sortedSet.Clear();
            }
            _dictionary.Clear();
        }

        public bool SetValue(decimal key, decimal value)
        {
            // Попытка удалить старую пару ключ-значение, если ключ уже существует
            if (_dictionary.TryRemove(key, out _))
            {
                // Если ключ существовал, удаляем его из отсортированного набора
                var oldKvp = new KeyValuePair<decimal, decimal>(key, value);
                lock (_sortedSet)
                {
                    _sortedSet.Remove(oldKvp);
                }
            }

            // Добавляем новую пару ключ-значение в словарь
            if (_dictionary.TryAdd(key, value))
            {
                // Если ключ был успешно добавлен, добавляем его в отсортированный набор
                var newKvp = new KeyValuePair<decimal, decimal>(key, value);
                lock (_sortedSet)
                {
                    _sortedSet.Add(newKvp);
                }
                return true;
            }

            return false;
        }


    }


}
