// PyExec/Helpers/UIHelper.cs
using PyExec.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows; // DependencyObject
using System.Windows.Controls;
using System.Windows.Media; // VisualTreeHelper

namespace PyExec.Helpers
{
    public static class UIHelper
    {
        // CodeViewer 업데이트 도우미 메서드 (minor improvement for ipynb)
        public static void UpdateCodeViewer(TextBox codeViewer, Program? selectedProgram)
        {
            // ... (existing null check, file not found check, exe check) ...
            if (selectedProgram == null)
            {
                codeViewer.Text = "No program selected.";
                codeViewer.ToolTip = null;
                return;
            }

            codeViewer.ToolTip = selectedProgram.Path; // Show full path in tooltip

            try
            {
                if (!File.Exists(selectedProgram.Path))
                {
                    codeViewer.Text = $"File not found:\n{selectedProgram.Path}";
                    return;
                }

                string ext = Path.GetExtension(selectedProgram.Path).ToLowerInvariant();
                if (ext == ".exe")
                {
                    codeViewer.Text = "Executable File (.exe)\n\nSource code not available.\n\nDouble-click or press F5 to run.";
                    return;
                }

                if (ext == ".ipynb")
                {
                    // Simplified ipynb handling for display
                    try
                    {
                        string content = File.ReadAllText(selectedProgram.Path);
                        // Basic check if it looks like JSON
                        if (content.TrimStart().StartsWith("{") && content.TrimEnd().EndsWith("}"))
                        {
                            codeViewer.Text = $"Jupyter Notebook (.ipynb)\n\nDisplaying raw JSON content.\nUse external tool (like VS Code, Jupyter) to view/run notebooks.\n\n---\n{content}";
                        }
                        else
                        {
                            codeViewer.Text = $"Jupyter Notebook (.ipynb)\n\nCould not parse as JSON.\n\n---\n{content}";
                        }
                    }
                    catch (Exception readEx)
                    {
                        codeViewer.Text = $"Error reading .ipynb file: {readEx.Message}";
                    }
                    return; // Stop after handling ipynb
                }

                // Handle other text-based files (.py, .pyw, .txt, etc.)
                try
                {
                    string content = File.ReadAllText(selectedProgram.Path);
                    codeViewer.Text = content; // Assign content directly
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        codeViewer.Text = "(File is empty)";
                    }
                }
                catch (Exception readEx)
                {
                    codeViewer.Text = $"Error reading file: {readEx.Message}";
                }

            }
            catch (Exception ex)
            {
                // General error catch
                codeViewer.Text = $"Error loading file view: {ex.Message}";
            }
        }


        // ### [수정] 가상 환경 경로 표시 문자열 가져오기 로직 ###
        public static string GetDisplayVirtualEnvPath(Program program, string defaultVirtualEnvRoot)
        {
            // 프로그램이 .exe 파일인 경우, 가상 환경 정보가 필요 없으므로 "N/A" 반환
            if (program.RunnerDisplay.Equals("exe", StringComparison.OrdinalIgnoreCase))
            {
                return "N/A";
            }

            if (!string.IsNullOrEmpty(program.VirtualEnvPath))
            {
                // Specific path is set for the program
                return program.VirtualEnvPath + " (Specific)";
            }
            else if (!string.IsNullOrEmpty(defaultVirtualEnvRoot))
            {
                // No specific path, use default root.
                return defaultVirtualEnvRoot + "\\venv (Default)";
            }
            else
            {
                // No specific path and no default root defined.
                return "(Not Set)";
            }
        }

        // Helper to find ancestor of a specific type in the visual tree
        public static T? TryFindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}