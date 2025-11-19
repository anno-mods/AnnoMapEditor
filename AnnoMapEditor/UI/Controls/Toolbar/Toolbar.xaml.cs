using System.Windows;
using System.Windows.Controls;
using AnnoMapEditor.Utilities.UndoRedo;

namespace AnnoMapEditor.UI.Controls.Toolbar
{
    public partial class Toolbar : UserControl
    {
        public Toolbar()
        {
            InitializeComponent();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.Undo();
        }

        private void ShowUndoPopup_Click(object sender, RoutedEventArgs e)
        {
            UndoHistoryPopup.IsOpen = !UndoHistoryPopup.IsOpen;
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoStack.Instance.Redo();
        }

        private void NewMap_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.NewMap);
        }

        private void LoadMap_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.LoadMap);
        }

        private void ImportMap_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonContextClick(ToolbarButtonType.ImportMap, sender);
            ToolbarImportContextMenu.PlacementTarget = sender as Panel;
            ToolbarImportContextMenu.IsOpen = true;
        }

        private void SaveMap_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.SaveMap);
        }

        private void ExportMap_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ExportMap);
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ZoomReset);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ZoomIn);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ZoomOut);
        }

        private void ShowLabels_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ShowLabels);
        }

        private void DeleteIsland_Click(object sender, RoutedEventArgs e)
        {
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.DeleteIsland);
        }
    }
}