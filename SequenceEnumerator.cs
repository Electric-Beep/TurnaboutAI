using System;
using System.Collections;
using System.Collections.Generic;

namespace TurnaboutAI
{
    /// <summary>
    /// An enumerator that wraps an enumerable, enumerating any enumerables it may yield.
    /// </summary>
    public sealed class SequenceEnumerator : IEnumerator
    {
        private readonly Stack<IEnumerator> _evalStack = new Stack<IEnumerator>();

        public SequenceEnumerator(IEnumerator inner)
        {
            _evalStack.Push(inner);
        }

        public SequenceEnumerator(IEnumerable inner)
        {
            _evalStack.Push(inner.GetEnumerator());
        }

        private object _current;
        public object Current => _current;

        public bool MoveNext()
        {
            while(_evalStack.Count != 0)
            {
                var cur = _evalStack.Pop();

                if(cur.MoveNext())
                {
                    _evalStack.Push(cur);

                    if (cur.Current is IEnumerator enumerator)
                    {
                        _evalStack.Push(enumerator);
                    }
                    else if(cur.Current is IEnumerable enumerable)
                    {
                        _evalStack.Push(enumerable.GetEnumerator());
                    }
                    else
                    {
                        _current = cur.Current;
                        return true;
                    }
                }
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
