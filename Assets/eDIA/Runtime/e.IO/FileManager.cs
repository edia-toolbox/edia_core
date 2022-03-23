using System.Text;
using System.Collections;
using System.IO;
using UnityEngine;

namespace eDIA {

	/// <summary>
	/// Static class to handle file IO
	/// </summary>
	public static class FileManager {

		/// <summary>Get all filenames with a certain extension from the applications given subfolder</summary>
		/// <param subFolder="subFolder">Folder to scan</param>
		/// <param extension="extension">filter on specific file type, use * for all</param>
		public static string[] GetAllFilenamesWithExtensionFrom (string subFolder, string extension) {
			
			string path =  CorrectPath() + "/" + subFolder + "/";
			DirectoryInfo dir = new DirectoryInfo(path);
			
			if (!dir.Exists) {
				Debug.Log("Directory " + subFolder + " does not exist");
				return null;
			}

			FileInfo[] info = dir.GetFiles("*." + extension);
			
			if (info.Length == 0) {
				Debug.Log("No files found with " + extension + " in " + subFolder);
				return null;
			}

			string[] result = new string[info.Length];

			for (int f=0; f<info.Length; f++) {
				result[f] = info[f].Name;
			}

			return result;
		}

		/// <summary>Get all filenames with a certain extension from the applications given subfolder</summary>
		/// <param subFolder="subFolder">Folder to scan</param>
		public static string[] GetAllFilenamesFrom (string subFolder) {
			
			string path =  CorrectPath() + "/" + subFolder + "/";
			DirectoryInfo dir = new DirectoryInfo(path);
			
			if (!dir.Exists) {
				Debug.Log("Directory " + subFolder + " does not exist");
				return null;
			}

			FileInfo[] info = dir.GetFiles("*.*");
			
			if (info.Length == 0) {
				Debug.Log("No files found in " + subFolder);
				return null;
			}

			string[] result = new string[info.Length];

			for (int f=0; f<info.Length; f++) {
				result[f] = info[f].Name;
			}

			return result;
		}

		public static string[] GetAllSubFolders (string subFolder) {
			
			string path =  CorrectPath() + "/" + subFolder + "/";
			DirectoryInfo dir = new DirectoryInfo(path);
			
			if (!dir.Exists) {
				Debug.Log("Directory " + subFolder + " does not exist");
				return null;
			}

			DirectoryInfo[] info = dir.GetDirectories();
			
			if (info.Length == 0) {
				Debug.Log("No task folders found in " + subFolder);
				return null;
			}

			string[] result = new string[info.Length];

			for (int f=0; f<info.Length; f++) {
				result[f] = info[f].Name;
			}

			return result;
		}

		/// <summary>
		/// Copy directory
		/// </summary>
		/// <param name="sourceDirectory"></param>
		/// <param name="targetDirectory"></param>
		public static void CopyDirectory(string sourceDirectory, string targetDirectory, string exclude)
		{
			var diSource = new DirectoryInfo(sourceDirectory);
			var diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget, exclude);
		}

		public static void CopyDirectory(string sourceDirectory, string targetDirectory)
		{
			var diSource = new DirectoryInfo(sourceDirectory);
			var diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget, "");
		}


		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, string exclude)
		{
			Directory.CreateDirectory(target.FullName);

			// Copy each file into the new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				// Debug.Log((@"Copying {0}\{1}", target.FullName, fi.Name));
				if (exclude.Length == 0 || (!fi.Name.Contains(exclude)))
					fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
				target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir,exclude);
			}
		}

		
		public static string ReadStringFromApplicationPathSubfolder (string _subfolder, string _fileName) {
			string path =  CorrectPath() + "/" + _subfolder + "/" + _fileName;

			return ReadString(path);
		}


		public static string ReadStringFromApplicationPath (string _fileName) {
			string path =  CorrectPath() + "/" + _fileName;

			return ReadString(path);
		}

		public static bool FileExists (string _fileName) {
			string path =  CorrectPath() + "/" + _fileName;
			return File.Exists(path);
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

		public static void CopyFileTo(string _sourcePath, string _filename, string _destinationPath) {
			
			string pathfile =  CorrectPath() + "/" + _sourcePath + "/" + _filename;

			File.Copy(pathfile, CorrectPath() + "/" + _destinationPath + "/" + _filename, true);
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
				Debug.Log("Folder created");
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
				platformSpecificPath = Application.dataPath + "/../";
			#endif
			
			return platformSpecificPath;
		}
	}

}