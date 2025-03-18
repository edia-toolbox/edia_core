using System;
using System.Text;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Edia {
    /// <summary> Static class to handle file IO </summary>
    public static class FileManager {
        /// <summary>Get all filenames with a certain extension from the applications given subfolder</summary>
        /// <param subFolder="subFolder">Folder to scan</param>
        /// <param extension="extension">filter on specific file type, use * for all</param>
        public static string[] GetAllFilenamesWithExtensionFrom(string subFolder, string extension) {
            string path = GetCorrectPath() + "/" + subFolder + "/";
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

            for (int f = 0; f < info.Length; f++) {
                result[f] = info[f].Name;
            }

            return result;
        }

        /// <summary>Get all filenames with a certain extension from the applications given subfolder</summary>
        /// <param subFolder="subFolder">Folder to scan</param>
        public static string[] GetAllFilenamesFrom(string subFolder) {
            string path = GetCorrectPath() + "/" + subFolder + "/";
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

            for (int f = 0; f < info.Length; f++) {
                result[f] = info[f].Name;
            }

            return result;
        }

        /// <summary> Retrieves all subfolder names from the specified subfolder. </summary>
        /// <param name="subFolder">The parent folder to scan for subfolders.</param>
        /// <returns>An array of subfolder names within the specified folder, or null if the directory does not exist or contains no subfolders.</returns>
        public static string[] GetAllSubFolders(string subFolder) {
            string path = GetCorrectPath() + "/" + subFolder + "/";
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

            for (int f = 0; f < info.Length; f++) {
                result[f] = info[f].Name;
            }

            return result;
        }

        /// <summary> Copies the contents of a source directory to a target directory. </summary>
        /// <param name="sourceDirectory">The path of the directory to copy from.</param>
        /// <param name="targetDirectory">The path of the directory to copy to.</param>
        /// <param name="exclude">Specifies a file extension or pattern that should be excluded during the copy process.</param>
        /// <returns>Returns true if the directory was successfully copied; otherwise, false.</returns>
        public static bool CopyDirectory(string sourceDirectory, string targetDirectory, string exclude) {
            if (!ValidateDirectories(sourceDirectory, targetDirectory, out var diSource, out var diTarget)) return false;

            // Call the copy function and return its result
            bool success = CopyAll(diSource, diTarget, exclude);

            return success;
        }

        /// <summary>
        /// Copies the contents of a source directory to a target directory.
        /// </summary>
        /// <param name="sourceDirectory">The path of the directory to copy from.</param>
        /// <param name="targetDirectory">The path of the directory to copy to.</param>
        /// <param name="exclude">Specifies a file extension or pattern that should be excluded during the copy process.</param>
        /// <param name="cleanCopy">Indicates whether the target directory should be deleted before copying.</param>
        /// <returns>Returns true if the directory was successfully copied; otherwise, false.</returns>
        public static bool CopyDirectory(string sourceDirectory, string targetDirectory, string exclude, bool cleanCopy = false) {
            if (!ValidateDirectories(sourceDirectory, targetDirectory, out var diSource, out var diTarget)) return false;

            if (cleanCopy)
                Directory.Delete(targetDirectory, true);
            
            // Call the copy function and return its result
            bool success = CopyAll(diSource, diTarget, exclude);

            return success;
        }

        /// <summary>
        /// Copies all contents from a source directory to a target directory.
        /// </summary>
        /// <param name="sourceDirectory">The path to the source directory.</param>
        /// <param name="targetDirectory">The path to the target directory.</param>
        /// <returns>True if the copy operation completes successfully; otherwise, false.</returns>
        public static bool CopyDirectory(string sourceDirectory, string targetDirectory) {
            if (!ValidateDirectories(sourceDirectory, targetDirectory, out var diSource, out var diTarget)) return false;

            // Call the copy function and return its result
            bool success = CopyAll(diSource, diTarget, "");

            return success;
        }

        private static bool ValidateDirectories(string sourceDirectory, string targetDirectory, out DirectoryInfo diSource, out DirectoryInfo diTarget) {
            if (string.IsNullOrWhiteSpace(sourceDirectory)) {
                SystemSettings.Instance.Add2Console("Source directory path is null or empty.");
                diSource = diTarget = null;
                return false;
            }

            if (string.IsNullOrWhiteSpace(targetDirectory)) {
                SystemSettings.Instance.Add2Console("Target directory path is null or empty.");
                diSource = diTarget = null;
                return false;
            }

            diSource = new DirectoryInfo(sourceDirectory);
            diTarget = new DirectoryInfo(targetDirectory);

            // Check if the source directory exists
            if (!diSource.Exists) {
                SystemSettings.Instance.Add2Console($"Source directory not found: {sourceDirectory}");
                return false;
            }

            // Ensure the target directory exists
            try {
                if (!diTarget.Exists) {
                    diTarget.Create();
                }
            }
            catch (Exception ex) {
                SystemSettings.Instance.Add2Console($"Failed to create target directory: {targetDirectory}, Error: {ex.Message}");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Copies all files and directories from the source directory to the target directory, excluding files or directories with names containing the specified exclude parameter.
        /// </summary>
        /// <param name="source">The source directory to copy from.</param>
        /// <param name="target">The target directory to copy to.</param>
        /// <param name="exclude">A string used to exclude files or directories with matching names. Pass an empty string to include all.</param>
        /// <returns>True if the copy operation completed successfully, otherwise false.</returns>
        private static bool CopyAll(DirectoryInfo source, DirectoryInfo target, string exclude) {
            try {
                // Ensure the target directory exists
                Directory.CreateDirectory(target.FullName);

                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles()) {
                    if (string.IsNullOrEmpty(exclude) || !fi.Name.Contains(exclude)) {
                        fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
                    }
                }

                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    if (!CopyAll(diSourceSubDir, nextTargetSubDir, exclude)) {
                        return false; // Propagate failure up the recursion stack
                    }
                }

                return true; // Success
            }
            catch (Exception ex) {
                return false; // Failure
            }
        }


        /// <summary> Reads the content of a file located in a specific subfolder of the application's path. </summary>
        /// <param name="_subfolder">The subfolder within the application path where the file is located.</param>
        /// <param name="_fileName">The name of the file to be read.</param>
        /// <returns>The content of the file as a string.</returns>
        public static string ReadStringFromApplicationPathSubfolder(string _subfolder, string _fileName) {
            string path = GetCorrectPath() + "/" + _subfolder + "/" + _fileName;

            return ReadString(path);
        }


        /// <summary> Reads a string from a file located in the application's root path. </summary>
        /// <param name="_fileName">The name of the file to read from the application's root path.</param>
        /// <returns>A string containing the contents of the specified file.</returns>
        public static string ReadStringFromApplicationPath(string _fileName) {
            string path = GetCorrectPath() + "/" + _fileName;

            return ReadString(path);
        }

        /// <summary> Determines if a file exists at the specified application path. </summary>
        /// <param name="_fileName">The name of the file to check for existence.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public static bool FileExists(string _fileName) {
            string path = GetCorrectPath() + "/" + _fileName;
            return File.Exists(path);
        }

        /// <summary>Tries to read the given textbased filename.</summary>
        /// <param _fileName="_fileName"></param>
        /// <returns>Content of the file, or 'ERROR' when failed</returns>
        public static string ReadString(string _fileName) {
            StreamReader reader = new StreamReader(_fileName);
            string result;

            try {
                result = reader.ReadToEnd();
            }
            catch (System.Exception) {
                result = "ERROR";
                throw;
            }

            reader.Close();
            return result;
        }

        /// <summary>Copies a specified file from a source path to a destination path.</summary>
        /// <param name="_sourcePath">The directory in the source from which the file will be copied.</param>
        /// <param name="_filename">The name of the file to copy.</param>
        /// <param name="_destinationPath">The directory in the destination where the file will be copied to.</param>
        public static void CopyFileTo(string _sourcePath, string _filename, string _destinationPath) {
            string pathfile = GetCorrectPath() + "/" + _sourcePath + "/" + _filename;

            File.Copy(pathfile, GetCorrectPath() + "/" + _destinationPath + "/" + _filename, true);
        }

        /// <summary>Saves a text file to given filename and containts given data</summary>
        /// <param _fileName="_fileName">Name of the file</param>
        /// <param _data="_data">The data that needs to be written</param>
        /// <param _overwrite="_overwrite">Overwrite if filename exists.</param>
        public static void WriteString(string _fileName, string _data, bool _overwrite) {
            string path = GetCorrectPath() + "/" + _fileName;

            StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8, 65536);

            writer.WriteLine(_data);

            // Cleanup
            writer.Flush();
            writer.Close();
        }

        /// <summary>Creates a folder in the application data directory</summary>
        /// <param _fileName="_fileName">Name of the file</param>
        public static void CreateFolder(string _folderName) {
            string path = GetCorrectPath() + "/" + _folderName;

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
                Debug.Log(string.Format("<color=#00FFFF>[eDIA]</color>Created Folder: {0} ", GetCorrectPath() + "/" + _folderName));
            }
        }

        /// <summary>Determines correct path depending on where the application is running on/in.</summary>
        /// <returns>Path to data on specific platform</returns>
        public static string GetCorrectPath() {
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