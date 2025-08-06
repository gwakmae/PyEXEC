// PyExec/Helpers/ContextMenuHandler.cs



using PyExec.Models;

using System;

using System.Diagnostics;

using System.IO;

using System.Windows;  // FrameworkElement



namespace PyExec.Helpers

{

    public class ContextMenuHandler

    {

        private readonly ProgramRunner _programRunner;



        public ContextMenuHandler(ProgramRunner programRunner)

        {

            _programRunner = programRunner ?? throw new ArgumentNullException(nameof(programRunner));

        }



        public void OpenProgram_Click(object sender, RoutedEventArgs e)

        {

            if (sender is FrameworkElement fe && fe.DataContext is Program program)

            {

                _programRunner.Run(program);

            }

            else

            {

                MessageBox.Show("Could not retrieve program information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }



        public void OpenProgramFolder_Click(object sender, RoutedEventArgs e)

        {

            if (sender is FrameworkElement fe && fe.DataContext is Program program)

            {

                string? folder = Path.GetDirectoryName(program.Path);

                if (!string.IsNullOrEmpty(folder))

                {

                    Process.Start(new ProcessStartInfo("explorer.exe", folder) { UseShellExecute = true });

                }

                else

                {

                     MessageBox.Show("Could not retrieve program information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }

            }

             else

            {

                MessageBox.Show("Could not retrieve program information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

    }

}