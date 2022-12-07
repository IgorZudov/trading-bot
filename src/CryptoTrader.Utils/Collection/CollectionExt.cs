using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CryptoTrader.Utils.Collection
{
    public static class CollectionExt
    {
        [NotNull, Pure]
        public static IEnumerable<T[]> Split<T>([NotNull] this IEnumerable<T> source, int size)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            return SplitImpl(source, size);
        }

        private static IEnumerable<T[]> SplitImpl<T>(IEnumerable<T> source, int size)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return SplitSequence(enumerator, size);
        }

        private static T[] SplitSequence<T>(IEnumerator<T> enumerator, int size)
        {
            var count = 0;
            var items = new T[size];

            do
            {
                items[count++] = enumerator.Current;
            } while (count < size && enumerator.MoveNext());

            if (count < size)
                Array.Resize(ref items, count);

            return items;
        }
    }
}
