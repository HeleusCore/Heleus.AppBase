using System;
using System.Collections.Generic;

namespace Heleus.Apps.Shared
{
    public static class ListDiff
    {
        public class ListDiffResult<A, B>
        {
            public readonly IReadOnlyList<bool> OldRemove;
            public readonly IReadOnlyList<bool> NewAdd;

            public ListDiffResult(IReadOnlyList<bool> oldRemove, IReadOnlyList<bool> newAdd)
            {
                OldRemove = oldRemove;
                NewAdd = newAdd;
            }

            public void Process(List<A> oldList, IReadOnlyList<B> newList, Func<A, bool> remove, Action<int, B> add)
            {
                for (var i = oldList.Count - 1; i >= 0; i--)
                {
                    if (OldRemove[i])
                    {
                        if (remove.Invoke(oldList[i]))
                            oldList.RemoveAt(i);
                    }
                }

                for (var i = 0; i < newList.Count; i++)
                {
                    if (NewAdd[i])
                    {
                        add.Invoke(i, newList[i]);
                    }
                }
            }
        }

        public static ListDiffResult<A, B> Compute<A, B>(IReadOnlyList<A> oldList, IReadOnlyList<B> newList, Func<A, B, bool> equals)
        {
            var oldRemove = new List<bool>(oldList.Count);

            var lastFoundIndex = -1;

            for (var o = 0; o < oldList.Count; o++)
            {
                var old = oldList[o];

                var found = false;

                for (var n = 0; n < newList.Count; n++)
                {
                    var @new = newList[n];

                    if (equals.Invoke(old, @new))
                    {
                        if (n > lastFoundIndex)
                        {
                            lastFoundIndex = n;
                            found = true;
                        }
                        break;
                    }
                }

                oldRemove.Add(!found);
            }

            var newAdd = new List<bool>(newList.Count);

            for (var n = 0; n < newList.Count; n++)
            {
                var @new = newList[n];

                var add = true;

                for (var o = 0; o < oldList.Count; o++)
                {
                    var old = oldList[o];

                    if (equals.Invoke(old, @new))
                    {
                        if (!oldRemove[o])
                            add = false;

                        break;
                    }
                }

                newAdd.Add(add);
            }

            return new ListDiffResult<A, B>(oldRemove, newAdd);
        }
    }
}
