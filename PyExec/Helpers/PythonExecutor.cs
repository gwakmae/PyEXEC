// PyExec/Helpers/PythonExecutor.cs
using PyExec.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks; // 추가
using System.Windows;
using System.Linq;
using System.ComponentModel;

namespace PyExec.Helpers
{
    public class PythonExecutor
    {
        private readonly VirtualEnvironmentManager _virtualEnvironmentManager;
        private List<Process> spawnedCmdProcesses = new List<Process>(); // 이 리스트 사용 여부 재검토 필요

        // 진단 스크립트 파일 이름
        private const string DIAGNOSTICS_SCRIPT_NAME = "diagnostics.py";

        // 클래스 멤버 변수로 추가
        private bool _showScriptOutput = true; // 기본값은 표시

        // 설정 메서드 추가
        public void SetShowScriptOutput(bool show)
        {
            _showScriptOutput = show;
        }

        public PythonExecutor(VirtualEnvironmentManager virtualEnvironmentManager)
        {
            _virtualEnvironmentManager = virtualEnvironmentManager;
        }

        // ### [추가] 지정된 스크립트를 새 CMD 창에서 실행하는 메서드 ###
        public void ExecuteInCmd(Program program)
        {
            if (program == null) return;

            string scriptPath = program.Path;
            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                MessageBox.Show($"스크립트 파일을 찾을 수 없습니다: {scriptPath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string? workingDir = Path.GetDirectoryName(scriptPath);
            if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
            {
                MessageBox.Show($"스크립트의 유효한 작업 디렉토리를 찾을 수 없습니다: {workingDir ?? "N/A"}", "작업 폴더 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                workingDir = Environment.CurrentDirectory; // Fallback
            }

            string actualVenvPath = _virtualEnvironmentManager.GetEffectiveVirtualEnvPath(program);
            string commandToRun = $"py \"{scriptPath}\"";
            string commandChain;

            if (!string.IsNullOrEmpty(actualVenvPath))
            {
                string activateScript = Path.Combine(actualVenvPath, "Scripts", "activate.bat");
                if (File.Exists(activateScript))
                {
                    commandChain = $"call \"{activateScript}\" && {commandToRun}";
                }
                else
                {
                    MessageBox.Show($"가상 환경의 'activate.bat' 파일을 찾을 수 없습니다: {activateScript}. 가상환경 활성화 없이 실행합니다.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
                    commandChain = commandToRun;
                }
            }
            else
            {
                commandChain = commandToRun;
            }

            string finalCommand = $"cd /d \"{workingDir}\" && {commandChain}";

            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/k \"{finalCommand}\"")
                {
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CMD 창을 여는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // --- MODIFIED Method to Show Detailed Start Info ---
        public async Task ShowStartInfoAsync(Program program) // 비동기 메서드로 변경
        {
            if (program == null)
            {
                MessageBox.Show("정보를 표시할 프로그램을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string scriptPath = program.Path; // 사용자 스크립트 경로 (Working Directory 결정용)
            if (string.IsNullOrEmpty(scriptPath))
            {
                MessageBox.Show($"프로그램 경로가 비어있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Determine windowless mode (diagnostics 스크립트는 콘솔 모드로 실행)
            // bool useWindowlessMode = (Path.GetExtension(program.Path).ToLower() == ".pyw");

            // --- 환경 결정 로직 (기존과 유사) ---
            string venvRootPath = GetVenvRootPath(program); // 프로그램 설정 또는 기본 루트

            // GetEffectiveVirtualEnvPath 사용으로 변경하여 실제 venv 경로 가져오기
            string actualVenvPath = _virtualEnvironmentManager.GetEffectiveVirtualEnvPath(program);

            // Check if actualVenvPath is valid before proceeding
            if (string.IsNullOrEmpty(actualVenvPath) && !program.UseUvRun) // uv run은 venv 경로가 필요 없을 수 있음
            {
                // Try to determine based on root path IF default was used
                if (string.IsNullOrEmpty(program.VirtualEnvPath) && !string.IsNullOrEmpty(venvRootPath))
                {
                    actualVenvPath = FindActualVenvPath(venvRootPath); // Use old helper as fallback display
                }

                // Still no path? Show error referencing both specific and default.
                if (string.IsNullOrEmpty(actualVenvPath))
                {
                    string specificPathInfo = !string.IsNullOrEmpty(program.VirtualEnvPath) ? program.VirtualEnvPath : "(Not Set)";
                    string defaultPathInfo = !string.IsNullOrEmpty(_virtualEnvironmentManager.DefaultVirtualEnvRoot) ? _virtualEnvironmentManager.DefaultVirtualEnvRoot : "(Not Set)";
                    MessageBox.Show($"유효한 Python 가상 환경을 찾을 수 없습니다.\n\n" +
                                    $"- 프로그램 지정 경로: {specificPathInfo}\n" +
                                    $"- 기본 루트 경로: {defaultPathInfo}\n\n" +
                                    "경로에 'Scripts' 폴더가 포함된 유효한 가상환경이 있는지 확인하세요.",
                                    "가상 환경 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // If we found a path via fallback, show a warning that execution might fail
                else
                {
                    MessageBox.Show($"경고: 지정된 경로에서 유효한 가상 환경('Scripts' 폴더 포함)을 직접 찾지 못했습니다.\n" +
                                    $"추정된 경로 '{actualVenvPath}'를 사용하여 진단을 시도합니다. 실제 실행은 실패할 수 있습니다.",
                                    "가상 환경 경고", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }


            string scriptsPath = Path.Combine(actualVenvPath, "Scripts");
            // 진단 스크립트는 항상 python.exe 로 실행 (pythonw.exe 불필요)
            string pythonExe = GetPythonExePath(actualVenvPath, useWindowlessMode: false);

            if (string.IsNullOrEmpty(pythonExe) && !program.UseUvRun)
            {
                MessageBox.Show($"가상 환경에서 Python 실행 파일(python.exe)을 찾을 수 없습니다.\n경로: {scriptsPath}", "Python 실행 파일 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 작업 디렉토리는 사용자 스크립트의 디렉토리로 설정 (진단 스크립트 실행 시에도 동일하게)
            string? workingDir = Path.GetDirectoryName(scriptPath);
            if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
            {
                MessageBox.Show($"스크립트의 작업 디렉토리를 찾을 수 없거나 유효하지 않습니다: {workingDir ?? "N/A"}", "작업 폴더 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                workingDir = Environment.CurrentDirectory; // Fallback
                MessageBox.Show($"현재 프로그램의 작업 폴더({workingDir})를 사용합니다.", "작업 폴더 알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // --- 진단 스크립트 경로 찾기 ---
            string diagnosticsScriptFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DIAGNOSTICS_SCRIPT_NAME);
            if (!File.Exists(diagnosticsScriptFullPath))
            {
                // 실행 파일 경로 외에 다른 경로도 확인해볼 수 있음 (예: 소스코드 위치 기준)
                // string altPath = Path.Combine(Directory.GetCurrentDirectory(), DIAGNOSTICS_SCRIPT_NAME);
                // if (File.Exists(altPath)) diagnosticsScriptFullPath = altPath;
                // else { // 최종 실패
                MessageBox.Show($"진단 스크립트 파일을 찾을 수 없습니다: {diagnosticsScriptFullPath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
                // }
            }

            // --- 기본 정보 구성 ---
            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.AppendLine("--- Pre-Execution Environment Setup ---");
            infoBuilder.AppendLine($"Selected Program: {program.Name}");
            infoBuilder.AppendLine($"Target Script: {scriptPath}");
            infoBuilder.AppendLine($"Execution Method: {(program.UseUvRun ? "uv run" : "python.exe")}");
            infoBuilder.AppendLine($"Determined Working Directory: {workingDir}");

            if (program.UseUvRun)
            {
                infoBuilder.AppendLine($"Executable: uv.exe (expected in PATH)");
            }
            else
            {
                infoBuilder.AppendLine($"Using Python Executable: {pythonExe}");
                infoBuilder.AppendLine($"Source Venv Path: {venvRootPath} {(string.IsNullOrEmpty(program.VirtualEnvPath) ? "(Default Root)" : "(Specific)")}");
                infoBuilder.AppendLine($"Resolved Actual Venv Path (contains Scripts): {actualVenvPath}");
                infoBuilder.AppendLine($"-> Venv Scripts Path: {scriptsPath}");
                infoBuilder.AppendLine($"--- Environment Variables Set for Process ---");
                infoBuilder.AppendLine($"VIRTUAL_ENV = {actualVenvPath}");
                string originalPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                string newPath = $"{scriptsPath};{originalPath}";
                infoBuilder.AppendLine($"PATH = {scriptsPath};[Original PATH]"); // 전체 PATH는 너무 길어서 생략
            }


            // --- 진단 스크립트 실행 ---
            infoBuilder.AppendLine("\n--- Running Diagnostics Script ---");

            var startInfo = new ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{diagnosticsScriptFullPath}\"", // 진단 스크립트 실행
                WorkingDirectory = workingDir, // 사용자 스크립트 기준 작업 폴더
                UseShellExecute = false,
                RedirectStandardOutput = true, // 출력 리디렉션
                RedirectStandardError = true,  // 에러 리디렉션
                CreateNoWindow = true,         // 콘솔 창 숨김
                StandardOutputEncoding = Encoding.UTF8, // UTF-8 인코딩 지정
                StandardErrorEncoding = Encoding.UTF8
            };

            // 환경 변수 설정
            if (!program.UseUvRun)
            {
                startInfo.EnvironmentVariables["VIRTUAL_ENV"] = actualVenvPath;
                startInfo.EnvironmentVariables["PATH"] = $"{scriptsPath};{Environment.GetEnvironmentVariable("PATH")}";
            }

            // PYTHONPATH/PYTHONHOME은 가상환경 문제를 일으킬 수 있으므로 제거
            if (startInfo.EnvironmentVariables.ContainsKey("PYTHONPATH"))
                startInfo.EnvironmentVariables.Remove("PYTHONPATH");
            if (startInfo.EnvironmentVariables.ContainsKey("PYTHONHOME"))
                startInfo.EnvironmentVariables.Remove("PYTHONHOME");
            // Force UTF-8 for Python I/O if needed (especially on Windows)
            startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";


            string diagnosticsOutput = "";
            string diagnosticsError = "";

            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // 비동기적으로 출력 읽기 (UI 블로킹 방지)
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // 최대 15초 대기 (무한 대기 방지)
                    bool exited = await Task.Run(() => process.WaitForExit(15000));

                    if (exited)
                    {
                        diagnosticsOutput = await outputTask;
                        diagnosticsError = await errorTask;
                        if (process.ExitCode != 0)
                        {
                            infoBuilder.AppendLine($"\n--- Diagnostics Script Finished with Exit Code: {process.ExitCode} ---");
                        }
                    }
                    else
                    {
                        infoBuilder.AppendLine("\n--- Diagnostics Script Timed Out (15 seconds) ---");
                        try { process.Kill(); } catch { /* Ignore kill error */ }
                    }
                }
            }
            catch (Exception ex)
            {
                infoBuilder.AppendLine($"\n--- Error Running Diagnostics Script ---");
                infoBuilder.AppendLine(ex.ToString());
            }

            // --- 결과 종합 및 표시 ---
            if (!string.IsNullOrWhiteSpace(diagnosticsOutput))
            {
                infoBuilder.AppendLine("\n--- Diagnostics Script Output ---");
                infoBuilder.AppendLine(diagnosticsOutput.Trim()); // 앞뒤 공백 제거
            }
            if (!string.IsNullOrWhiteSpace(diagnosticsError))
            {
                infoBuilder.AppendLine("\n--- Diagnostics Script Error Output ---");
                infoBuilder.AppendLine(diagnosticsError.Trim());
            }

            ShowInformationDialog("Process Start Information", infoBuilder.ToString());
        }

        // --- ExecutePythonScriptAsync (REVISED) ---
        public async Task ExecutePythonScriptAsync(Program program, bool useWindowlessMode = false)
        {
            if (program == null) return;

            string scriptPath = program.Path;
            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
            {
                MessageBox.Show($"스크립트 파일을 찾을 수 없습니다: {scriptPath}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string? workingDir = Path.GetDirectoryName(scriptPath);
            if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
            {
                MessageBox.Show($"스크립트의 유효한 작업 디렉토리를 찾을 수 없습니다: {workingDir ?? "N/A"}\n현재 프로그램의 작업 폴더를 사용합니다.", "작업 폴더 경고", MessageBoxButton.OK, MessageBoxImage.Warning);
                workingDir = Environment.CurrentDirectory;
            }

            ProcessStartInfo startInfo;
            StringBuilder outputLog = new StringBuilder();

            if (program.UseUvRun)
            {
                // --- UV RUN 실행 경로 ---
                outputLog.AppendLine($"--- uv run으로 실행 준비 중 ---");
                startInfo = new ProcessStartInfo
                {
                    FileName = "uv.exe", // 'uv'가 PATH에 있다고 가정
                    Arguments = $"run \"{scriptPath}\"",
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };
                // 'uv run'은 자체적으로 환경을 관리하므로 VIRTUAL_ENV 또는 PATH 조작이 필요 없음
            }
            else
            {
                // --- PYTHON.EXE 실행 경로 (기존 로직) ---
                outputLog.AppendLine($"--- python.exe로 실행 준비 중 ---");
                string actualVenvPath = _virtualEnvironmentManager.GetEffectiveVirtualEnvPath(program);
                if (string.IsNullOrEmpty(actualVenvPath))
                {
                    string specificPathInfo = !string.IsNullOrEmpty(program.VirtualEnvPath) ? program.VirtualEnvPath : "(설정 안됨)";
                    string defaultPathInfo = !string.IsNullOrEmpty(_virtualEnvironmentManager.DefaultVirtualEnvRoot) ? _virtualEnvironmentManager.DefaultVirtualEnvRoot : "(설정 안됨)";
                    MessageBox.Show($"스크립트 실행 실패: 유효한 Python 가상 환경을 찾을 수 없습니다.\n\n" +
                                    $"- 프로그램 지정 경로: {specificPathInfo}\n" +
                                    $"- 기본 루트 경로: {defaultPathInfo}\n\n" +
                                    "지정된 위치에 'Scripts' 폴더를 포함한 유효한 가상 환경이 있는지 확인하세요.",
                                    "가상 환경 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string scriptsPath = Path.Combine(actualVenvPath, "Scripts");
                string pythonExe = GetPythonExePath(actualVenvPath, false); // 항상 python.exe 사용

                if (string.IsNullOrEmpty(pythonExe))
                {
                    MessageBox.Show($"가상 환경에서 Python 실행 파일(python.exe)을 찾을 수 없습니다.\n경로: {scriptsPath}", "Python 실행 파일 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptPath}\"",
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                // 표준 파이썬 실행을 위한 환경 변수 설정
                startInfo.EnvironmentVariables["VIRTUAL_ENV"] = actualVenvPath;
                string originalPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                string newPath = $"{scriptsPath};{originalPath}";
                startInfo.EnvironmentVariables["PATH"] = newPath;
                if (startInfo.EnvironmentVariables.ContainsKey("PYTHONPATH"))
                    startInfo.EnvironmentVariables.Remove("PYTHONPATH");
                if (startInfo.EnvironmentVariables.ContainsKey("PYTHONHOME"))
                    startInfo.EnvironmentVariables.Remove("PYTHONHOME");
                startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            }

            // --- 공통 실행 및 로깅 로직 ---
            try
            {
                using (Process process = new Process { StartInfo = startInfo, EnableRaisingEvents = true })
                {
                    process.OutputDataReceived += (sender, e) => {
                        if (e.Data != null) lock (outputLog) { outputLog.AppendLine(e.Data); }
                    };
                    process.ErrorDataReceived += (sender, e) => {
                        if (e.Data != null) lock (outputLog) { outputLog.AppendLine($"ERROR: {e.Data}"); }
                    };

                    outputLog.AppendLine($"--- 프로세스 시작: {startInfo.FileName} ---");
                    outputLog.AppendLine($"인수: {startInfo.Arguments}");
                    outputLog.AppendLine($"작업 디렉토리: {startInfo.WorkingDirectory}");

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool exited = await Task.Run(() => process.WaitForExit(86400000)); // 24시간 타임아웃

                    outputLog.AppendLine($"--- 프로세스 {(exited ? $"종료 코드: {process.ExitCode}" : "시간 내에 종료되지 않음")} ---");
                    if (!exited)
                    {
                        try { process.Kill(); } catch { }
                        outputLog.AppendLine("--- 타임아웃으로 인해 프로세스 강제 종료됨 ---");
                    }
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 2) // ERROR_FILE_NOT_FOUND
            {
                outputLog.AppendLine($"\n--- 치명적 오류 ---");
                outputLog.AppendLine($"실행 파일 '{startInfo.FileName}'을(를) 찾을 수 없습니다.");
                outputLog.AppendLine("프로그램이 설치되어 있고 시스템의 PATH 환경 변수에 경로가 포함되어 있는지 확인하세요.");
                outputLog.AppendLine(ex.ToString());

                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"실행 실패: '{startInfo.FileName}' 명령을 찾을 수 없습니다.\n\n프로그램이 설치되어 있고 시스템의 PATH에 있는지 확인하세요.", "명령을 찾을 수 없음", MessageBoxButton.OK, MessageBoxImage.Error)
                );
            }
            catch (Exception ex)
            {
                outputLog.AppendLine($"\n--- 프로세스 시작 또는 모니터링 중 오류 ---");
                outputLog.AppendLine(ex.ToString());
                Application.Current.Dispatcher.Invoke(() =>
                    MessageBox.Show($"스크립트 프로세스 시작 중 오류 발생: {ex.Message}", "프로세스 오류", MessageBoxButton.OK, MessageBoxImage.Error)
                );
            }
            finally
            {
                if (_showScriptOutput)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        ShowInformationDialog($"스크립트 출력/오류 로그 ({program.Name})", outputLog.ToString())
                    );
                }
            }
        }


        [Obsolete("Use ExecutePythonScriptAsync instead.")]
        public void ExecutePythonScript(Program program, bool useWindowlessMode = false)
        {
            _ = ExecutePythonScriptAsync(program, useWindowlessMode);
        }

        private string GetVenvRootPath(Program program)
        {
            if (!string.IsNullOrEmpty(program.VirtualEnvPath))
            {
                return program.VirtualEnvPath;
            }
            return _virtualEnvironmentManager.DefaultVirtualEnvRoot;
        }

        private string FindActualVenvPath(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return string.Empty;

            if (Directory.Exists(Path.Combine(rootPath, "Scripts")))
            {
                return rootPath;
            }

            string[] commonVenvNames = { "venv", ".venv", "env", ".env", "virtualenv" };
            foreach (string venvName in commonVenvNames)
            {
                string testPath = Path.Combine(rootPath, venvName);
                if (Directory.Exists(testPath) && Directory.Exists(Path.Combine(testPath, "Scripts")))
                {
                    return testPath;
                }
            }
            return string.Empty;
        }

        private string GetPythonExePath(string actualVenvPath, bool useWindowlessMode)
        {
            if (string.IsNullOrEmpty(actualVenvPath) || !Directory.Exists(actualVenvPath))
            {
                return string.Empty;
            }

            string pythonExe = useWindowlessMode ? "pythonw.exe" : "python.exe";
            string pythonPath = Path.Combine(actualVenvPath, "Scripts", pythonExe);

            if (!File.Exists(pythonPath))
            {
                string alternativeExe = useWindowlessMode ? "python.exe" : "pythonw.exe";
                string alternativePath = Path.Combine(actualVenvPath, "Scripts", alternativeExe);
                if (File.Exists(alternativePath))
                {
                    return alternativePath;
                }
                return string.Empty;
            }

            return pythonPath;
        }

        public void CloseAllProcesses()
        {
            foreach (var proc in spawnedCmdProcesses.ToList())
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                    spawnedCmdProcesses.Remove(proc);
                }
                catch
                {
                    spawnedCmdProcesses.Remove(proc);
                }
            }
        }

        private void ShowInformationDialog(string title, string content)
        {
            var infoWindow = new InfoWindow(title, content);
            infoWindow.Owner = Application.Current.MainWindow;
            infoWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            infoWindow.ShowDialog();
        }
    }
}