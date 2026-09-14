using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UniversalOfficeDataManager.ViewModels;

namespace UniversalOfficeDataManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;

        // TreeView.SelectedItem is read-only in WPF (no direct two-way
        // binding support), so selection is wired through this event
        // rather than a Binding on the TreeView itself.
        private void WorkspaceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (ViewModel == null) return;

            if (e.NewValue is SheetNodeViewModel sheetNode)
                ViewModel.SelectedSheetNode = sheetNode;
        }

        // Context-menu items live in a popup, which is a separate visual
        // tree from the TreeView — that breaks RelativeSource lookups back
        // up to the Window's DataContext, so these handlers read the
        // MenuItem's own (inherited) DataContext instead and invoke the
        // matching command directly.
        private void AddSheetMenuItem_Click(object sender, RoutedEventArgs e) =>
            RunWorkspaceCommand(sender, vm => vm.AddSheetCommand);

        private void RenameWorkspaceMenuItem_Click(object sender, RoutedEventArgs e) =>
            RunWorkspaceCommand(sender, vm => vm.EditWorkspaceCommand);

        private void DeleteWorkspaceMenuItem_Click(object sender, RoutedEventArgs e) =>
            RunWorkspaceCommand(sender, vm => vm.DeleteWorkspaceCommand);

        private void RenameSheetMenuItem_Click(object sender, RoutedEventArgs e) =>
            RunSheetCommand(sender, vm => vm.EditSheetCommand);

        private void DeleteSheetMenuItem_Click(object sender, RoutedEventArgs e) =>
            RunSheetCommand(sender, vm => vm.DeleteSheetCommand);

        private void RunWorkspaceCommand(object sender, Func<MainViewModel, ICommand> commandSelector)
        {
            if (ViewModel == null) return;
            if (sender is not MenuItem menuItem) return;
            if (menuItem.DataContext is not WorkspaceNodeViewModel node) return;

            var command = commandSelector(ViewModel);
            if (command.CanExecute(node))
                command.Execute(node);
        }

        private void RunSheetCommand(object sender, Func<MainViewModel, ICommand> commandSelector)
        {
            if (ViewModel == null) return;
            if (sender is not MenuItem menuItem) return;
            if (menuItem.DataContext is not SheetNodeViewModel node) return;

            var command = commandSelector(ViewModel);
            if (command.CanExecute(node))
                command.Execute(node);
        }
    }
}
