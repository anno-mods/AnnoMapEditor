using AnnoMapEditor.MapTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Controls
{
    public class SessionPropertiesViewModel : ViewModelBase
    {
        internal Session _session;

        public string MapSizeText { get; private set; }
        public Region SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (value != _selectedRegion)
                {
                    _selectedRegion = value;
                    _session.Region = value;
                    _ = _session.UpdateAsync();
                }
            }
        }
        private Region _selectedRegion;

        public Region[] Regions { get; } = Region.All;

        public SessionPropertiesViewModel(Session session)
        {
            _session = session;
            MapSizeText = $"Size: {_session.Size.X}, Playable: {_session.PlayableArea.Width}";
            _selectedRegion = _session.Region;
        }
    }
}
