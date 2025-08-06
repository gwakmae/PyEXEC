// PyExec/App.xaml.cs



using System;

using System.Linq; // 추가: FirstOrDefault()를 사용하기 위해

using System.Windows;

using System.Threading; // Mutex를 사용하기 위해 추가



namespace PyExec

{

    public partial class App : Application

    {

        private static Mutex? _mutex = null; // Mutex 인스턴스를 저장할 변수

        private const string MutexName = "Global\\{YourUniqueAppName}"; // 고유한 Mutex 이름 (애플리케이션 이름 사용)



        protected override void OnStartup(StartupEventArgs e)

        {

            _mutex = new Mutex(true, MutexName, out bool createdNew);



            if (!createdNew)

            {

                // 이미 실행 중인 인스턴스가 있으면 활성화하고 현재 인스턴스 종료

                ActivateExistingInstance();

                Shutdown();

                return; // 추가적인 초기화 방지

            }



            base.OnStartup(e);

        }



        private void ActivateExistingInstance()

        {

            // 이미 실행 중인 윈도우를 찾아서 활성화 (간단한 방법)

            var existingWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (existingWindow != null)

            {

                if (existingWindow.WindowState == WindowState.Minimized)

                {

                    existingWindow.WindowState = WindowState.Normal;

                }

                existingWindow.Activate();

            }





            // 더 정교한 방법 (프로세스 찾기, 윈도우 핸들 사용 등)은 필요에 따라 구현

            // 예: FindWindow, SetForegroundWindow API 사용 (System.Diagnostics, System.Runtime.InteropServices 사용)

        }

        protected override void OnExit(ExitEventArgs e)

        {

            try

            {

                // Mutex 해제 (소유한 경우에만)

                if (_mutex != null)

                {

                    // Dispose만 호출하고 ReleaseMutex는 호출하지 않음

                    _mutex.Dispose();

                    _mutex = null;

                }

            }

            catch

            {

                // 오류 무시 (변수 선언 없이)

            }



            base.OnExit(e);

        }

    }

}