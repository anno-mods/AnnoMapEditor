using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public partial class UnifiedIslandProperties : UserControl
    {
        public UnifiedIslandProperties()
        {
            InitializeComponent();
        }

        private void LabelTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Enter or Key.Return)) return;
            e.Handled = true;
            (sender as TextBox)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}