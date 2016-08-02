using System;
using System.Collections.Generic;
using System.Linq;

namespace ZMachine.Tests.TestHelpers
{
    public static class EnumerableExtensions
    {
        static public IEnumerable<IEnumerable<T>> BufferUntil<T>(this IEnumerable<T> input, Func<T, bool> newGroupCondition)
        {
            if (!input.Any())
            {
                yield break;
            }

            List<T> currentGroup = null;
            bool started = false;
            foreach (T value in input)
            {
                if (newGroupCondition(value))
                {
                    if (started)
                    {
                        yield return currentGroup;
                    }
                    else
                    {
                        started = true;
                    }

                    currentGroup = new List<T>();
                }
                currentGroup.Add(value);
            }
            yield return currentGroup;
        }
    }
}
