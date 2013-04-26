using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core.Interfaces
{
    public interface ICompany
    {
        string Code { get; set; }
        string Description { get; set; }
        string Territory { get; set; }
        bool IsActive { get; set; }
    }
}
