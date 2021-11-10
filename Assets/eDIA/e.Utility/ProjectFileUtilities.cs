using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace eDIA.Utilities {

	[System.Serializable]
	public class ChangeLogEntry {
		public string description;
		public string type;
		public string version;
		public string date;
		public string dev;
	}

	public static class ProjectFileUtilities {

		public static List<ChangeLogEntry> logSRC;
		public static List<ChangeLogEntry> logOrderedByVersion;
		public static List<ChangeLogEntry> logSingleVersion;
		public static List<ChangeLogEntry> logOrderedOnType;

		public static string changeLogString = string.Empty;

		/// <summary>Converts CSV file to sorted CHANGELOG.md file</summary>
		public static void UpdateChangelog () {

			logSRC 			= new List<ChangeLogEntry> ();
			logOrderedByVersion 	= new List<ChangeLogEntry> ();

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

			// CHANGELOG header
			changeLogString = string.Empty;
			changeLogString += "# Changelog\n\nAll notable changes to this project will be documented in this file.\nThe format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),\nand this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).\n\n";

			// Add sections per version
			while (logOrderedByVersion.Count > 0) 
				GenerateSingleVersionEntry ();

			// Write result to file
			FileManager.WriteString ("/../CHANGELOG.md", changeLogString, true);
			
			Debug.Log ("Generated new CHANGELOG.md");
		}

		/// <summary>Loops through the sorted array and filters one version sorted by ChangeType and Date</summary>
		public static void GenerateSingleVersionEntry () {

			if (logOrderedByVersion.Count == 0)
				return;
			
			logSingleVersion = new List<ChangeLogEntry> ();
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
			logOrderedOnType = new List<ChangeLogEntry> ();
			logOrderedOnType = logSingleVersion.OrderBy (c => c.type).ThenByDescending (c => c.date).ToList ();

			// Add ChangeLog section for this version
			changeLogString += "\n## [" + logOrderedOnType[0].version + "] - " + logOrderedOnType[0].date;

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
	}
}