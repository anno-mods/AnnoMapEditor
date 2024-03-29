﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Controls
{
    public partial class FancyToggle : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty =
              DependencyProperty.Register("IsChecked", typeof(object),
            typeof(FancyToggle), new UIPropertyMetadata(false));
        public static readonly DependencyProperty OnTextProperty =
              DependencyProperty.Register("OnText", typeof(object),
            typeof(FancyToggle), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty OffTextProperty =
              DependencyProperty.Register("OffText", typeof(object),
            typeof(FancyToggle), new UIPropertyMetadata(string.Empty));
        public static readonly DependencyProperty LabelProperty =
              DependencyProperty.Register("Label", typeof(object),
            typeof(FancyToggle), new UIPropertyMetadata(string.Empty));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }

        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public event RoutedEventHandler? ValueChanged;

        public FancyToggle()
        {
            InitializeComponent();

            OnTextBlock.SizeChanged += OnTextBlock_SizeChanged;
            OffTextBlock.SizeChanged += OnTextBlock_SizeChanged;

            Box.Checked += OnBoxValueChanged;
            Box.Unchecked += OnBoxValueChanged;

            OnText = "on";
            OffText = "off"; 
        }

        private void OnBoxValueChanged(object sender, RoutedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        private void OnTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // on/off have a difference of only 1 or 2 pixels and it looks extremely odd to see the "o" jiggle
            // hence fill 1-2 pixel gaps with right margin (left, since flow direction is RightToLeft)

            double diff = OffTextBlock.ActualWidth - OnTextBlock.ActualWidth;
            if (Math.Abs(diff) >= 3)
                diff = 0;

            OnTextBlock.Margin = new Thickness(Math.Max(0, diff), 0, 0, 0);
            OffTextBlock.Margin = new Thickness(Math.Max(0, -diff), 0, 0, 0);
        }
    }
}
