namespace TurnaboutAI.NeuroAPI
{
    /// <summary>
    /// Token to propagate a cancellation request.
    /// </summary>
    public readonly struct CancellationToken
    {
        private readonly CancellationTokenSource _source;

        internal CancellationToken(CancellationTokenSource source)
        {
            _source = source;
        }

        /// <summary>
        /// Gets if cancellation was requested.
        /// </summary>
        public bool IsCancellationRequested => _source != null && _source.IsCancellationRequested;
    }
}
