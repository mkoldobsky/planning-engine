using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    public class GLAccount : Entity, IGLAccount
    {
        public string SAPCode { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
    }
}
