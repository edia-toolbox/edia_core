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
		
		public enum CHANGETYPE { Added, Changed, Deprecated, Removed, Fixed, Security };

		public List<ChangeLogEntry> logEntries = new List<ChangeLogEntry>();
		
		public void UpdateChangelog () {

			// read in CSV file, if not found > alert
			// first line is headers



			string CSVlog = FileManager.ReadString ("ChangeLog.csv");

			string[] rows = CSVlog.Split ('\n');

			foreach (string s in rows) {
				string[] values = s.Split(',');
				
				ChangeLogEntry newEntry 	= new ChangeLogEntry();
				newEntry.description 		= values[0];
				newEntry.type 			= values[1];
				newEntry.version 			= values[2];
				newEntry.date 			= values[3];
				newEntry.dev = 			values[4];

				logEntries.Add(newEntry);

				Debug.Log (s);
			}

			// Remove first entry
			logEntries.RemoveAt(0);




		}

		void Start() {
			UpdateChangelog();
		}
	}
}