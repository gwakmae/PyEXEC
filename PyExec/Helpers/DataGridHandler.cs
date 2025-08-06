// PyExec/Helpers/DataGridHandler.cs

using System;
using PyExec.Models;
using System.Collections.ObjectModel; // ObservableCollection
using System.Windows.Controls; // DataGrid
using System.Windows.Data; // Binding


namespace PyExec.Helpers
{
    public class DataGridHandler
    {
        private readonly ProgramListManager _programListManager;

        public DataGridHandler(ProgramListManager programListManager)
        {
            _programListManager = programListManager;
        }

        public void ProgramGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e, ObservableCollection<Program> programs)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                // 순서(Order) 열이 변경된 경우에만 ReorderByOrder 호출
                if (e.Column is DataGridTextColumn textColumn && textColumn.Binding is Binding binding && binding.Path.Path == nameof(Program.Order))
                {
                    // UI 스레드에서 비동기적으로 순서 업데이트
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _programListManager.ReorderByOrder(programs);
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
                // 저장 로직은 MainWindow.xaml.cs에서 처리하므로 여기서 제거
            }
        }
        // DataGrid 로드 시 가상 환경 경로 표시
        public void ProgramGrid_LoadingRow(object? sender, DataGridRowEventArgs e)
        {
            if (e.Row.DataContext is Program program)
            {
                program.DisplayVirtualEnvPath = UIHelper.GetDisplayVirtualEnvPath(program, _programListManager.GetDefaultVirtualEnvRoot());
            }
        }
    }
}
