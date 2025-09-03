using PyExec.Models; // 추가
using PyExec.ViewModels;
using System; // 추가
using System.Diagnostics; // 추가
using System.IO; // 추가
using System.Text.Json; // 추가
using System.Windows;
using System.Windows.Input;
using PyExec.Helpers;          // ### [추가] UIHelper 사용을 위해
using System.Windows.Controls;    // ### [추가] DataGridCell 사용을 위해

namespace PyExec
{
    public partial class MainWindow : Window
    {
        // 변경: 하드코딩된 문자열 대신 상수 사용
        private const string WindowSettingsFile = AppConstants.WindowSettingsFile;

        public MainWindow()
        {
            InitializeComponent();
            // 변경: 생성자에서 LoadWindowSettings() 직접 호출 대신 Loaded 이벤트 핸들러 등록
            this.Loaded += MainWindow_Loaded;
        }

        // 변경: Window가 완전히 로드된 후 설정을 적용하여 UI 깨짐 방지
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWindowSettings();
        }

        // --- 창 설정 저장 및 로드 로직 (새로 추가된 부분) ---

        private void LoadWindowSettings()
        {
            if (!File.Exists(WindowSettingsFile)) return;

            try
            {
                var json = File.ReadAllText(WindowSettingsFile);
                var settings = JsonSerializer.Deserialize<WindowSettings>(json);

                if (settings != null)
                {
                    // --- 외부 창 크기 및 위치 복원 (유효성 검사 추가) ---
                    // 화면 밖으로 창이 나가는 것을 방지
                    double screenWidth = SystemParameters.VirtualScreenWidth;
                    double screenHeight = SystemParameters.VirtualScreenHeight;

                    // Width와 Height가 최소 크기보다 작아지는 것을 방지
                    this.Width = Math.Max(this.MinWidth, settings.Width);
                    this.Height = Math.Max(this.MinHeight, settings.Height);

                    // Top과 Left가 화면 영역 내에 있도록 조정
                    this.Left = Math.Max(0, Math.Min(settings.Left, screenWidth - this.Width));
                    this.Top = Math.Max(0, Math.Min(settings.Top, screenHeight - this.Height));

                    // 창 상태는 마지막에 설정
                    this.WindowState = settings.WindowState;

                    // --- 내부 패널 높이 복원 ---
                    TemplatesRow.Height = settings.TemplatesPanelHeight;
                    ProgramView1Row.Height = settings.ProgramPanel1Height;

                    // --- 각 패널의 내부 너비 복원 ---
                    View1_ListColumn.Width = settings.Panel1_ListWidth;
                    View2_ListColumn.Width = settings.Panel2_ListWidth;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading window settings: {ex.Message}");
                // 오류 발생 시 기존 설정 파일 삭제하여 다음 실행 시 기본값으로 시작
                if (File.Exists(WindowSettingsFile))
                {
                    try { File.Delete(WindowSettingsFile); } catch { }
                }
            }
        }

        private void SaveWindowSettings()
        {
            var settings = new WindowSettings();

            // 변경: 현재 창 상태에 따라 저장할 값 결정
            if (this.WindowState == WindowState.Maximized)
            {
                // 최대화 상태일 경우, 최대화 되기 전의 '복원' 크기를 저장
                settings.Top = this.RestoreBounds.Top;
                settings.Left = this.RestoreBounds.Left;
                settings.Height = this.RestoreBounds.Height;
                settings.Width = this.RestoreBounds.Width;
                settings.WindowState = WindowState.Maximized; // 상태는 '최대화'로 저장
            }
            else
            {
                // 일반 또는 최소화 상태일 경우, 현재 크기를 그대로 저장
                settings.Top = this.Top;
                settings.Left = this.Left;
                settings.Height = this.Height;
                settings.Width = this.Width;
                settings.WindowState = this.WindowState;
            }

            // --- 내부 패널 크기 저장 (이전과 동일) ---
            settings.TemplatesPanelHeight = TemplatesRow.Height;
            settings.ProgramPanel1Height = ProgramView1Row.Height;
            settings.Panel1_ListWidth = View1_ListColumn.Width;
            settings.Panel2_ListWidth = View2_ListColumn.Width;

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(WindowSettingsFile, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving window settings: {ex.Message}");
            }
        }


        // --- 기존 이벤트 핸들러 ---

        private void ProgramView_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                if (sender == View1) vm.ActivePanelVM = vm.Panel1VM;
                else if (sender == View2) vm.ActivePanelVM = vm.Panel2VM;
            }
        }

        private void RecentTemplatesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedRecentTemplate != null)
            {
                if (vm.LoadSpecificRecentTemplateCommand.CanExecute(null))
                {
                    vm.LoadSpecificRecentTemplateCommand.Execute(null);
                }
            }
        }

        // ### [추가] 'No.' 컬럼 더블클릭 시 프로그램 실행을 위한 이벤트 핸들러 ###
        private void ProgramGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 더블클릭된 UI 요소에서 DataGridCell을 찾습니다.
            if (e.OriginalSource is DependencyObject source && UIHelper.TryFindAncestor<DataGridCell>(source) is DataGridCell cell)
            {
                // 해당 셀이 'No.' 컬럼인지 확인합니다.
                if (cell.Column is DataGridTextColumn column && column.Header.ToString() == "No.")
                {
                    // 이벤트가 발생한 DataGrid의 ViewModel을 가져옵니다.
                    if (sender is DataGrid dataGrid && dataGrid.DataContext is ProgramPanelViewModel vm)
                    {
                        // ViewModel의 프로그램 실행 커맨드를 실행합니다.
                        if (vm.RunProgramCommand.CanExecute(null))
                        {
                            vm.RunProgramCommand.Execute(null);
                        }
                        // 이벤트가 다른 곳으로 전파되지 않도록 처리되었음을 표시합니다.
                        e.Handled = true;
                    }
                }
            }
        }

        // 변경: 창이 닫힐 때 모든 설정을 저장하도록 SaveWindowSettings() 호출 추가
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // ViewModel의 데이터 저장
            if (DataContext is MainViewModel vm)
            {
                vm.SaveAllSettings();
            }

            // 창 레이아웃 설정 저장
            SaveWindowSettings();
        }
    }
}
