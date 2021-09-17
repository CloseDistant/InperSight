using System.Collections.Concurrent;
using System.Collections.Generic;

namespace InperStudio.Lib.Helper
{
    public static class InperClassExtensionHelper
    {
        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            return ((IDictionary<TKey, TValue>)self).Remove(key);
        }
    }
}
