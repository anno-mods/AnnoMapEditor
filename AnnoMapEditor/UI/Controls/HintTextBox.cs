/*
 * based on the work by Miňo
 * https://code.msdn.microsoft.com/TextBox-with-null-text-hint-0b384543
 * Apache License Version 2.0
 */

using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls
{
    public class HintTextBox : TextBox
    {
        static HintTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintTextBox),
                new FrameworkPropertyMetadata(typeof(HintTextBox)));
        }

        public static readonly DependencyProperty NullTextProperty =
             DependencyProperty.Register("Hint", 
                propertyType: typeof(string), 
                ownerType: typeof(HintTextBox), 
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: ""));

        public string Hint
        {
            get { return (string)GetValue(NullTextProperty); }
            set { SetValue(NullTextProperty, value); }
        }

        public static readonly DependencyProperty MaxTextWidthProperty =
             DependencyProperty.Register("MaxTextWidth",
                propertyType: typeof(int),
                ownerType: typeof(HintTextBox),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: 0));

        public int MaxTextWidth
        {
            get { return (int)GetValue(MaxTextWidthProperty); }
            set { SetValue(MaxTextWidthProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty =
             DependencyProperty.Register("Label", 
                 propertyType: typeof(string),
                 ownerType: typeof(HintTextBox), 
                 typeMetadata: new FrameworkPropertyMetadata(""));

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
    }
}