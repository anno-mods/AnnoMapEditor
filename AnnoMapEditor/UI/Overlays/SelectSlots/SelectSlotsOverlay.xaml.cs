using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    /// <summary>
    /// Interaction logic for SelectSlotsOverlay.xaml
    /// </summary>
    public partial class SelectSlotsOverlay : UserControl
    {
        private readonly ItemContainerGenerator _selectorGeneratorLeft;
        private readonly ItemContainerGenerator _selectorGeneratorRight;
        private readonly ItemContainerGenerator _pinGenerator;


        public SelectSlotsOverlay()
        {
            InitializeComponent();

            SizeChanged += This_SizeChanged;
            DataContextChanged += This_DataContextChanged;

            _selectorGeneratorLeft = SlotSelectionsLeft.ItemContainerGenerator;
            _selectorGeneratorRight = SlotSelectionsRight.ItemContainerGenerator;
            _pinGenerator = Map.ItemContainerGenerator;

            Map.LayoutUpdated += Map_LayoutUpdated;
            ClayFilter.ValueChanged += Filter_ValueChanged;
            MinesFilter.ValueChanged += Filter_ValueChanged;
            OilFilter.ValueChanged += Filter_ValueChanged;
        }


        bool updateScheduled = false;

        private void Filter_ValueChanged(object sender, RoutedEventArgs e)
        {
            updateScheduled = true;
        }

        private void Map_LayoutUpdated(object? sender, EventArgs e)
        {
            if (updateScheduled)
            {
                UpdatePointers();
                updateScheduled = false;
            }
        }


        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is SelectSlotsViewModel oldViewModel)
            {
                oldViewModel.SlotAssignmentViewModels.CollectionChanged -= SlotAssignmentViewModels_CollectionChanged;
                foreach (SlotAssignmentViewModel slotAssignment in oldViewModel.SlotAssignmentViewModels)
                    slotAssignment.PropertyChanged -= SlotAssginment_PropertyChanged;
            }

            if (e.NewValue is SelectSlotsViewModel newViewModel)
            {
                newViewModel.SlotAssignmentViewModels.CollectionChanged += SlotAssignmentViewModels_CollectionChanged;
                foreach (SlotAssignmentViewModel slotAssignment in newViewModel.SlotAssignmentViewModels)
                    slotAssignment.PropertyChanged += SlotAssginment_PropertyChanged;
            }

            UpdatePointers();
        }

        private void SlotAssignmentViewModels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SlotAssignmentViewModel slotAssignment in e.OldItems)
                {
                    slotAssignment.PropertyChanged -= SlotAssginment_PropertyChanged;

                    if (_pointers.Remove(slotAssignment, out Path? path))
                    {
                        LineCanvas.Children.Remove(path);
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (SlotAssignmentViewModel slotAssignment in e.NewItems)
                {
                    slotAssignment.PropertyChanged += SlotAssginment_PropertyChanged;
                    UpdatePointer(slotAssignment);
                }
            }
        }

        private void SlotAssginment_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is SlotAssignmentViewModel slotAssignment)
                UpdatePointer(slotAssignment);
        }

        private void This_SizeChanged(object? sender, EventArgs e)
        {
            UpdatePointers();
        }


        private void UpdatePointers()
        {
            SelectSlotsViewModel? viewModel = DataContext as SelectSlotsViewModel;
            if (viewModel == null)
                return;

            foreach (SlotAssignmentViewModel slotAssignment in viewModel.SlotAssignmentViewModels)
                UpdatePointer(slotAssignment);
        }

        private readonly Dictionary<SlotAssignmentViewModel, Path> _pointers = new();

        private void UpdatePointer(SlotAssignmentViewModel slotAssignment)
        {
            bool pointerExists = _pointers.TryGetValue(slotAssignment, out Path? pointer);

            ListBoxItem? selector = _selectorGeneratorLeft.ContainerFromItem(slotAssignment) as ListBoxItem
                                 ?? _selectorGeneratorRight.ContainerFromItem(slotAssignment) as ListBoxItem;
            ListBoxItem? pin = _pinGenerator.ContainerFromItem(slotAssignment) as ListBoxItem;

            if (selector != null && pin != null)
            {
                Point selectorCenter = new(selector.ActualWidth / 2, selector.ActualHeight / 2);
                Point selectorPosition = selector.TranslatePoint(selectorCenter, LineCanvas);
                Point pinCenter = new(pin.ActualWidth / 2, pin.ActualHeight / 2);
                Point pinPosition = pin.TranslatePoint(pinCenter, LineCanvas);

                double yOffset = pinPosition.Y - selectorPosition.Y;
                double cornerX = pinPosition.X > selectorPosition.X
                    ? pinPosition.X - Math.Abs(yOffset)
                    : pinPosition.X + Math.Abs(yOffset);

                PathFigure pointerFigure;
                LineSegment toCorner;
                LineSegment toSelector;

                // update an existing path
                if (pointerExists)
                {
                    pointerFigure = (pointer.Data as PathGeometry)!.Figures.First();
                    toCorner = (pointerFigure.Segments[0] as LineSegment)!;
                    toSelector = (pointerFigure.Segments[1] as LineSegment)!;
                }

                else
                {
                    PathGeometry pathGeometry = new();
                    pathGeometry.FillRule = FillRule.Nonzero;

                    pointerFigure = new();
                    pointerFigure.IsClosed = false;
                    pathGeometry.Figures.Add(pointerFigure);

                    toCorner = new();
                    pointerFigure.Segments.Add(toCorner);

                    toSelector = new();
                    pointerFigure.Segments.Add(toSelector);

                    pointer = new();
                    pointer.Data = pathGeometry;

                    _pointers.Add(slotAssignment, pointer);
                }

                pointer.StrokeThickness  = slotAssignment.PathThickness;
                pointer.Stroke           = slotAssignment.PinBrush;
                pointerFigure.StartPoint = pinPosition;
                toCorner.Point           = new(cornerX, selectorPosition.Y);
                toSelector.Point         = selectorPosition;

                if (!LineCanvas.Children.Contains(pointer))
                    LineCanvas.Children.Add(pointer);
            }

            // remove the pointer if either the selector or the pin don't exist.
            else
            {
                _pointers.Remove(slotAssignment);
                LineCanvas.Children.Remove(pointer);
            }
        }
    }
}
