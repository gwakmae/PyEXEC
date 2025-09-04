// PyExec/Models/Program.cs

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic; // Added for generic collections
using System.IO; // Path 클래스 사용을 위해 추가
using System; // StringComparison 사용을 위해 추가

namespace PyExec.Models
{
    public class Program : INotifyPropertyChanged
    {
        private int _order;
        private string _name = "";
        private string _description = "";
        private string _category = "";
        private string _path = "";
        private string _virtualEnvPath = "";
        private string _displayVirtualEnvPath = ""; // UI 표시용
        private bool _useUvRun; // 추가: uv run 사용 여부
        private bool _runInCmd; // ### [추가] CMD에서 실행 여부

        public int Order
        {
            get { return _order; }
            set { SetField(ref _order, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetField(ref _name, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }

        public string Category
        {
            get { return _category; }
            set { SetField(ref _category, value); }
        }

        public string Path
        {
            get { return _path; }
            set
            {
                // 경로가 변경되면 RunnerDisplay도 업데이트해야 하므로 속성 변경 알림을 호출합니다.
                if (SetField(ref _path, value))
                {
                    OnPropertyChanged(nameof(RunnerDisplay));
                    OnPropertyChanged(nameof(DisplayVirtualEnvPath)); // ### [추가] Path가 바뀌면 DisplayVirtualEnvPath도 갱신
                }
            }
        }

        public string VirtualEnvPath
        {
            get { return _virtualEnvPath; }
            set
            {
                if (SetField(ref _virtualEnvPath, value))
                {
                    OnPropertyChanged(nameof(DisplayVirtualEnvPath)); // ### [추가] VirtualEnvPath가 바뀌면 DisplayVirtualEnvPath도 갱신
                }
            }
        }

        // ### [수정] UI 표시용 가상 환경 경로 속성 ###
        public string DisplayVirtualEnvPath
        {
            get
            {
                // RunnerDisplay가 "exe"이면 가상 환경 경로를 표시하지 않음
                if (RunnerDisplay.Equals("exe", StringComparison.OrdinalIgnoreCase))
                {
                    return "N/A"; // 또는 string.Empty;
                }
                return _displayVirtualEnvPath; // .exe가 아니면 기존 값 표시
            }
            set { SetField(ref _displayVirtualEnvPath, value); }
        }

        // uv run 사용 여부를 결정하는 속성
        public bool UseUvRun
        {
            get { return _useUvRun; }
            set
            {
                if (SetField(ref _useUvRun, value))
                {
                    if (value) RunInCmd = false; // ### [추가] 상호 배제를 위해 RunInCmd를 false로 설정
                    OnPropertyChanged(nameof(RunnerDisplay)); // UI 업데이트를 위해 RunnerDisplay 속성 변경 알림
                    OnPropertyChanged(nameof(DisplayVirtualEnvPath)); // ### [추가] UseUvRun이 바뀌면 DisplayVirtualEnvPath도 갱신
                }
            }
        }

        // ### [추가] CMD 창에서 직접 실행할지 여부를 결정하는 속성 ###
        public bool RunInCmd
        {
            get { return _runInCmd; }
            set
            {
                if (SetField(ref _runInCmd, value))
                {
                    if (value) UseUvRun = false; // ### [추가] 상호 배제를 위해 UseUvRun을 false로 설정
                    OnPropertyChanged(nameof(RunnerDisplay));
                }
            }
        }

        // DataGrid에 표시될 실행기 유형 (수정된 로직)
        public string RunnerDisplay
        {
            get
            {
                // 1. .exe 파일인 경우 'exe'를 최우선으로 표시합니다.
                if (!string.IsNullOrEmpty(Path) && System.IO.Path.GetExtension(Path).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return "exe";
                }

                // ### [추가] 2. RunInCmd가 true이면 'cmd'를 표시합니다. ###
                if (RunInCmd)
                {
                    return "cmd";
                }

                // 3. .exe가 아니면 UseUvRun 설정에 따라 'uv run' 또는 'python'을 표시합니다.
                return UseUvRun ? "uv run" : "python";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}