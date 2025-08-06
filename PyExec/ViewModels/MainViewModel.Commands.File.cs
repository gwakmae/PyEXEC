// PyExec/ViewModels/MainViewModel.Commands.File.cs
using PyExec.Helpers;
using System.IO; // ### [추가] Path 클래스 사용을 위해
using System.Windows;

namespace PyExec.ViewModels
{
    public partial class MainViewModel
    {
        // --- File & Template Command Implementations ---

        private void ExecuteAddProgram(object? parameter)
        {
            if (_programListManager.AddProgram(ActivePanelVM.Programs))
            {
                ActivePanelVM.SavePrograms();
            }
        }

        private void ExecuteRemoveProgram(object? parameter)
        {
            if (ActivePanelVM.SelectedProgram != null)
            {
                _programListManager.RemovePrograms(ActivePanelVM.Programs, new() { ActivePanelVM.SelectedProgram });
                ActivePanelVM.SavePrograms();
            }
        }

        private void ExecuteNewTemplate(object? parameter)
        {
            ActivePanelVM.Programs.Clear();
            ActivePanelVM.SavePrograms();
        }

        private void ExecuteSaveTemplate(object? parameter)
        {
            // 이 메서드는 "다른 이름으로 저장"을 담당하며, 변경되지 않습니다.
            _templateManager.SaveTemplate(ActivePanelVM.Programs);
            UpdateRecentTemplates();
        }

        // ### [추가] 템플릿 덮어쓰기 실행 로직 (사용자 확인 포함) ###
        private void ExecuteOverwriteTemplate(object? parameter)
        {
            // 이 커맨드는 SelectedRecentTemplate가 null이 아닐 때만 활성화되지만, 안전을 위해 한 번 더 확인합니다.
            if (SelectedRecentTemplate == null) return;

            // 사용자에게 덮어쓰기를 확인할 대화 상자를 표시합니다.
            var result = MessageBox.Show(
                $"현재 프로그램 목록을 선택된 템플릿에 덮어쓰시겠습니까?\n\n파일: {Path.GetFileName(SelectedRecentTemplate)}",
                "템플릿 덮어쓰기 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // 사용자가 '예'를 선택한 경우에만 저장 로직을 실행합니다.
            if (result == MessageBoxResult.Yes)
            {
                // 활성 패널의 프로그램 목록을 사용하여 템플릿 매니저의 덮어쓰기 메서드 호출
                _templateManager.OverwriteTemplate(SelectedRecentTemplate, ActivePanelVM.Programs);
            }
        }

        private void ExecuteLoadTemplate(object? parameter)
        {
            _templateManager.LoadTemplate(ActivePanelVM.Programs, _programListManager);
            ActivePanelVM.SavePrograms();
            UpdateRecentTemplates();
        }

        private void ExecuteLoadSpecificRecentTemplate(object? parameter)
        {
            if (SelectedRecentTemplate != null)
            {
                _templateManager.LoadTemplateFromFile(SelectedRecentTemplate, ActivePanelVM.Programs, _programListManager);
                ActivePanelVM.SavePrograms();
            }
        }

        private void ExecuteRemoveRecentTemplate(object? parameter)
        {
            if (SelectedRecentTemplate != null)
            {
                _templateManager.RemoveRecentTemplate(SelectedRecentTemplate);
                UpdateRecentTemplates();
            }
        }

        private void ExecuteMoveTemplateUp(object? parameter)
        {
            if (SelectedRecentTemplate != null)
            {
                _templateManager.MoveTemplateUp(RecentTemplates, new[] { SelectedRecentTemplate });
            }
        }

        private void ExecuteMoveTemplateDown(object? parameter)
        {
            if (SelectedRecentTemplate != null)
            {
                _templateManager.MoveTemplateDown(RecentTemplates, new[] { SelectedRecentTemplate });
            }
        }

        private void ExecuteExit(object? parameter)
        {
            Application.Current.Shutdown();
        }

        private void ExecuteAbout(object? parameter)
        {
            Utility.ShowAboutMessage();
        }

        // --- Helper Method for File/Template Commands ---

        private void UpdateRecentTemplates()
        {
            RecentTemplates.Clear();
            _templateManager.GetRecentTemplates().ForEach(RecentTemplates.Add);
        }
    }
}