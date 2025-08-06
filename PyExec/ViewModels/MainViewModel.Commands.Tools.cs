// PyExec/ViewModels/MainViewModel.Commands.Tools.cs
using PyExec.Models;
using PyExec.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WF = System.Windows.Forms;

namespace PyExec.ViewModels
{
    public partial class MainViewModel
    {
        // --- Tools Command Implementations ---

        private void ExecuteSetDefaultVenv(object? parameter)
        {
            using (var dialog = new WF.FolderBrowserDialog())
            {
                dialog.Description = "Select Default Virtual Environment Root Folder (e.g., C:\\Venvs)";
                dialog.SelectedPath = _venvManager.DefaultVirtualEnvRoot;

                if (dialog.ShowDialog() == WF.DialogResult.OK)
                {
                    _venvManager.DefaultVirtualEnvRoot = dialog.SelectedPath;
                    _venvManager.SaveDefaultVirtualEnvRoot();
                    UpdateVirtualEnvInfo();
                    RefreshPanelDisplayPaths(Panel1VM);
                    RefreshPanelDisplayPaths(Panel2VM);
                }
            }
        }

        private void ExecuteSetVenvForSelected(object? parameter)
        {
            var selectedProgram = ActivePanelVM.SelectedProgram;
            if (selectedProgram == null) return;

            var programsToModify = new ObservableCollection<Program> { selectedProgram };
            if (_venvManager.SetVirtualEnvForSelectedPrograms(programsToModify))
            {
                ActivePanelVM.SavePrograms();
                RefreshPanelDisplayPaths(ActivePanelVM);
            }
        }

        private void ExecuteSetExecutionMethod(bool useUv)
        {
            var selectedProgram = ActivePanelVM.SelectedProgram;
            if (selectedProgram != null && selectedProgram.UseUvRun != useUv)
            {
                selectedProgram.UseUvRun = useUv;
                ActivePanelVM.SavePrograms();
            }
        }

        private void ExecuteOpenCmdDefaultVenv(object? parameter)
        {
            string defaultVenvRoot = _venvManager.DefaultVirtualEnvRoot;
            if (string.IsNullOrEmpty(defaultVenvRoot))
            {
                MessageBox.Show("기본 가상 환경 폴더가 설정되지 않았습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string actualVenvPath = Path.Combine(defaultVenvRoot, "venv");
            string activateScript = Path.Combine(actualVenvPath, "Scripts", "activate.bat");

            if (!File.Exists(activateScript))
            {
                activateScript = Path.Combine(defaultVenvRoot, "Scripts", "activate.bat");
            }

            if (File.Exists(activateScript))
            {
                OpenCmd(defaultVenvRoot, activateScript);
            }
            else
            {
                MessageBox.Show($"기본 가상 환경의 'activate.bat' 파일을 찾을 수 없습니다: {activateScript}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteOpenCmdSelectedVenv(object? parameter)
        {
            var selectedProg = ActivePanelVM.SelectedProgram;
            if (selectedProg == null) return;

            string venvPathToUse = _venvManager.GetEffectiveVirtualEnvPath(selectedProg);
            if (string.IsNullOrEmpty(venvPathToUse))
            {
                MessageBox.Show("선택된 프로그램에 유효한 가상 환경 경로가 설정되지 않았습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string activateScript = Path.Combine(venvPathToUse, "Scripts", "activate.bat");
            if (!File.Exists(activateScript))
            {
                MessageBox.Show($"선택된 프로그램의 가상 환경 'activate.bat' 파일을 찾을 수 없습니다: {activateScript}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string workingDir = Path.GetDirectoryName(selectedProg.Path) ?? Environment.CurrentDirectory;
            OpenCmd(workingDir, activateScript);
        }

        private void ExecuteConvertPyPyw(object? parameter)
        {
            var program = ActivePanelVM.SelectedProgram;
            if (program == null) return;

            string currentPath = program.Path;
            string newPath;

            if (currentPath.EndsWith(".py", StringComparison.OrdinalIgnoreCase))
            {
                newPath = Path.ChangeExtension(currentPath, ".pyw");
            }
            else if (currentPath.EndsWith(".pyw", StringComparison.OrdinalIgnoreCase))
            {
                newPath = Path.ChangeExtension(currentPath, ".py");
            }
            else
            {
                MessageBox.Show("선택된 파일은 .py 또는 .pyw 확장자가 아닙니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                File.Move(currentPath, newPath);
                program.Path = newPath;
                ActivePanelVM.SavePrograms();
                MessageBox.Show($"파일 확장자가 성공적으로 변경되었습니다:\n{Path.GetFileName(currentPath)} -> {Path.GetFileName(newPath)}", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 확장자 변경 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteApplyNameToDescription(object? parameter)
        {
            var program = ActivePanelVM.SelectedProgram;
            if (program == null) return;
            
            string fileName = Path.GetFileNameWithoutExtension(program.Path);
            if (program.Description != fileName)
            {
                program.Description = fileName;
                ActivePanelVM.SavePrograms();
            }
        }

        private void ExecuteApplyFolderNameToDescription(object? parameter)
        {
            var program = ActivePanelVM.SelectedProgram;
            if (program == null) return;

            string? folderPath = Path.GetDirectoryName(program.Path);
            if (!string.IsNullOrEmpty(folderPath))
            {
                string lastFolderName = new DirectoryInfo(folderPath).Name;
                if (program.Description != lastFolderName)
                {
                    program.Description = lastFolderName;
                    ActivePanelVM.SavePrograms();
                }
            }
        }

        private async Task ExecuteShowProcessStartInfoAsync()
        {
            var selectedProgram = ActivePanelVM.SelectedProgram;
            if (selectedProgram != null)
            {
                await _pythonExecutor.ShowStartInfoAsync(selectedProgram);
            }
        }

        // --- Helper Methods for Tools Commands ---

        private void OpenCmd(string workingDir, string activateScript)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/k \"cd /d \"{workingDir}\" && call \"{activateScript}\"\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = workingDir
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CMD 실행 중 오류 발생: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateVirtualEnvInfo()
        {
            _venvManager.LoadDefaultVirtualEnvRoot();
            VirtualEnvInfo = string.IsNullOrEmpty(_venvManager.DefaultVirtualEnvRoot)
                ? "Default VEnv Root: (Not Set)"
                : $"Default VEnv Root: {_venvManager.DefaultVirtualEnvRoot}";
        }

        private void RefreshPanelDisplayPaths(ProgramPanelViewModel panelVM)
        {
            foreach (var prog in panelVM.Programs)
            {
                prog.DisplayVirtualEnvPath = UIHelper.GetDisplayVirtualEnvPath(prog, _venvManager.DefaultVirtualEnvRoot);
            }
        }
    }
}
