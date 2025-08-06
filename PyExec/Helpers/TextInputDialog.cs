// PyExec/Helpers/TextInputDialog.cs
using System.Windows;
using System.Windows.Controls;

namespace PyExec.Helpers
{
    public class TextInputDialog : Window
    {
        private TextBox inputTextBox;
        private Button okButton;
        private Button cancelButton;

        public string InputText { get; private set; } = "";

        public TextInputDialog(string prompt, string title)
        {
            Title = title;
            Width = 400;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 프롬프트 텍스트
            TextBlock promptText = new TextBlock
            {
                Text = prompt,
                Margin = new Thickness(0, 0, 0, 5)
            };
            Grid.SetRow(promptText, 0);
            grid.Children.Add(promptText);

            // 입력 텍스트 박스
            inputTextBox = new TextBox
            {
                Margin = new Thickness(0, 5, 0, 5)
            };
            Grid.SetRow(inputTextBox, 1);
            grid.Children.Add(inputTextBox);

            // 버튼 패널
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };
            Grid.SetRow(buttonPanel, 2);

            okButton = new Button
            {
                Content = "확인",
                Width = 75,
                Margin = new Thickness(0, 0, 5, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) =>
            {
                InputText = inputTextBox.Text;
                DialogResult = true;
            };

            cancelButton = new Button
            {
                Content = "취소",
                Width = 75,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }
    }
}
