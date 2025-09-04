// PyExec/Models/ProgramRunner.cs
using PyExec.Helpers;
using PyExec.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.ComponentModel;
using System.Threading.Tasks;

namespace PyExec.Models
{
    public class ProgramRunner
    {
        private readonly PythonExecutor _pythonExecutor;

        public ProgramRunner(PythonExecutor pythonExecutor)
        {
            _pythonExecutor = pythonExecutor ?? throw new ArgumentNullException(nameof(pythonExecutor));
        }

        public void Run(Program? program)
        {
            if (program is null) { return; }
            string programPath = program.Path;
            if (string.IsNullOrEmpty(programPath) || !File.Exists(programPath)) { return; }
            string ext = Path.GetExtension(programPath).ToLowerInvariant();


            try
            {
                // ### [추가] CMD 실행 모드가 우선 순위를 가집니다. ###
                if (program.RunInCmd && (ext == ".py" || ext == ".pyw"))
                {
                    _pythonExecutor.ExecuteInCmd(program);
                    return; // 실행 후 종료
                }

                if (ext == ".py" || ext == ".pyw")
                {
                    // 비동기 메서드 호출 (기다리지 않음 - fire and forget)
                    // 오류는 ExecutePythonScriptAsync 내부에서 처리하고 로그 표시
                    _ = _pythonExecutor.ExecutePythonScriptAsync(program);
                }
                else if (ext == ".exe")
                {
                    string? workingDirectory = Path.GetDirectoryName(programPath);
                    if (workingDirectory == null) { return; }
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = programPath,
                        WorkingDirectory = workingDirectory,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
                else
                {
                    var result = MessageBox.Show($"지원되지 않는 파일 형식({ext})입니다.\nWindows 기본 연결 프로그램으로 실행을 시도하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = programPath,
                                UseShellExecute = true
                            };
                            Process.Start(startInfo);
                        }
                        catch (Exception shellEx)
                        {
                            MessageBox.Show($"연결 프로그램 실행 중 오류 발생: {shellEx.Message}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show($"프로그램 실행 중 오류 발생 (Win32): {ex.Message}\n\n파일: {programPath}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show($"프로그램 파일을 찾을 수 없습니다:\n{programPath}", "파일 없음", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"프로그램 실행 중 예상치 못한 오류 발생: {ex.Message}\n\n파일: {programPath}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}