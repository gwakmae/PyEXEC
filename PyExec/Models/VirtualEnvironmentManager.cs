// PyExec/Models/VirtualEnvironmentManager.cs
using PyExec.Helpers; // For FindActualVenvPath if moved here
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using WF = System.Windows.Forms; // FolderBrowserDialog

namespace PyExec.Models
{
    public class VirtualEnvironmentManager
    {
        private const string DEFAULT_ENV_FILE = "default_venv_root.json"; // Store root path
        public string DefaultVirtualEnvRoot { get; set; } = ""; // Parent folder (e.g., C:\Projects)

        public VirtualEnvironmentManager()
        {
            LoadDefaultVirtualEnvRoot(); // Load on creation
        }

        // Load the Default ROOT folder path
        public void LoadDefaultVirtualEnvRoot()
        {
            if (File.Exists(DEFAULT_ENV_FILE))
            {
                try
                {
                    string json = File.ReadAllText(DEFAULT_ENV_FILE);
                    string? deserializedFolder = JsonSerializer.Deserialize<string>(json);
                    if (!string.IsNullOrEmpty(deserializedFolder) && Directory.Exists(deserializedFolder)) // Check if dir still exists
                    {
                        DefaultVirtualEnvRoot = deserializedFolder;
                    }
                    else if (!string.IsNullOrEmpty(deserializedFolder))
                    {
                        // Path exists in config but not on disk - reset or warn?
                        DefaultVirtualEnvRoot = ""; // Reset if invalid
                        Debug.WriteLine($"Warning: Default VirtualEnv Root path not found: {deserializedFolder}");
                    }
                    else
                    {
                        DefaultVirtualEnvRoot = ""; // Reset if empty/null in file
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"기본 가상 환경 루트 경로 로드 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    DefaultVirtualEnvRoot = ""; // Reset on error
                }
            }
            else
            {
                DefaultVirtualEnvRoot = ""; // Initialize if file doesn't exist
            }
        }

        // Save the Default ROOT folder path
        public void SaveDefaultVirtualEnvRoot()
        {
            try
            {
                string json = JsonSerializer.Serialize(DefaultVirtualEnvRoot ?? ""); // Ensure not null
                File.WriteAllText(DEFAULT_ENV_FILE, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"기본 가상 환경 루트 경로 저장 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gets the ACTUAL venv path (containing Scripts) based on Program setting or Default Root
        // Returns empty string if no valid venv found.
        public string GetEffectiveVirtualEnvPath(Program program)
        {
            string pathToCheck;
            bool isSpecific = false;

            if (!string.IsNullOrEmpty(program.VirtualEnvPath))
            {
                // Use specific path set for the program
                pathToCheck = program.VirtualEnvPath;
                isSpecific = true;
            }
            else if (!string.IsNullOrEmpty(DefaultVirtualEnvRoot))
            {
                // Use default root (expecting 'venv' subdir)
                pathToCheck = DefaultVirtualEnvRoot;
            }
            else
            {
                // No specific path and no default root set
                return string.Empty;
            }

            // Now find the actual venv directory (containing Scripts)
            string actualVenvPath = FindActualVenvPath(pathToCheck, isSpecific);
            return actualVenvPath; // Returns empty if not found
        }


        // Helper to find the real venv folder (containing Scripts)
        // pathToCheck: Can be the direct venv path OR the parent directory (like DefaultVirtualEnvRoot)
        // isSpecific: True if pathToCheck came from Program.VirtualEnvPath
        private string FindActualVenvPath(string pathToCheck, bool isSpecific)
        {
            if (string.IsNullOrEmpty(pathToCheck) || !Directory.Exists(pathToCheck))
                return string.Empty;

            // 1. Check if pathToCheck IS the venv folder (contains Scripts)
            if (Directory.Exists(Path.Combine(pathToCheck, "Scripts")))
            {
                return pathToCheck;
            }

            // 2. If using Default Root OR if specific path didn't have Scripts,
            //    check for a 'venv' subfolder inside pathToCheck
            string venvSubfolderPath = Path.Combine(pathToCheck, "venv");
            if (Directory.Exists(Path.Combine(venvSubfolderPath, "Scripts")))
            {
                return venvSubfolderPath;
            }

            // 3. Check common alternative names if checking Default Root
            if (!isSpecific)
            {
                string[] commonVenvNames = { ".venv", "env", ".env", "virtualenv" };
                foreach (string venvName in commonVenvNames)
                {
                    string testPath = Path.Combine(pathToCheck, venvName);
                    if (Directory.Exists(testPath) && Directory.Exists(Path.Combine(testPath, "Scripts")))
                    {
                        return testPath;
                    }
                }
            }

            // If we reach here, no valid venv structure was found based on the input path
            return string.Empty;
        }


        // Sets the VirtualEnvPath for selected programs. Returns true if changes were made.
        public bool SetVirtualEnvForSelectedPrograms(ObservableCollection<Program> programs)
        {
            if (programs == null || programs.Count == 0) return false;

            var dialog = new WF.FolderBrowserDialog
            {
                // Updated Description
                Description = "Select the specific Virtual Environment folder (e.g., C:\\Project\\venv) OR the project's root folder (e.g., C:\\Project).",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            // Set initial directory based on the first selected program's current setting or default
            string initialPath = DefaultVirtualEnvRoot; // Start with default root
            if (!string.IsNullOrEmpty(programs[0].VirtualEnvPath) && Directory.Exists(programs[0].VirtualEnvPath))
            {
                // If a specific path is set and valid, try using its parent or itself
                initialPath = Path.GetDirectoryName(programs[0].VirtualEnvPath) ?? programs[0].VirtualEnvPath;
            }
            else if (!string.IsNullOrEmpty(programs[0].Path) && Directory.Exists(Path.GetDirectoryName(programs[0].Path)))
            {
                // Try the script's directory as a fallback initial path
                initialPath = Path.GetDirectoryName(programs[0].Path)!;
            }

            if (!string.IsNullOrEmpty(initialPath) && Directory.Exists(initialPath))
            {
                dialog.InitialDirectory = initialPath;
            }
            else if (!string.IsNullOrEmpty(DefaultVirtualEnvRoot) && Directory.Exists(DefaultVirtualEnvRoot))
            {
                dialog.InitialDirectory = DefaultVirtualEnvRoot; // Fallback to default root
            }


            if (dialog.ShowDialog() == WF.DialogResult.OK)
            {
                string selectedPath = dialog.SelectedPath;
                if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
                {
                    MessageBox.Show("선택한 경로가 유효하지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Option 1: Reset to Default?
                // Check if selected path IS the Default Root Folder
                bool resetToDefault = false;
                if (selectedPath.Equals(DefaultVirtualEnvRoot, StringComparison.OrdinalIgnoreCase))
                {
                    var result = MessageBox.Show(
                        "선택한 경로는 현재 설정된 '기본 가상환경 루트 폴더'입니다.\n\n" +
                        "선택한 프로그램들의 개별 가상환경 설정을 제거하고 기본 설정을 사용하도록 하시겠습니까?",
                        "기본값으로 재설정 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    resetToDefault = (result == MessageBoxResult.Yes);
                }

                string finalPathToStore = ""; // Path to store in Program.VirtualEnvPath
                if (resetToDefault)
                {
                    finalPathToStore = ""; // Empty string means use default
                }
                else
                {
                    // Validate the selected path - does it look like a venv or contain one?
                    string actualVenv = FindActualVenvPath(selectedPath, true); // Check selected path directly

                    if (!string.IsNullOrEmpty(actualVenv))
                    {
                        // Found a valid venv structure (e.g., selected C:\Project\venv or C:\Project which contains venv)
                        // Store the path that was SELECTED by the user, not necessarily the 'actualVenv' path.
                        // This gives flexibility if the user selected the root C:\Project instead of C:\Project\venv.
                        // The GetEffectiveVirtualEnvPath logic will resolve it correctly later.
                        finalPathToStore = selectedPath;
                    }
                    else
                    {
                        // No direct venv structure found in the selection or its 'venv' subdir
                        var result = MessageBox.Show(
                            "선택한 폴더 또는 그 하위의 'venv' 폴더에서 유효한 Python 가상환경 ('Scripts' 폴더 포함)을 찾을 수 없습니다.\n\n" +
                            $"경로: {selectedPath}\n\n" +
                            "이 경로를 프로그램의 가상환경 경로로 설정하시겠습니까?\n" +
                            "(나중에 이 경로에 유효한 가상환경을 생성해야 할 수 있습니다.)",
                            "가상환경 확인 필요", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.No)
                        {
                            return false; // User cancelled
                        }
                        // User confirmed to set the path anyway
                        finalPathToStore = selectedPath;
                    }
                }

                // Apply the chosen path (or empty for default) to selected programs
                bool changed = false;
                foreach (var program in programs)
                {
                    if (program.VirtualEnvPath != finalPathToStore)
                    {
                        program.VirtualEnvPath = finalPathToStore;
                        // DisplayVirtualEnvPath will be updated by MainWindow after this returns
                        changed = true;
                    }
                }

                if (changed)
                {
                    string message = resetToDefault
                        ? $"{programs.Count}개 프로그램의 가상환경 설정이 기본값을 사용하도록 변경되었습니다."
                        : $"{programs.Count}개 프로그램에 대한 개별 가상환경 경로가 설정되었습니다:\n{finalPathToStore}";
                    MessageBox.Show(message, "설정 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true; // Indicate changes were made
                }
                else
                {
                    MessageBox.Show("선택한 프로그램들의 가상환경 설정이 이미 지정된 경로와 동일합니다.", "변경 없음", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

            } // End if DialogResult.OK
            return false; // Dialog cancelled
        }

        // SaveVirtualEnvironmentSettings is implicitly done by SaveDefaultVirtualEnvRoot

        // SaveProgramList should be called by the caller (MainWindow) after SetVirtualEnvForSelectedPrograms returns true.

    }
}