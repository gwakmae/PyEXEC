// PyExec/ViewModels/ProgramPanelViewModel.cs
using PyExec.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Windows;

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
            set { _selectedProgram = value; OnPropertyChanged(); }
        }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand RunProgramCommand { get; }

        public ProgramPanelViewModel(ProgramListManager manager, ProgramRunner runner, string filePath)
        {
            _programListManager = manager;
            _programRunner = runner;
            FilePath = filePath;

            MoveUpCommand = new RelayCommand(ExecuteMoveUp, CanMove);
            MoveDownCommand = new RelayCommand(ExecuteMoveDown, CanMove);
            RunProgramCommand = new RelayCommand(ExecuteRunProgram, (p) => SelectedProgram != null);
            
            LoadPrograms();
        }

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
    }
}