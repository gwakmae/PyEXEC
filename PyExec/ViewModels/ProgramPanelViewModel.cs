// PyExec/ViewModels/ProgramPanelViewModel.cs
using PyExec.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PyExec.ViewModels
{
    public class ProgramPanelViewModel : ViewModelBase
    {
        private readonly ProgramListManager _programListManager;
        private readonly ProgramRunner _programRunner;
        private Program? _selectedProgram;

        public string FilePath { get; }
        public ObservableCollection<Program> Programs { get; } = new();

        public Program? SelectedProgram
        {
            get => _selectedProgram;
            set => SetField(ref _selectedProgram, value); // [수정] OnPropertyChanged() → SetField 사용
        }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand RunProgramCommand { get; }

        // 컨텍스트 메뉴용 명령들
        public ICommand OpenFolderCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand CopyPathCommand { get; }
        public ICommand SelectInExplorerCommand { get; }

        public ProgramPanelViewModel(ProgramListManager manager, ProgramRunner runner, string filePath)
        {
            _programListManager = manager;
            _programRunner = runner;
            FilePath = filePath;

            MoveUpCommand = new RelayCommand(ExecuteMoveUp, CanMove);
            MoveDownCommand = new RelayCommand(ExecuteMoveDown, CanMove);
            RunProgramCommand = new RelayCommand(ExecuteRunProgram, (p) => SelectedProgram != null);

            OpenFolderCommand = new RelayCommand(ExecuteOpenFolder, CanExecuteOnProgram);
            OpenFileCommand = new RelayCommand(ExecuteOpenFile, CanExecuteOnProgram);
            CopyPathCommand = new RelayCommand(ExecuteCopyPath, CanExecuteOnProgram);
            SelectInExplorerCommand = new RelayCommand(ExecuteSelectInExplorer, CanExecuteOnProgram);

            LoadPrograms();
        }

        // [수정] Program 타입이고, Path가 null/빈값이 아닐 때만 명령 실행 허용
        private bool CanExecuteOnProgram(object? parameter) => parameter is Program prog && !string.IsNullOrEmpty(prog.Path);

        public void LoadPrograms() => _programListManager.LoadProgramList(Programs, FilePath);
        public void SavePrograms() => _programListManager.SaveProgramList(Programs, FilePath);

        private bool CanMove(object? parameter) => SelectedProgram != null;

        private void ExecuteMoveUp(object? parameter)
        {
            if (SelectedProgram == null) return;
            _programListManager.MoveUp(Programs, SelectedProgram);
            SavePrograms();
        }

        private void ExecuteMoveDown(object? parameter)
        {
            if (SelectedProgram == null) return;
            _programListManager.MoveDown(Programs, SelectedProgram);
            SavePrograms();
        }

        private void ExecuteRunProgram(object? parameter)
        {
            if (SelectedProgram != null)
            {
                _programRunner.Run(SelectedProgram);
            }
            else
            {
                MessageBox.Show("실행할 프로그램을 선택해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 컨텍스트 메뉴 명령 실행 메서드들
        private void ExecuteOpenFolder(object? parameter)
        {
            if (parameter is not Program prog) return;
            try
            {
                string? dir = Path.GetDirectoryName(prog.Path);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", dir));
                }
                else
                {
                    MessageBox.Show("폴더를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"폴더를 여는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ExecuteOpenFile(object? parameter)
        {
            if (parameter is not Program prog) return;
            try
            {
                if (File.Exists(prog.Path))
                {
                    string ext = Path.GetExtension(prog.Path).ToLowerInvariant();
                    // .py, .pyw 파일은 내부 실행 로직(ProgramRunner)을 따름
                    if (ext == ".py" || ext == ".pyw")
                    {
                        _programRunner.Run(prog);
                    }
                    else // 그 외 파일(.exe, .ipynb 등)은 Windows 기본 연결 프로그램으로 실행
                    {
                        Process.Start(new ProcessStartInfo(prog.Path) { UseShellExecute = true });
                    }
                }
                else
                {
                    MessageBox.Show("파일을 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex) { MessageBox.Show($"파일을 실행하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ExecuteCopyPath(object? parameter)
        {
            if (parameter is not Program prog) return;
            try
            {
                Clipboard.SetText(prog.Path);
            }
            catch (Exception ex) { MessageBox.Show($"경로를 복사하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ExecuteSelectInExplorer(object? parameter)
        {
            if (parameter is not Program prog) return;
            try
            {
                if (File.Exists(prog.Path))
                {
                    Process.Start("explorer.exe", $"/select, \"{prog.Path}\"");
                }
                else
                {
                    // 파일이 없으면 폴더라도 열어줌
                    ExecuteOpenFolder(parameter);
                }
            }
            catch (Exception ex) { MessageBox.Show($"탐색기에서 파일을 선택하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
