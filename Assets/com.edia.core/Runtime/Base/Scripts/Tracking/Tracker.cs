using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Edia.Data;

namespace Edia.Tracking {

    /// <summary>
    /// Abstract base class for frame-by-frame data tracking.
    /// Inherit from this to create custom tracking behaviour.
    /// </summary>
    public abstract class Tracker : MonoBehaviour {

        private static string[] baseHeaders = new string[] { "time" };
        private TrackerState currentState = TrackerState.Stopped;

        /// <summary>
        /// Name of the object used in saving.
        /// </summary>
        public string objectName;

        /// <summary>
        /// Description of the type of measurement this tracker performs.
        /// </summary>
        public abstract string MeasurementDescriptor { get; }

        /// <summary>
        /// Custom column headers for tracked data.
        /// </summary>
        public abstract IEnumerable<string> CustomHeader { get; }

        /// <summary>
        /// A name used when saving the data from this tracker.
        /// </summary>
        public string DataName {
            get {
                Debug.AssertFormat(MeasurementDescriptor.Length > 0,
                    "No measurement descriptor has been specified for this Tracker!");
                return string.Join("_", new string[] { objectName, MeasurementDescriptor });
            }
        }

        public bool Recording { get { return currentState == TrackerState.Recording; } }

        public DataTable Data { get; private set; } = new DataTable();

        public TrackerState CurrentState { get => currentState; }

        [Tooltip("When the measurements should be taken.")]
        public TrackerUpdateType updateType = TrackerUpdateType.LateUpdate;

        void Reset() {
            objectName = gameObject.name.Replace(" ", "_").ToLower();
        }

        void LateUpdate() {
            if (Recording && updateType == TrackerUpdateType.LateUpdate) RecordRow();
        }

        void FixedUpdate() {
            if (Recording && updateType == TrackerUpdateType.FixedUpdate) RecordRow();
        }

        public void RecordRow() {
            if (!Recording) throw new System.InvalidOperationException(
                "Tracker measurements cannot be taken when not recording!");

            DataRow newRow = GetCurrentValues();
            newRow.Add(("time", Time.time));
            Data.AddCompleteRow(newRow);
        }

        public void StartRecording() {
            if (currentState == TrackerState.Recording) {
                Debug.LogWarning($"Start command received for tracker in state: '{TrackerState.Recording}'." +
                    " This will dump existing data! " +
                    $"If you want to restart a paused tracker, use '{nameof(ResumeRecording)}()' instead.");
            }
            var header = baseHeaders.Concat(CustomHeader);
            Data = new DataTable(header.ToArray());
            currentState = TrackerState.Recording;
        }

        public void StopRecording() {
            if (currentState != TrackerState.Recording) {
                Debug.LogWarning($"Stop command received for tracker in state: '{currentState}'." +
                    $" This should only be called when tracker is in state '{TrackerState.Recording}'");
            }
            currentState = TrackerState.Stopped;
        }

        public void PauseRecording() {
            if (currentState != TrackerState.Recording) {
                Debug.LogWarning($"Pause command received for tracker in state: '{currentState}'." +
                    $"This should only be called when tracker is in state '{TrackerState.Recording}'");
            }
            currentState = TrackerState.Paused;
        }

        public void ResumeRecording() {
            if (currentState != TrackerState.Paused) {
                Debug.LogWarning($"Resume command received for tracker in state: '{currentState}'." +
                    $"This should only be called when tracker is in state '{TrackerState.Paused}'");
            }
            currentState = TrackerState.Recording;
        }

        /// <summary>
        /// Acquire values for this frame. Must return values for ALL custom columns.
        /// </summary>
        protected abstract DataRow GetCurrentValues();
    }

    public enum TrackerUpdateType {
        LateUpdate, FixedUpdate, Manual
    }

    public enum TrackerState {
        Recording, Paused, Stopped
    }
}
