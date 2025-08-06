// PyExec/Helpers/ShortcutKeyHandler.cs

using System.Windows.Input;
using PyExec.Models;
using PyExec.ViewModels; // MainViewModel을 사용하기 위해 추가
using System;

namespace PyExec.Helpers
{
    public class ShortcutKeyHandler
    {
        private readonly MainWindow _mainWindow;
        private readonly ProgramRunner _programRunner;
        public ShortcutKeyHandler(MainWindow mainWindow, ProgramRunner programRunner)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
            _programRunner = programRunner ?? throw new ArgumentNullException(nameof(programRunner));
        }

        public void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                RunSelectedProgram();
                e.Handled = true; // 이벤트 처리 완료
            }
        }
        private void RunSelectedProgram()
        {
            // MainWindow의 DataContext를 MainViewModel로 가져옵니다.
            if (_mainWindow.DataContext is MainViewModel vm)
            {
                // 활성 패널(ActivePanelVM)에서 선택된 프로그램을 가져옵니다.
                var selectedProgram = vm.ActivePanelVM.SelectedProgram;
                if (selectedProgram != null)
                {
                    _programRunner.Run(selectedProgram); // ProgramRunner 사용
                }
            }
        }
    }
}