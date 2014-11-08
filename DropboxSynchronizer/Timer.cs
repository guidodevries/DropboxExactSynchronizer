using System;
using System.Threading;

using DropboxSynchronizer.Interfaces;

namespace DropboxSynchronizer
{
    /// <summary>
    /// Timer which can be started 
    /// </summary>
    internal class Timer : ITimer
    {
        #region Private Members

        private readonly System.Threading.Timer timer;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        public Timer()
        {
            this.timer = new System.Threading.Timer(this.InternalCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the timer ticks.
        /// </summary>
        public event EventHandler TimerTick;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the timer with the specified newInterval. This timer starts immediately.
        /// </summary>
        /// <param name="newInterval">The newInterval in milliseconds.</param>
        public void Start(long newInterval)
        {
            this.timer.Change(0, newInterval);
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Internal timer callback for adding some logging around calling the registered callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void InternalCallback(object state)
        {
            var handler = this.TimerTick;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        #endregion
    }
}
