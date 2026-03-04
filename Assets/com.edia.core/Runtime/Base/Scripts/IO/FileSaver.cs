using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Edia.Data;
using UnityEngine;

namespace Edia.IO {

    /// <summary>
    /// Manages file I/O in a separate thread to avoid hitches.
    /// Non-MonoBehaviour — lifecycle managed by Experiment.
    /// </summary>
    public class FileSaver {

        public bool sortDataIntoFolders = true;
        public bool verboseDebug = false;

        private string storagePath;
        private BlockingQueue<Action> bq = new BlockingQueue<Action>();
        private Thread parallelThread;
        private bool quitting = false;

        public bool IsActive { get { return parallelThread != null && parallelThread.IsAlive; } }

        public FileSaver(string storagePath, bool sortDataIntoFolders = true) {
            this.storagePath = storagePath;
            this.sortDataIntoFolders = sortDataIntoFolders;
        }

        public void SetUp() {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            quitting = false;
            Directory.CreateDirectory(storagePath);

            if (!IsActive) {
                parallelThread = new Thread(Worker);
                parallelThread.CurrentCulture = new CultureInfo("en-US");
                parallelThread.Start();
            }
            else {
                Debug.LogWarning("[FileSaver] Parallel thread is still active!");
            }
        }

        public void ManageInWorker(Action action) {
            if (quitting)
                throw new InvalidOperationException("Cannot add action to FileSaver, is currently quitting.");
            bq.Enqueue(action);
        }

        void Worker() {
            foreach (var action in bq) {
                try {
                    action.Invoke();
                }
                catch (ThreadAbortException) {
                    break;
                }
                catch (IOException e) {
                    Debug.LogError(string.Format("[FileSaver] Error, file may be in use! Exception: {0}", e));
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }

                if (quitting && bq.NumItems() == 0)
                    break;
            }
        }

        public bool CheckIfRiskOfOverwrite(string experiment, string ppid, int sessionNum) {
            string directory = Path.Combine(storagePath, experiment, ppid, SessionNumToName(sessionNum));
            return Directory.Exists(directory);
        }

        public string HandleDataTable(DataTable table, string experiment, string ppid, int sessionNum, string dataName, DataType dataType, int optionalTrialNum = 0) {
            string ext = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == DataLevel.PerTrial)
                dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string[] lines = table.GetCSVLines();

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults)
                directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.csv", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);

            ManageInWorker(() => { File.WriteAllLines(savePath, lines); });
            return GetRelativePath(storagePath, savePath);
        }

        public string HandleJSONSerializableObject(string jsonText, string experiment, string ppid, int sessionNum, string dataName, DataType dataType, int optionalTrialNum = 0) {
            string ext = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == DataLevel.PerTrial)
                dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults)
                directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.json", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);

            ManageInWorker(() => { File.WriteAllText(savePath, jsonText); });
            return GetRelativePath(storagePath, savePath);
        }

        public string HandleText(string text, string experiment, string ppid, int sessionNum, string dataName, DataType dataType, int optionalTrialNum = 0) {
            string ext = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == DataLevel.PerTrial)
                dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults)
                directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);

            ManageInWorker(() => { File.WriteAllText(savePath, text); });
            return GetRelativePath(storagePath, savePath);
        }

        public string HandleBytes(byte[] bytes, string experiment, string ppid, int sessionNum, string dataName, DataType dataType, int optionalTrialNum = 0) {
            string ext = Path.GetExtension(dataName);
            dataName = Path.GetFileNameWithoutExtension(dataName);

            if (dataType.GetDataLevel() == DataLevel.PerTrial)
                dataName = string.Format("{0}_T{1:000}", dataName, optionalTrialNum);

            string directory = GetSessionPath(experiment, ppid, sessionNum);
            if (sortDataIntoFolders && dataType != DataType.TrialResults)
                directory = Path.Combine(directory, dataType.GetFolderName());
            Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(ext) ? string.Format("{0}.txt", dataName) : string.Format("{0}{1}", dataName, ext);
            string savePath = Path.Combine(directory, name);

            ManageInWorker(() => { File.WriteAllBytes(savePath, bytes); });
            return GetRelativePath(storagePath, savePath);
        }

        public string GetSessionPath(string experiment, string ppid, int sessionNum) {
            if (!Directory.Exists(storagePath)) {
                string fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "EDIA_Data");
                Directory.CreateDirectory(fallback);
                Debug.LogError(string.Format("[FileSaver] Storage location ({0}) does not exist! Defaulting to {1}.", storagePath, fallback));
                return Path.Combine(fallback, experiment, ppid, SessionNumToName(sessionNum));
            }
            return Path.Combine(storagePath, experiment, ppid, SessionNumToName(sessionNum));
        }

        public void CleanUp() {
            quitting = true;
            bq.Enqueue(() => { }); // ensures bq breaks from foreach loop
            parallelThread?.Join();
        }

        public static string SessionNumToName(int num) {
            return string.Format("S{0:000}", num);
        }

        public static string GetRelativePath(string relativeToDirectory, string path) {
            relativeToDirectory = Path.GetFullPath(relativeToDirectory);
            if (!relativeToDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                relativeToDirectory += Path.DirectorySeparatorChar;

            path = Path.GetFullPath(path);
            Uri path1 = new Uri(relativeToDirectory);
            Uri path2 = new Uri(path);
            Uri diff = path1.MakeRelativeUri(path2);
            return Uri.UnescapeDataString(diff.OriginalString);
        }
    }

    /// <summary>
    /// Data type categories for file saving.
    /// </summary>
    public enum DataType {
        TrialResults, SessionLog, Settings, ParticipantDetails, Trackers, SummaryStatistics, OtherTrialData, OtherSessionData
    }

    public enum DataLevel {
        PerTrial, PerSession
    }

    public static class DataTypeExtensions {

        public static string GetFolderName(this DataType dt) {
            switch (dt) {
                case DataType.TrialResults:
                    return "";
                case DataType.SessionLog:
                case DataType.Settings:
                case DataType.ParticipantDetails:
                case DataType.SummaryStatistics:
                    return "session_info";
                case DataType.Trackers:
                    return "trackers";
                default:
                    return "other";
            }
        }

        static readonly Dictionary<DataType, DataLevel> typeLevelMapping = new Dictionary<DataType, DataLevel> {
            { DataType.TrialResults, DataLevel.PerSession },
            { DataType.SessionLog, DataLevel.PerSession },
            { DataType.Settings, DataLevel.PerSession },
            { DataType.ParticipantDetails, DataLevel.PerSession },
            { DataType.SummaryStatistics, DataLevel.PerSession },
            { DataType.OtherSessionData, DataLevel.PerSession },
            { DataType.Trackers, DataLevel.PerTrial },
            { DataType.OtherTrialData, DataLevel.PerTrial }
        };

        public static DataLevel GetDataLevel(this DataType dt) {
            return typeLevelMapping[dt];
        }
    }
}
