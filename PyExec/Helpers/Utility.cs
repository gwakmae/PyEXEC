// PyExec/Helpers/Utility.cs



using System;

using System.Diagnostics;

using System.IO;

using System.Linq;

using System.Windows; // MessageBox



namespace PyExec.Helpers

{

    public static class Utility

    {

        // Python 설치 페이지 열기

        public static void InstallPython()

        {

            Process.Start(new ProcessStartInfo

            {

                FileName = "https://www.python.org/downloads/",

                UseShellExecute = true

            });

        }



        // Python을 시스템 PATH에 추가

        public static void AddPythonToPath()

        {

            try

            {

                var pythonPath = GetPythonInstallPath();

                if (string.IsNullOrEmpty(pythonPath))

                {

                    MessageBox.Show("Python installation not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;

                }



                var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";

                if (!userPath.Contains(pythonPath))

                {

                    Environment.SetEnvironmentVariable(

                        "PATH",

                        userPath + ";" + pythonPath,

                        EnvironmentVariableTarget.User

                    );

                    MessageBox.Show("Python added to PATH successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }

                else

                {

                    MessageBox.Show("Python is already in PATH.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                }

            }

            catch (Exception ex)

            {

                MessageBox.Show($"Error adding Python to PATH: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        // Python 설치 경로 가져오기 (레지스트리 사용 X, 환경변수와 일반적인 경로 확인)

        private static string GetPythonInstallPath()

        {

            // 1. PYTHONHOME 환경 변수 확인

            var pythonHome = Environment.GetEnvironmentVariable("PYTHONHOME");

            if (!string.IsNullOrEmpty(pythonHome))

                return pythonHome;



            // 2. 일반적인 설치 경로 확인

            string[] commonPaths = {

            @"C:\Python39",

            @"C:\Python38",

            @"C:\Python37",

            @"C:\Program Files\Python39",

            @"C:\Program Files\Python38",

            @"C:\Program Files\Python37"

        };

            return commonPaths.FirstOrDefault(Directory.Exists) ?? string.Empty;

        }



        // About 메시지 표시

        public static void ShowAboutMessage()

        {

            MessageBox.Show(

                "PyExec\nVersion 1.0\n\nA tool for managing and running Python programs.",

                "About",

                MessageBoxButton.OK,

                MessageBoxImage.Information

            );

        }

    }

}