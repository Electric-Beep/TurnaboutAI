using System.Threading;

namespace TurnaboutAI.NeuroAPI
{
    /// <summary>
    /// Allows for thread-safe cancellation of operations.
    /// </summary>
    public sealed class CancellationTokenSource
    {
        private const int StateNotCanceled = 0;
        private const int StateCanceled = 1;

        private int _state = StateNotCanceled;

        /// <summary>
        /// Gets if cancellation is requested for this source.
        /// </summary>
        public bool IsCancellationRequested
        {
            get
            {
                return Interlocked.CompareExchange(ref _state, StateCanceled, StateCanceled) == StateCanceled;
            }
        }

        /// <summary>
        /// Gets a new token linked to this source.
        /// </summary>
        public CancellationToken Token => new CancellationToken(this);

        /// <summary>
        /// Sends a cancellation request.
        /// </summary>
        public void Cancel()
        {
            Interlocked.Exchange(ref _state, StateCanceled);
        }
    }
}
