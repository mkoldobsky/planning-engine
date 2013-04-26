using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    public class Company : Entity, ICompany
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Territory { get; set; }
        public bool IsActive { get; set; }
    }
}
