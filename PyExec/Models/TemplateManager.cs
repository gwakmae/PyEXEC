// PyExec/Models/TemplateManager.cs
using PyExec.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls; // ListBox (Keep for context, though not used directly)

namespace PyExec.Models
{
    public class TemplateManager
    {
        private const string RECENT_TEMPLATES_FILE = "recent_templates.json";
        private List<string> _recentTemplates = new List<string>();

        public TemplateManager()
        {
            LoadRecentTemplates(); // 생성자에서 로드
        }

        // 템플릿 저장 (No changes needed here, relies on AddRecentTemplate)
        public void SaveTemplate(ObservableCollection<Program> programs)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Template Files (*.tpl)|*.tpl|All Files (*.*)|*.*",
                DefaultExt = ".tpl"
            };

            if (programs.Count > 0)
            {
                string suggestedFileName = string.Join("_", programs.Take(3).Select(p => p.Name));
                dialog.FileName = SanitizeFileName(suggestedFileName) + ".tpl"; // Sanitize filename
            }
            else
            {
                dialog.FileName = "Template.tpl";
            }

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = JsonSerializer.Serialize(
                        programs.ToList(),
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    File.WriteAllText(dialog.FileName, json);

                    // 최근 템플릿 목록에 추가 (modified logic will handle it)
                    AddRecentTemplate(dialog.FileName);
                    // MainWindow should update its view after this call returns

                    MessageBox.Show("Template saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving template: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Helper to remove invalid characters from filenames
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }


        // 템플릿 로드 (No changes needed here, relies on AddRecentTemplate)
        public void LoadTemplate(ObservableCollection<Program> programs, ProgramListManager programListManager) // Removed ListBox parameter
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Template Files (*.tpl)|*.tpl|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                string tpl = dialog.FileName;
                // Ask for confirmation BEFORE loading
                var result = MessageBox.Show($"Load programs from template '{Path.GetFileName(tpl)}'?\nThis will replace the current list.",
                                             "Confirm Load Template", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    LoadTemplateFromFile(tpl, programs, programListManager); // 수정: 파일 이름 전달
                    AddRecentTemplate(tpl); // 최근 템플릿 추가 (modified logic will handle it)
                                            // MainWindow should update its view after this call returns
                }
            }
        }

        // 파일에서 템플릿 로드 (내부 메서드) - 저장 로직 제거
        public void LoadTemplateFromFile(string fileName, ObservableCollection<Program> programs, ProgramListManager programListManager)
        {
            try
            {
                var json = File.ReadAllText(fileName);
                var items = JsonSerializer.Deserialize<List<Program>>(json);
                if (items != null)
                {
                    // ### [추가] 템플릿에서 불러온 데이터 정리 로직 ###
                    foreach (var p in items)
                    {
                        if (p.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            p.VirtualEnvPath = "";
                        }
                    }

                    programs.Clear();
                    foreach (var item in items)
                    {
                        programs.Add(item);
                    }

                    programListManager.ReorderByOrder(programs);
                    programListManager.UpdateOrder(programs);
                    // programListManager.SaveProgramList(programs); // 저장 로직 제거

                    MessageBox.Show($"Template '{Path.GetFileName(fileName)}' loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading template '{Path.GetFileName(fileName)}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // *** MODIFIED: 최근 템플릿 목록에 추가 ***
        public void AddRecentTemplate(string filePath)
        {
            // Check if it exists (case-insensitive)
            bool exists = _recentTemplates.Any(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase));

            // Only add if it doesn't already exist
            if (!exists)
            {
                _recentTemplates.Insert(0, filePath); // Add new items to the top

                // Limit the list size
                const int maxRecentFiles = 10;
                if (_recentTemplates.Count > maxRecentFiles)
                {
                    _recentTemplates.RemoveRange(maxRecentFiles, _recentTemplates.Count - maxRecentFiles);
                }
                SaveRecentTemplates(); // Save the changes
            }
            // If it exists, do nothing - maintain manual order.
        }

        // ### [추가] 템플릿 덮어쓰기 저장 메서드 ###
        /// <summary>
        /// 지정된 파일 경로에 현재 프로그램 목록을 덮어씁니다.
        /// </summary>
        /// <param name="filePath">덮어쓸 템플릿 파일의 전체 경로</param>
        /// <param name="programs">저장할 프로그램 목록</param>
        public void OverwriteTemplate(string filePath, ObservableCollection<Program> programs)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("저장할 템플릿 파일의 경로가 유효하지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(
                    programs.ToList(),
                    new JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(filePath, json);

                MessageBox.Show($"템플릿을 성공적으로 덮어썼습니다.\n\n{Path.GetFileName(filePath)}", "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"템플릿 덮어쓰기 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void RemoveRecentTemplate(string templatePath)
        {
            int index = _recentTemplates.FindIndex(p => p.Equals(templatePath, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                _recentTemplates.RemoveAt(index);
                SaveRecentTemplates();
            }
        }

        //최근 템플릿 불러오기 (public으로 변경)
        public void LoadRecentTemplates()
        {
            if (File.Exists(RECENT_TEMPLATES_FILE))
            {
                try
                {
                    string json = File.ReadAllText(RECENT_TEMPLATES_FILE);
                    _recentTemplates = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
                    // Ensure no duplicates after loading (optional cleanup)
                    _recentTemplates = _recentTemplates.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                }
                catch
                {
                    _recentTemplates = new List<string>(); // Reset on error
                }
            }
            else
            {
                _recentTemplates = new List<string>(); // Initialize if file doesn't exist
            }
        }

        //최근 템플릿 저장
        private void SaveRecentTemplates()
        {
            try
            {
                string json = JsonSerializer.Serialize(
                    _recentTemplates,
                    new JsonSerializerOptions { WriteIndented = true }
                );

                File.WriteAllText(RECENT_TEMPLATES_FILE, json);
            }
            catch (Exception ex)
            {
                // Avoid showing message box for background saves, maybe log instead?
                Debug.WriteLine($"Error saving recent templates: {ex.Message}");
                // Or show if critical:
                // MessageBox.Show($"Error saving recent templates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // DataGrid 업데이트 메서드는 더 이상 필요 없음 (MainWindow handles its ObservableCollection)
        // public void UpdateRecentTemplatesDataGrid(DataGrid recentTemplatesGrid) { ... }


        // 템플릿 위로 이동
        public void MoveTemplateUp(ObservableCollection<string> templates, System.Collections.IList selectedItems)
        {
            var selectedTemplates = selectedItems.Cast<string>().ToList();
            if (selectedTemplates.Count != 1) return; // Only move one at a time for simplicity

            string itemToMove = selectedTemplates[0];
            int currentIndex = templates.IndexOf(itemToMove);

            if (currentIndex > 0) // Can move up
            {
                templates.Move(currentIndex, currentIndex - 1);
                // Update internal list and save
                _recentTemplates = templates.ToList();
                SaveRecentTemplates();
            }
        }

        // 템플릿 아래로 이동
        public void MoveTemplateDown(ObservableCollection<string> templates, System.Collections.IList selectedItems)
        {
            var selectedTemplates = selectedItems.Cast<string>().ToList();
            if (selectedTemplates.Count != 1) return; // Only move one at a time

            string itemToMove = selectedTemplates[0];
            int currentIndex = templates.IndexOf(itemToMove);

            if (currentIndex >= 0 && currentIndex < templates.Count - 1) // Can move down
            {
                templates.Move(currentIndex, currentIndex + 1);
                // Update internal list and save
                _recentTemplates = templates.ToList();
                SaveRecentTemplates();
            }
        }

        // GetRecentTemplates 메서드 추가
        public List<string> GetRecentTemplates()
        {
            // Return a copy to prevent external modification
            return new List<string>(_recentTemplates);
        }
    }
}