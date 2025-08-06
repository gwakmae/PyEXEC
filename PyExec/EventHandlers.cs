// PyExec/EventHandlers.cs
// Note: Many handlers previously here were moved to MainWindow.xaml.cs for better cohesion.
// This file can be kept for future event handlers or removed if empty.

using PyExec.Helpers;
using PyExec.Models;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PyExec
{
    // public partial class MainWindow // Keep if other partial methods exist, otherwise remove partial
    // {
    // --- The following handlers are now likely in MainWindow.xaml.cs ---
    // ProgramGrid_SelectionChanged
    // ProgramGrid_DoubleClick
    // MoveUp_Click (Removed - replaced by RowMoveUp_Click in MainWindow.xaml.cs)
    // MoveDown_Click (Removed - replaced by RowMoveDown_Click in MainWindow.xaml.cs)
    // RunButton_Click
    // RunSelectedProgram
    // ConvertPyPyw_Click
    // ApplyNameToDescription_Click
    // }
}