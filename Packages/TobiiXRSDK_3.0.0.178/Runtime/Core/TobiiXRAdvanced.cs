using System.Collections.Generic;
using Unity.Jobs;

namespace Tobii.XR
{
    public class TobiiXRAdvanced
    {
        private readonly TobiiProvider _provider;

        public TobiiXRAdvanced(TobiiProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Calling this method will update <see cref="QueuedData"/> and <see cref="LatestData"/> if there is new
        /// data available from the eye tracker.
        /// </summary>
        /// <remarks>
        /// TobiiXR automatically checks for new eye tracking data at the beginning of every frame so calling
        /// this method is only necessary if you want to poll for more recent data late in your frame.
        /// </remarks>
        public void ProcessNewData()
        {
            _provider.Tick();
        }

        /// <summary>
        /// Uses Unity's job system to schedule a timesync operation. Use <see cref="FinishTimesyncJob"/> to
        /// get the result of the timesync operation.
        /// </summary>
        /// <remarks>
        /// The resulting <see cref="TobiiXR_AdvancedTimesyncData"/> will also be used internally to improve the
        /// accuracy for <see cref="TobiiXR_AdvancedEyeTrackingData.SystemTimestamp"/>. Calling this function
        /// once per minute will give a good accuracy.
        /// See https://vr.tobii.com/sdk/develop/general/time-synchronization/ for more details on
        /// time synchronization.
        /// </remarks>
        /// <returns>A handle that can be polled to check if the timesync operation has completed.</returns>
        public JobHandle StartTimesyncJob()
        {
            return _provider.StartTimesyncJob();
        }

        /// <summary>
        /// Finishes the timesync operation and returns timesync data. If the JobHandle received from
        /// <see cref="StartTimesyncJob"/> has not signalled IsCompleted, this function will block.
        /// </summary>
        /// <returns>Timesync data if the operation succeeded. Otherwise null.</returns>
        public TobiiXR_AdvancedTimesyncData? FinishTimesyncJob()
        {
            return _provider.FinishTimesyncJob();
        }

        /// <summary>
        /// Gets a timestamp from the same system clock that is being used to compute <see cref="TobiiXR_AdvancedEyeTrackingData.SystemTimestamp"/>.
        /// The timestamp will be monotonic on all host systems that have a monotonic clock. 
        /// </summary>
        /// <returns>System timestamp in microseconds</returns>
        public long GetSystemTimestamp()
        {
            return _provider.GetSystemTimestamp();
        }

        /// <summary>
        /// A circular FIFO queue containing advanced eye tracking data read from the eye tracker.
        /// It is optional to dequeue data from the queue. If the size of the queue reaches 30,
        /// the top element will be automatically dequeued before new data is enqueued.
        /// Unless otherwise specified, the signals are reported in <see cref="TobiiXR_TrackingSpace.Local"/>
        /// coordinate system.
        /// </summary>
        public Queue<TobiiXR_AdvancedEyeTrackingData> QueuedData
        {
            get { return _provider.AdvancedData; }
        }

        /// <summary>
        /// The latest advanced data read from the eye tracker. This function will not incur a
        /// new read from the eye tracker and will still return latest data even if <see cref="QueuedData"/>
        /// has been emptied.
        /// Unless otherwise specified, the signals are reported in <see cref="TobiiXR_TrackingSpace.Local"/>
        /// coordinate system. 
        /// </summary>
        public TobiiXR_AdvancedEyeTrackingData LatestData
        {
            get { return _provider.AdvancedEyeTrackingData; }
        }

        /// <summary>
        /// Query the eye tracker for metadata. This function can block for several milliseconds.
        /// </summary>
        /// <returns>Eye tracker metadata</returns>
        public TobiiXR_EyeTrackerMetadata GetMetadata()
        {
            return _provider.GetMetadata();
        }
    }
}