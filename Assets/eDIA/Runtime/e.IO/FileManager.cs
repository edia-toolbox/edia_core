using System.Text;
using System.Collections;
using System.IO;
using UnityEngine;

namespace eDIA {

	/// <summary>
	/// Static class to handle file IO
	/// </summary>
	public static class FileManager {

		public static string ReadStringFromApplicationPath (string _fileName) {
			string path =  CorrectPath() + "/" + _fileName;

			return ReadString(path);
		}

		/// <summary>Tries to read the given textbased filename.</summary>
		/// <param _fileName="_fileName"></param>
		/// <returns>Content of the file, or 'ERROR' when failed</returns>
		public static string ReadString (string _fileName) {

			StreamReader reader = new StreamReader (_fileName);
			string result;

			try {
				result = reader.ReadToEnd ();
			}
			catch (System.Exception) {
			    result = "ERROR";
			    throw;
			}

			reader.Close ();
			return result;
		}

		/// <summary>Saves a text file to given filename and containts given data</summary>
		/// <param _fileName="_fileName">Name of the file</param>
		/// <param _data="_data">The data that needs to be written</param>
		/// <param _overwrite="_overwrite">Overwrite if filename exists.</param>
		public static void WriteString (string _fileName, string _data, bool _overwrite) {

			string path =  CorrectPath() + "/" + _fileName;
			
			StreamWriter writer = new StreamWriter ( path, false, Encoding.UTF8, 65536);

			writer.WriteLine (_data);

			// Cleanup
			writer.Flush ();
			writer.Close ();
		}

		/// <summary>Creates a folder in the application data directory</summary>
		/// <param _fileName="_fileName">Name of the file</param>
		public static void CreateFolder (string _folderName) {

			string path = CorrectPath() + "/" + _folderName;

			if (!Directory.Exists (path)) {
				Directory.CreateDirectory (path);
			}
		}

		/// <summary>Determines correct path depending on where the application is running on/in.</summary>
		/// <returns>Path to data on specific platform</returns>
		static string CorrectPath () {
			string platformSpecificPath;

			#if UNITY_EDITOR
				platformSpecificPath = Application.dataPath;
			#elif UNITY_ANDROID
				platformSpecificPath = Application.persistentDataPath;
			#else
				platformSpecificPath = Application.dataPath;
			#endif
			
			return platformSpecificPath;
		}
	}

}