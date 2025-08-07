// PyExec/App.xaml.cs
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace PyExec
{
    public partial class App : Application
    {
        private static Mutex? _mutex;
        private const string MutexName = @"Global\PyExec_SingleInstance_FINAL_TEST";

        /* ── Win32 API DECLARATIONS ───────────────────────────────────────── */

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        // ### [추가] 더 나은 창 찾기를 위한 API ###
        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /* ── CONSTANTS ────────────────────────────────────────────────────── */

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOWNORMAL = 1;

        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        private const uint GW_OWNER = 4;
        private const int GWL_STYLE = -16;
        private const uint WS_VISIBLE = 0x10000000;

        /* ── START-UP / SHUTDOWN ──────────────────────────────────────────── */

        protected override void OnStartup(StartupEventArgs e)
        {
            Debug.WriteLine("[PyExec] OnStartup");

            _mutex = new Mutex(true, MutexName, out bool isNew);
            if (!isNew)
            {
                Debug.WriteLine("기존 인스턴스가 감지되었습니다 - 활성화를 시도합니다.");
                bool activated = ActivateExistingInstance();
                if (activated)
                {
                    Debug.WriteLine("기존 인스턴스가 성공적으로 활성화되었습니다.");
                }
                else
                {
                    Debug.WriteLine("기존 인스턴스 활성화에 실패했습니다.");
                }
                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.Dispose();
            base.OnExit(e);
        }

        /* ── LOCATE EXISTING INSTANCE ────────────────────────────────────── */

        private bool ActivateExistingInstance()
        {
            var self = Process.GetCurrentProcess();
            IntPtr foundWindow = IntPtr.Zero;

            // 1) 먼저 프로세스 목록에서 찾기
            foreach (var p in Process.GetProcessesByName(self.ProcessName)
                                     .Where(x => x.Id != self.Id && !x.HasExited))
            {
                try
                {
                    // MainWindowHandle가 0이 아닌 경우 사용
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        foundWindow = p.MainWindowHandle;
                        Debug.WriteLine($"프로세스 MainWindowHandle로 창을 찾았습니다: {foundWindow}");
                        break;
                    }

                    // MainWindowHandle가 0인 경우, 프로세스의 모든 창 검색
                    p.Refresh(); // 프로세스 정보 새로고침
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        foundWindow = p.MainWindowHandle;
                        Debug.WriteLine($"프로세스 새로고침 후 창을 찾았습니다: {foundWindow}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"프로세스 검색 중 오류: {ex.Message}");
                }
            }

            // 2) 프로세스에서 못 찾았으면 창 제목으로 찾기
            if (foundWindow == IntPtr.Zero)
            {
                foundWindow = FindWindow(null, "PyExec - Script Manager");
                if (foundWindow != IntPtr.Zero)
                {
                    Debug.WriteLine($"FindWindow로 창을 찾았습니다: {foundWindow}");
                }
            }

            // 3) 그래도 못 찾았으면 EnumWindows로 모든 창 검색
            if (foundWindow == IntPtr.Zero)
            {
                EnumWindows((hWnd, _) =>
                {
                    // 창이 보이는지 확인
                    if (!IsWindowVisible(hWnd)) return true;

                    // 프로세스 ID 확인
                    GetWindowThreadProcessId(hWnd, out int windowPid);

                    // 같은 프로세스 이름을 가진 프로세스인지 확인
                    try
                    {
                        var proc = Process.GetProcessById(windowPid);
                        if (proc.ProcessName == self.ProcessName && proc.Id != self.Id)
                        {
                            // 창 제목 확인
                            int len = GetWindowTextLength(hWnd);
                            if (len > 0)
                            {
                                var sb = new System.Text.StringBuilder(len + 1);
                                GetWindowText(hWnd, sb, sb.Capacity);
                                string title = sb.ToString();

                                if (title.Contains("PyExec", StringComparison.OrdinalIgnoreCase))
                                {
                                    Debug.WriteLine($"EnumWindows로 창을 찾았습니다: {hWnd}, 제목: {title}");
                                    foundWindow = hWnd;
                                    return false; // 열거 중단
                                }
                            }
                        }
                    }
                    catch { }

                    return true; // 계속 열거
                }, IntPtr.Zero);
            }

            // 창을 찾았으면 활성화
            if (foundWindow != IntPtr.Zero)
            {
                return ActivateWindow(foundWindow);
            }

            Debug.WriteLine("기존 인스턴스의 창을 찾을 수 없습니다.");
            return false;
        }

        /* ── BRING WINDOW TO FRONT ───────────────────────────────────────── */

        private bool ActivateWindow(IntPtr hWnd)
        {
            try
            {
                Debug.WriteLine($"창 활성화 시도: {hWnd}");

                // 창이 유효한지 확인
                if (!IsWindowEnabled(hWnd))
                {
                    Debug.WriteLine("창이 비활성화 상태입니다.");
                    return false;
                }

                // 소유자 창이 있는지 확인
                IntPtr owner = GetWindow(hWnd, GW_OWNER);
                if (owner != IntPtr.Zero)
                {
                    Debug.WriteLine($"소유자 창이 있습니다: {owner}");
                    ActivateWindow(owner);
                }

                // 1. 창이 최소화되어 있으면 복원
                if (IsIconic(hWnd))
                {
                    Debug.WriteLine("창이 최소화되어 있습니다. 복원합니다.");
                    ShowWindow(hWnd, SW_RESTORE);
                    System.Threading.Thread.Sleep(100);
                }

                // 2. 창을 보이게 함
                ShowWindow(hWnd, SW_SHOW);
                ShowWindow(hWnd, SW_SHOWNORMAL);

                // 3. 스레드 입력 연결
                int targetTid = GetWindowThreadProcessId(hWnd, out _);
                int currentTid = GetCurrentThreadId();
                bool attached = false;

                if (targetTid != currentTid)
                {
                    attached = AttachThreadInput(currentTid, targetTid, true);
                    Debug.WriteLine($"스레드 입력 연결: {attached}");
                }

                // 4. 창을 최상위로 올리고 다시 일반으로
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                System.Threading.Thread.Sleep(50);
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

                // 5. 창을 맨 앞으로
                BringWindowToTop(hWnd);

                // 6. 포커스 설정
                bool foregroundResult = SetForegroundWindow(hWnd);
                Debug.WriteLine($"SetForegroundWindow 결과: {foregroundResult}");

                // 7. 스레드 입력 연결 해제
                if (attached)
                {
                    AttachThreadInput(currentTid, targetTid, false);
                }

                // 8. 확인 및 추가 시도
                System.Threading.Thread.Sleep(100);
                IntPtr currentForeground = GetForegroundWindow();

                if (currentForeground != hWnd)
                {
                    Debug.WriteLine("창이 전경으로 오지 않았습니다. 다시 시도합니다.");

                    // 한 번 더 시도
                    FlashWindow(hWnd, true);
                    System.Threading.Thread.Sleep(100);
                    FlashWindow(hWnd, false);

                    // Alt 키를 이용한 활성화 트릭
                    System.Windows.Forms.SendKeys.SendWait("%");
                    SetForegroundWindow(hWnd);
                }

                Debug.WriteLine($"창 활성화 완료. 현재 전경 창: {currentForeground}, 대상 창: {hWnd}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ActivateWindow 오류: {ex.Message}");
                return false;
            }
        }
    }
}
