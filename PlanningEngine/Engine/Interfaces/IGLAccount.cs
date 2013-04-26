using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core.Interfaces
{
    public interface IGLAccount
    {
        int Id { get; set; }
        string SAPCode { get; set; }
        string Title { get; set; }
        bool IsActive { get; set; }
    }
}
