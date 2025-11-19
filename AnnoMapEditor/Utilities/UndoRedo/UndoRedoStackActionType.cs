using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public enum ActionType
    {
        MapElementTransform,
        IslandAdd,
        IslandRemove,
        IslandProperties,
        IslandFertilities,
        Label,
        Group,
        MapProperties
    }
}
