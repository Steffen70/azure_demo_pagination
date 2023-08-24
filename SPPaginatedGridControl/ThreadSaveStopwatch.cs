using System.Diagnostics;

namespace SPPaginatedGridControl
{
    public class CustomStopwatch : Stopwatch
    {
        // Create IsRunning volatile property to avoid latened reads
        private volatile bool _isRunning = true;

        public new void Start()
        {
            if (!_isRunning)
                throw new InvalidOperationException("The stopwatch already ran.");
            base.Start();
        }

        public new void Stop()
        {
            _isRunning = false;
            base.Stop();
        }

        public new void Reset()
        {
            throw new InvalidOperationException("Reset is not supported.");
        }

        public event EventHandler<int>? ObserveElapsed;

        public CustomStopwatch()
        {
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);

                    if (Elapsed == TimeSpan.Zero)
                        continue;

                    var elapsed = (int)Elapsed.TotalMilliseconds;

                    ObserveElapsed?.Invoke(this, elapsed);

                    if (!_isRunning)
                        return;
                }
            }).Start();
        }
    }
}
