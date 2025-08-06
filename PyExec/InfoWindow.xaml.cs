using System.Windows;

namespace PyExec
{
    /// <summary>
    /// Interaction logic for InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        // 기본 생성자 (XAML 로딩에 필요)
        public InfoWindow()
        {
            InitializeComponent();
        }

        // 제목과 내용을 받아 창을 초기화하는 생성자
        public InfoWindow(string title, string content) : this() // 기본 생성자 호출 필수!
        {
            this.Title = title; // 창 제목 설정
            this.InfoTextBox.Text = content; // 텍스트 상자 내용 설정
        }
    }
}