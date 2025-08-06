// PyExec/ViewModels/MainViewModel.Commands.File.cs
using PyExec.Helpers;
using System.IO; // ### [�߰�] Path Ŭ���� ����� ����
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
            // �� �޼���� "�ٸ� �̸����� ����"�� ����ϸ�, ������� �ʽ��ϴ�.
            _templateManager.SaveTemplate(ActivePanelVM.Programs);
            UpdateRecentTemplates();
        }

        // ### [�߰�] ���ø� ����� ���� ���� (����� Ȯ�� ����) ###
        private void ExecuteOverwriteTemplate(object? parameter)
        {
            // �� Ŀ�ǵ�� SelectedRecentTemplate�� null�� �ƴ� ���� Ȱ��ȭ������, ������ ���� �� �� �� Ȯ���մϴ�.
            if (SelectedRecentTemplate == null) return;

            // ����ڿ��� ����⸦ Ȯ���� ��ȭ ���ڸ� ǥ���մϴ�.
            var result = MessageBox.Show(
                $"���� ���α׷� ����� ���õ� ���ø��� ����ðڽ��ϱ�?\n\n����: {Path.GetFileName(SelectedRecentTemplate)}",
                "���ø� ����� Ȯ��",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // ����ڰ� '��'�� ������ ��쿡�� ���� ������ �����մϴ�.
            if (result == MessageBoxResult.Yes)
            {
                // Ȱ�� �г��� ���α׷� ����� ����Ͽ� ���ø� �Ŵ����� ����� �޼��� ȣ��
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