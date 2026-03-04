using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using Edia.Data;
using Edia.IO;
using Edia.Tracking;

namespace Edia {

    /// <summary>
    /// The base unit of experiments. A Trial is a singular attempt at a task by a participant.
    /// </summary>
    [Serializable]
    public class Trial {

        /// <summary>
        /// Returns non-zero indexed trial number based on position across all blocks.
        /// </summary>
        public int number { get { return Experiment.Instance.Trials.ToList().IndexOf(this) + 1; } }

        /// <summary>
        /// Returns non-zero indexed trial number within its block.
        /// </summary>
        public int numberInBlock { get { return block.trials.IndexOf(this) + 1; } }

        public TrialStatus status = TrialStatus.NotDone;

        public Block block;
        float startTime, endTime;

        /// <summary>
        /// Trial settings. Cascades up to block settings, then experiment settings.
        /// </summary>
        public Settings settings { get; protected set; }

        /// <summary>
        /// Dictionary of results for this trial.
        /// </summary>
        public ResultsDictionary result;

        // Worker thread for async file I/O
        private static BlockingQueue<Action> blockingQueue = new BlockingQueue<Action>();
        private static Task workerTask;
        private static bool quitting = false;

        internal Trial(Block trialBlock) {
            settings = Settings.empty;
            block = trialBlock;
            settings.SetParent(block.settings);
        }

        /// <summary>
        /// Begins the trial: updates current trial/block numbers, starts timer, begins tracker recording.
        /// </summary>
        public void Begin() {
            var exp = Experiment.Instance;

            if (!exp.hasInitialised)
                throw new InvalidOperationException("Cannot begin trial, experiment session has not been started yet.");

            if (exp.InTrial) exp.CurrentTrial.End();

            exp.currentTrialNum = number;
            exp.currentBlockNum = block.number;

            status = TrialStatus.InProgress;
            startTime = Time.time;
            result = new ResultsDictionary(exp.Headers, true);

            result["experiment"] = exp.experimentName;
            result["ppid"] = exp.ppid;
            result["session_num"] = exp.sessionNumber;
            result["trial_num"] = number;
            result["block_num"] = block.number;
            result["trial_num_in_block"] = numberInBlock;
            result["start_time"] = startTime;

            foreach (Tracker tracker in exp.trackedObjects) {
                try {
                    tracker.StartRecording();
                }
                catch (NullReferenceException) {
                    Debug.LogWarning("An item in the tracked objects list is empty (null)!");
                }
            }
        }

        /// <summary>
        /// Ends the trial: queues saving results, stops and saves tracker data.
        /// </summary>
        public void End() {
            status = TrialStatus.Done;
            endTime = Time.time;
            result["end_time"] = endTime;

            SaveData();
        }

        /// <summary>
        /// Saves a DataTable associated with this trial.
        /// </summary>
        public void SaveDataTable(DataTable table, string dataName, DataType dataType = DataType.OtherTrialData) {
            var exp = Experiment.Instance;
            string location = exp.fileSaver.HandleDataTable(table, exp.experimentName, exp.ppid, exp.sessionNumber, dataName, dataType, number);
            result[string.Format("{0}_location_0", dataName)] = location.Replace("\\", "/");
        }

        /// <summary>
        /// Saves a JSON string associated with this trial.
        /// </summary>
        public void SaveJSONSerializableObject(string jsonText, string dataName, DataType dataType = DataType.OtherTrialData) {
            var exp = Experiment.Instance;
            string location = exp.fileSaver.HandleJSONSerializableObject(jsonText, exp.experimentName, exp.ppid, exp.sessionNumber, dataName, dataType, number);
            result[string.Format("{0}_location_0", dataName)] = location.Replace("\\", "/");
        }

        /// <summary>
        /// Saves a text string associated with this trial.
        /// </summary>
        public void SaveText(string text, string dataName, DataType dataType = DataType.OtherTrialData) {
            var exp = Experiment.Instance;
            string location = exp.fileSaver.HandleText(text, exp.experimentName, exp.ppid, exp.sessionNumber, dataName, dataType, number);
            result[string.Format("{0}_location_0", dataName)] = location.Replace("\\", "/");
        }

        /// <summary>
        /// Saves bytes associated with this trial.
        /// </summary>
        public void SaveBytes(byte[] bytes, string dataName, DataType dataType = DataType.OtherTrialData) {
            var exp = Experiment.Instance;
            string location = exp.fileSaver.HandleBytes(bytes, exp.experimentName, exp.ppid, exp.sessionNumber, dataName, dataType, number);
            result[string.Format("{0}_location_0", dataName)] = location.Replace("\\", "/");
        }

        private void SaveData() {
            var exp = Experiment.Instance;

            // check no duplicate trackers
            List<string> duplicateTrackers = exp.trackedObjects.Where(tracker => tracker != null)
                .GroupBy(tracker => tracker.DataName)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            if (duplicateTrackers.Any())
                throw new InvalidOperationException(string.Format(
                    "Two or more trackers have the same DataName, please make them unique: {0}",
                    string.Join(",", duplicateTrackers)));

            // save tracked objects
            foreach (Tracker tracker in exp.trackedObjects) {
                try {
                    tracker.StopRecording();
                    if (tracker.Data.CountRows() > 0) {
                        DataTable table = tracker.Data;
                        string name = tracker.DataName;
                        ManageInWorker(() => {
                            SaveDataTable(table, name, dataType: DataType.Trackers);
                        });
                    }
                }
                catch (NullReferenceException) {
                    Debug.LogWarning("An item in the tracked objects list is empty (null)!");
                }
            }

            // log settings to trial results
            foreach (string s in exp.settingsToLog) {
                var settingObject = settings.GetObject(s, string.Empty);

                if (settingObject is System.Collections.IList list)
                    result[s] = string.Join(";", list.OfType<object>().Select(o => o?.ToString() ?? ""));
                else
                    result[s] = settingObject?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Adds a new command to a queue which is executed in a separate worker thread.
        /// Warning: The Unity Engine API is not thread safe — do not use Unity commands here.
        /// </summary>
        public static void ManageInWorker(Action action) {
            if (workerTask == null) {
                workerTask = Task.Run(Worker);
                quitting = false;
            }
            blockingQueue.Enqueue(action);
        }

        private static void Worker() {
            foreach (var action in blockingQueue) {
                try {
                    action.Invoke();
                }
                catch (ThreadAbortException) {
                    break;
                }
                catch (IOException e) {
                    Debug.LogError(string.Format("Error, file may be in use! Exception: {0}", e));
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }

                if (quitting && blockingQueue.NumItems() == 0)
                    break;
            }
        }

        /// <summary>
        /// Wait for all tasks scheduled through ManageInWorker to complete.
        /// </summary>
        public static void WaitForTasks() {
            Debug.Log("[Trial] Waiting for tasks to finish");
            quitting = true;
            blockingQueue.Enqueue(() => { }); // ensures bq breaks from foreach loop
            workerTask?.Wait();
            Debug.Log("[Trial] Tasks finished");
        }
    }

    public enum TrialStatus {
        NotDone,
        InProgress,
        Done
    }

    /// <summary>
    /// Exception thrown in cases where we try to access a trial that does not exist.
    /// </summary>
    public class NoSuchTrialException : Exception {
        public NoSuchTrialException() { }
        public NoSuchTrialException(string message) : base(message) { }
        public NoSuchTrialException(string message, Exception inner) : base(message, inner) { }
    }
}
