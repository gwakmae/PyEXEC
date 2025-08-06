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
            // ... (초반 null 및 파일 존재 체크는 동일) ...
            if (program is null) { /*...*/ return; }
            string programPath = program.Path;
            if (string.IsNullOrEmpty(programPath) || !File.Exists(programPath)) { /*...*/ return; }
            string ext = Path.GetExtension(programPath).ToLowerInvariant();


            try
            {
                if (ext == ".py" || ext == ".pyw")
                {
                    // 비동기 메서드 호출 (기다리지 않음 - fire and forget)
                    // 오류는 ExecutePythonScriptAsync 내부에서 처리하고 로그 표시
                    _ = _pythonExecutor.ExecutePythonScriptAsync(program); // useWindowlessMode 제거됨
                }
                else if (ext == ".exe")
                {
                    // ... (기존 .exe 실행 로직 - 문제 없음) ...
                    string? workingDirectory = Path.GetDirectoryName(programPath);
                    if (workingDirectory == null) { /*...*/ return; }
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
                    // --- 수정된 부분: 기타 확장자 처리 ---
                    var result = MessageBox.Show($"지원되지 않는 파일 형식({ext})입니다.\nWindows 기본 연결 프로그램으로 실행을 시도하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // ProcessStartInfo 생성 및 Process.Start 호출을 if 블록 안으로 이동
                        try // ShellExecute 오류 가능성 대비 try-catch 추가
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo
                            {
                                FileName = programPath,
                                UseShellExecute = true // ShellExecute 사용
                            };
                            Process.Start(startInfo);
                        }
                        catch (Exception shellEx) // Win32Exception 등 ShellExecute 관련 오류 처리
                        {
                            MessageBox.Show($"연결 프로그램 실행 중 오류 발생: {shellEx.Message}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    // ----------------------------------
                }
            }
            // --- 예외 처리 블록 (기존과 유사하게 유지) ---
            catch (Win32Exception ex) // Process.Start(.exe 등) 또는 ShellExecute 관련 오류
            {
                MessageBox.Show($"프로그램 실행 중 오류 발생 (Win32): {ex.Message}\n\n파일: {programPath}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FileNotFoundException) // 파일 못 찾는 경우
            {
                MessageBox.Show($"프로그램 파일을 찾을 수 없습니다:\n{programPath}", "파일 없음", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex) // 기타 예외
            {
                MessageBox.Show($"프로그램 실행 중 예상치 못한 오류 발생: {ex.Message}\n\n파일: {programPath}", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}