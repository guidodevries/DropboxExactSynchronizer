using System;

namespace DropboxSynchronizer.Interfaces
{
    /// <summary>
    /// Interface of the timer.
    /// </summary>
    public interface ITimer
    {
        /// <summary>
        /// Occurs when the timer ticks.
        /// </summary>
        event EventHandler TimerTick;

        /// <summary>
        /// Starts the timer with the specified interval. This timer starts immediately.
        /// </summary>
        /// <param name="interval">The newInterval in milliseconds.</param>
        void Start(long interval);

        /// <summary>
        /// Stops the timer.
        /// </summary>
        void Stop();
    }
}
