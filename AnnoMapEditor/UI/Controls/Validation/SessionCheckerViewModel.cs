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
    public class SessionCheckerViewModel : ObservableBase
    {
        private readonly Session _session;

        private readonly ICollection<ISessionValidator> _validators = new ISessionValidator[]
        {
            new SmallPoolSizeValidator(),
            new PoolSizeValidator(IslandSize.Medium),
            new PoolSizeValidator(IslandSize.Large),
            new ContinentalIslandLimitValidator()
        };

        public ObservableCollection<SessionValidatorResult> Results { get; } = new();

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


        public SessionCheckerViewModel(Session session)
        {
            _session = session;
            _session.Elements.CollectionChanged += Session_OnIslandCollectionChanged;

            foreach (var island in session.Elements)
                island.PropertyChanged += Island_PropertyChanged;

            Update();
        }


        private void Session_OnIslandCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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

            foreach (ISessionValidator validator in _validators)
            {
                SessionValidatorResult validationResult = validator.Validate(_session);

                if (validationResult.Status != SessionValidatorStatus.Ok)
                {
                    Results.Add(validationResult);

                    hasWarnings |= validationResult.Status == SessionValidatorStatus.Warning;
                    hasErrors |= validationResult.Status == SessionValidatorStatus.Error;
                }
            }

            HasErrors = hasErrors;
            HasWarnings = hasWarnings;
        }
    }
}
