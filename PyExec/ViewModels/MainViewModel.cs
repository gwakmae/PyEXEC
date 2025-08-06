// PyExec/ViewModels/MainViewModel.cs
using PyExec.Models;
using PyExec.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;

namespace PyExec.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly ProgramListManager _programListManager;
        private readonly TemplateManager _templateManager;
        private readonly VirtualEnvironmentManager _venvManager;
        private readonly PythonExecutor _pythonExecutor;
        #endregion

        #region Properties
        public ProgramPanelViewModel Panel1VM { get; }
        public ProgramPanelViewModel Panel2VM { get; }

        private ProgramPanelViewModel _activePanelVM;
        public ProgramPanelViewModel ActivePanelVM
        {
            get => _activePanelVM;
            set => SetField(ref _activePanelVM, value);
        }

        public ObservableCollection<string> RecentTemplates { get; } = new();

        private string? _selectedRecentTemplate;
        public string? SelectedRecentTemplate
        {
            get => _selectedRecentTemplate;
            set => SetField(ref _selectedRecentTemplate, value);
        }

        private string _virtualEnvInfo = string.Empty;
        public string VirtualEnvInfo
        {
            get => _virtualEnvInfo;
            set => SetField(ref _virtualEnvInfo, value);
        }

        // ### [수정] 기본값을 false로 변경 ###
        private bool _showScriptOutput = false;
        public bool ShowScriptOutput
        {
            get => _showScriptOutput;
            set
            {
                if (SetField(ref _showScriptOutput, value))
                {
                    _pythonExecutor.SetShowScriptOutput(value);
                }
            }
        }
        #endregion

        #region Commands
        // File Menu Commands
        public ICommand AddProgramCommand { get; }
        public ICommand RemoveProgramCommand { get; }
        public ICommand NewTemplateCommand { get; }
        public ICommand SaveTemplateCommand { get; }
        public ICommand OverwriteTemplateCommand { get; } // ### [추가] 템플릿 덮어쓰기 커맨드
        public ICommand LoadTemplateCommand { get; }
        public ICommand LoadSpecificRecentTemplateCommand { get; }
        public ICommand RemoveRecentTemplateCommand { get; }
        public ICommand MoveTemplateUpCommand { get; }
        public ICommand MoveTemplateDownCommand { get; }
        public ICommand ExitCommand { get; }

        // Tools Menu Commands
        public ICommand SetDefaultVenvCommand { get; }
        public ICommand SetVenvForSelectedCommand { get; }
        public ICommand SetRunUvCommand { get; }
        public ICommand SetRunPythonCommand { get; }
        public ICommand OpenCmdDefaultVenvCommand { get; }
        public ICommand OpenCmdSelectedVenvCommand { get; }
        public ICommand ConvertPyPywCommand { get; }
        public ICommand ApplyNameToDescriptionCommand { get; }
        public ICommand ApplyFolderNameToDescriptionCommand { get; }
        public ICommand ShowProcessStartInfoCommand { get; }

        // Help Menu Command
        public ICommand AboutCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel()
        {
            // Managers and Services Initialization
            _venvManager = new VirtualEnvironmentManager();
            _programListManager = new ProgramListManager(_venvManager);
            _pythonExecutor = new PythonExecutor(_venvManager);
            var programRunner = new ProgramRunner(_pythonExecutor);
            _templateManager = new TemplateManager();

            // ViewModels Initialization
            // 변경: 하드코딩된 문자열 대신 상수 사용
            Panel1VM = new ProgramPanelViewModel(_programListManager, programRunner, AppConstants.Panel1ProgramsFile);
            Panel2VM = new ProgramPanelViewModel(_programListManager, programRunner, AppConstants.Panel2ProgramsFile);
            _activePanelVM = Panel1VM;

            // Command Initialization
            AddProgramCommand = new RelayCommand(ExecuteAddProgram);
            RemoveProgramCommand = new RelayCommand(ExecuteRemoveProgram, CanExecuteOnSelectedProgram);
            NewTemplateCommand = new RelayCommand(ExecuteNewTemplate);
            SaveTemplateCommand = new RelayCommand(ExecuteSaveTemplate);
            // ### [추가] 템플릿 덮어쓰기 커맨드 초기화. 'SelectedRecentTemplate'가 null이 아닐 때만 활성화됩니다.
            OverwriteTemplateCommand = new RelayCommand(ExecuteOverwriteTemplate, p => SelectedRecentTemplate != null);
            LoadTemplateCommand = new RelayCommand(ExecuteLoadTemplate);
            LoadSpecificRecentTemplateCommand = new RelayCommand(ExecuteLoadSpecificRecentTemplate, p => SelectedRecentTemplate != null);
            RemoveRecentTemplateCommand = new RelayCommand(ExecuteRemoveRecentTemplate, p => SelectedRecentTemplate != null);
            MoveTemplateUpCommand = new RelayCommand(ExecuteMoveTemplateUp, p => SelectedRecentTemplate != null && RecentTemplates.IndexOf(SelectedRecentTemplate) > 0);
            MoveTemplateDownCommand = new RelayCommand(ExecuteMoveTemplateDown, p => SelectedRecentTemplate != null && RecentTemplates.IndexOf(SelectedRecentTemplate) < RecentTemplates.Count - 1);
            ExitCommand = new RelayCommand(ExecuteExit);
            AboutCommand = new RelayCommand(ExecuteAbout);

            SetDefaultVenvCommand = new RelayCommand(ExecuteSetDefaultVenv);
            SetVenvForSelectedCommand = new RelayCommand(ExecuteSetVenvForSelected, CanExecuteOnSelectedProgram);
            SetRunUvCommand = new RelayCommand(p => ExecuteSetExecutionMethod(true), CanExecuteOnSelectedProgram);
            SetRunPythonCommand = new RelayCommand(p => ExecuteSetExecutionMethod(false), CanExecuteOnSelectedProgram);
            OpenCmdDefaultVenvCommand = new RelayCommand(ExecuteOpenCmdDefaultVenv);
            OpenCmdSelectedVenvCommand = new RelayCommand(ExecuteOpenCmdSelectedVenv, CanExecuteOnSelectedProgram);
            ConvertPyPywCommand = new RelayCommand(ExecuteConvertPyPyw, CanExecuteOnSelectedProgram);
            ApplyNameToDescriptionCommand = new RelayCommand(ExecuteApplyNameToDescription, CanExecuteOnSelectedProgram);
            ApplyFolderNameToDescriptionCommand = new RelayCommand(ExecuteApplyFolderNameToDescription, CanExecuteOnSelectedProgram);
            ShowProcessStartInfoCommand = new RelayCommand(async p => await ExecuteShowProcessStartInfoAsync(), CanExecuteOnSelectedProgram);

            // Initial State Setup
            _pythonExecutor.SetShowScriptOutput(ShowScriptOutput);
            UpdateRecentTemplates();
            UpdateVirtualEnvInfo();
        }
        #endregion

        #region General Methods
        private bool CanExecuteOnSelectedProgram(object? parameter)
        {
            return ActivePanelVM?.SelectedProgram != null;
        }

        public void SaveAllSettings()
        {
            Panel1VM.SavePrograms();
            Panel2VM.SavePrograms();
            _venvManager.SaveDefaultVirtualEnvRoot();
        }
        #endregion
    }
}