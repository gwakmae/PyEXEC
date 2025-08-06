// PyExec/Models/ProgramListManager.cs
using PyExec.Models;
using PyExec.Helpers; // For UIHelper if needed, though better in MainWindow
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32; // OpenFileDialog

namespace PyExec.Models
{
    public class ProgramListManager
    {
        private const string PROGRAM_LIST_FILE = "program_list.json";
        private readonly VirtualEnvironmentManager _virtualEnvironmentManager; // Store reference

        // Constructor to receive dependency
        public ProgramListManager(VirtualEnvironmentManager virtualEnvironmentManager)
        {
            _virtualEnvironmentManager = virtualEnvironmentManager ?? throw new ArgumentNullException(nameof(virtualEnvironmentManager));
        }


        public void LoadProgramList(ObservableCollection<Program> programs, string fileName)
        {
            if (!File.Exists(fileName))
            {
                UpdateOrder(programs); // Ensure order is correct even for empty list
                return; // Nothing to load
            }

            try
            {
                var json = File.ReadAllText(fileName);
                var items = JsonSerializer.Deserialize<List<Program>>(json);
                if (items != null)
                {
                    // Sort by the 'Order' property read from the file first
                    items = items.OrderBy(p => p.Order).ToList();

                    programs.Clear();
                    foreach (var p in items)
                    {
                        programs.Add(p);
                        // Note: DisplayVirtualEnvPath is set in MainWindow after loading
                    }
                    // Ensure Order property is sequential AFTER loading and sorting
                    UpdateOrder(programs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading program list: {ex.Message}\n\n{fileName}",
                    "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                programs.Clear(); // Clear list on error to avoid inconsistent state
                UpdateOrder(programs); // Reset order
            }
        }

        public void SaveProgramList(ObservableCollection<Program> programs, string fileName)
        {
            // Ensure order is sequential before saving
            UpdateOrder(programs);
            try
            {
                var json = JsonSerializer.Serialize(
                    programs.ToList(), // Serialize the current state
                    new JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving program list: {ex.Message}\n\n{fileName}",
                    "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Returns true if any programs were added
        public bool AddProgram(ObservableCollection<Program> programs)
        {
            bool added = false;
            var dialog = new OpenFileDialog
            {
                Title = "Add Python Programs",
                Filter = "Python/Exe/Notebook (*.py;*.pyw;*.exe;*.ipynb)|*.py;*.pyw;*.exe;*.ipynb|Python Files (*.py;*.pyw)|*.py;*.pyw|Jupyter Notebook (*.ipynb)|*.ipynb|Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                Multiselect = true
            };
            if (dialog.ShowDialog() == true)
            {
                int nextOrder = programs.Any() ? programs.Max(p => p.Order) + 1 : 1;
                foreach (var path in dialog.FileNames)
                {
                    // Case-insensitive check for existing path
                    if (programs.Any(p => p.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"Program already in list:\n{path}", "Duplicate Program", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    var prog = new Program
                    {
                        Order = nextOrder++, // Assign sequential order
                        Name = Path.GetFileNameWithoutExtension(path) ?? "Unnamed",
                        Description = Path.GetFileNameWithoutExtension(path) ?? "Description needed",
                        Path = path,
                        Category = "Default", // Default category
                        // VirtualEnvPath is empty by default (uses global)
                        // DisplayVirtualEnvPath is set later in MainWindow
                    };
                    programs.Add(prog);
                    added = true;
                }
                if (added)
                {
                    UpdateOrder(programs); // Ensure sequential order
                    // SaveProgramList(programs); // 저장 로직 제거
                }
            }
            return added; // Return whether any program was successfully added
        }


        // Remove multiple programs
        // Returns true if any programs were removed
        public bool RemovePrograms(ObservableCollection<Program> programs, List<Program> programsToRemove)
        {
            if (programsToRemove == null || programsToRemove.Count == 0) return false;

            bool removed = false;
            // Iterate backwards when removing multiple items from a collection
            for (int i = programsToRemove.Count - 1; i >= 0; i--)
            {
                if (programs.Remove(programsToRemove[i]))
                {
                    removed = true;
                }
            }

            if (removed)
            {
                UpdateOrder(programs); // Re-sequence after removal
                // SaveProgramList(programs); // 저장 로직 제거
            }
            return removed;
        }


        // MoveUp for single item
        public void MoveUp(ObservableCollection<Program> programs, Program programToMove)
        {
            if (programToMove == null) return;

            int currentIndex = programs.IndexOf(programToMove);
            if (currentIndex <= 0) return; // Already at top or not found

            programs.Move(currentIndex, currentIndex - 1);
            UpdateOrder(programs); // Update numerical order property
            // Saving is handled by the caller (MainWindow)
        }

        // MoveDown for single item
        public void MoveDown(ObservableCollection<Program> programs, Program programToMove)
        {
            if (programToMove == null) return;

            int currentIndex = programs.IndexOf(programToMove);
            if (currentIndex < 0 || currentIndex >= programs.Count - 1) return; // Not found or already at bottom

            programs.Move(currentIndex, currentIndex + 1);
            UpdateOrder(programs); // Update numerical order property
            // Saving is handled by the caller (MainWindow)
        }

        // Update Order property based on current collection index
        public void UpdateOrder(ObservableCollection<Program> programs)
        {
            for (int i = 0; i < programs.Count; i++)
            {
                if (programs[i].Order != i + 1) // Only update if necessary to reduce notifications
                {
                    programs[i].Order = i + 1;
                }
            }
        }

        // Reorder collection based on the numerical Order property
        public void ReorderByOrder(ObservableCollection<Program> programs)
        {
            var sorted = programs.OrderBy(p => p.Order).ToList();
            // Check if the current order is different from the sorted order
            bool changed = !programs.SequenceEqual(sorted);

            if (changed)
            {
                programs.Clear();
                foreach (var item in sorted)
                {
                    programs.Add(item);
                }
                // Order property itself is now correct due to sorting,
                // but call UpdateOrder again to ensure sequence 1, 2, 3...
                // if the loaded Order numbers had gaps or duplicates.
                UpdateOrder(programs);
            }
        }

        // Method to get default root (relies on injected manager)
        public string GetDefaultVirtualEnvRoot()
        {
            // Ensure it's loaded if needed, though LoadProgramList might do it earlier
            _virtualEnvironmentManager.LoadDefaultVirtualEnvRoot();
            return _virtualEnvironmentManager.DefaultVirtualEnvRoot;
        }
    }
}