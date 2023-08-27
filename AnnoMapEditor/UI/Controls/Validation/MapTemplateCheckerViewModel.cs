using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Validation;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class MapTemplateCheckerViewModel : ObservableBase
    {
        private readonly MapTemplate _mapTemplate;

        private readonly ICollection<IMapTemplateValidator> _validators = new IMapTemplateValidator[]
        {
            new SmallPoolSizeValidator(),
            new PoolSizeValidator(IslandSize.Medium),
            new PoolSizeValidator(IslandSize.Large),
            new ContinentalIslandLimitValidator()
        };

        public ObservableCollection<MapTemplateValidatorResult> Results { get; } = new();

        public bool HasWarnings
        {
            get => _hasWarnings;
            private set => SetProperty(ref _hasWarnings, value);
        }
        private bool _hasWarnings;

        public bool HasErrors
        {
            get => _hasErrors;
            private set => SetProperty(ref _hasErrors, value);
        }
        private bool _hasErrors;


        public MapTemplateCheckerViewModel(MapTemplate mapTemplate)
        {
            _mapTemplate = mapTemplate;
            _mapTemplate.Elements.CollectionChanged += MapTemplate_OnIslandCollectionChanged;

            foreach (var island in mapTemplate.Elements)
                island.PropertyChanged += Island_PropertyChanged;

            Update();
        }


        private void MapTemplate_OnIslandCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Update();
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        public void Update()
        {
            Results.Clear();

            bool hasWarnings = false;
            bool hasErrors = false;

            foreach (IMapTemplateValidator validator in _validators)
            {
                MapTemplateValidatorResult validationResult = validator.Validate(_mapTemplate);

                if (validationResult.Status != MapTemplateValidatorStatus.Ok)
                {
                    Results.Add(validationResult);

                    hasWarnings |= validationResult.Status == MapTemplateValidatorStatus.Warning;
                    hasErrors |= validationResult.Status == MapTemplateValidatorStatus.Error;
                }
            }

            HasErrors = hasErrors;
            HasWarnings = hasWarnings;
        }
    }
}
