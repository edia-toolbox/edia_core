using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace eDIA {

	[System.Serializable]
	public class ChangeLogEntry {
		public string description;
		public string type;
		public string version;
		public string date;
		public string dev;
	}

	public class ProjectFileUtilities : MonoBehaviour {

		public List<ChangeLogEntry> logSRC = new List<ChangeLogEntry> ();
		public List<ChangeLogEntry> logOrderedByVersion = new List<ChangeLogEntry> ();
		public List<ChangeLogEntry> logSingleVersion = new List<ChangeLogEntry> ();
		public List<ChangeLogEntry> logOrderedOnType = new List<ChangeLogEntry> ();

		public string changeLogString = string.Empty;

		public void UpdateChangelog () {

			string CSVlog = FileManager.ReadString ("ChangeLog.csv");
			string[] rows = CSVlog.Split ('\n');

			foreach (string s in rows) {
				string[] values = s.Split (',');

				//! Add only pick first 5 values as the one behind is the hidden 'created date' in falsch format.
				ChangeLogEntry newEntry = new ChangeLogEntry ();
				newEntry.description = values[0];
				newEntry.type = values[1];
				newEntry.version = values[2];
				newEntry.date = values[3];
				newEntry.dev = values[4];

				logSRC.Add (newEntry);
			}

			// Remove the first one, as those are the headers
			logSRC.RemoveAt (0);

			// Sort on versions
			logOrderedByVersion = logSRC.OrderByDescending (c => c.version).ToList ();

			// header
			changeLogString = string.Empty;
			changeLogString += "# Changelog\n\nAll notable changes to this project will be documented in this file.\nThe format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),\nand this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).\n\n";

			Debug.Log ("logOrderedByVersion.Count " + logOrderedByVersion.Count);
			
			while (logOrderedByVersion.Count > 0) 
				GenerateSingleVersionEntry ();

			Debug.Log ("Result:\n" + changeLogString);

			FileManager.WriteString ("/../CHANGELOG.md", changeLogString);
		}

		/// <summary>Loops through the sorted array and filters one version sorted by ChangeType and Date</summary>
		public void GenerateSingleVersionEntry () {

			if (logOrderedByVersion.Count == 0)
				return;

			string versioncheck = logOrderedByVersion[0].version;
			bool CheckForVersion = true;

			while (CheckForVersion) {

				foreach (ChangeLogEntry c in logOrderedByVersion) {
					if (c.version == versioncheck) {
						logSingleVersion.Add (c);
						logOrderedByVersion.Remove (c);
						break;
					}

					CheckForVersion = false;
				}
				
				if (logOrderedByVersion.Count == 0 || !CheckForVersion)  // array depleted or last was found, stop while loop.
					break;
			}

			// Sort the list on type and dates
			logOrderedOnType = logSingleVersion.OrderBy (c => c.type).ThenByDescending (c => c.date).ToList ();

			// Clean previous list if not empty
			logSingleVersion.Clear ();

			// Add ChangeLog section for this version
			changeLogString += "\n## [" + logOrderedOnType[0].version + "] - " + logOrderedOnType[0].date + "\n";

			string currentType = string.Empty;

			foreach (ChangeLogEntry c in logOrderedOnType) {
				if (c.type != currentType) { // new changetype header
					changeLogString += "\n### " + c.type + "\n";
					currentType = c.type;
				}

				changeLogString += "- " + c.description + "\n";
			}

			logOrderedOnType.Clear();
		}

		void Start () {
			UpdateChangelog ();
		}

	}
}