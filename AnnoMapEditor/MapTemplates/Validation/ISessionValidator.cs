using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public interface ISessionValidator
    {
        public SessionValidatorResult Validate(Session session);
    }
}
