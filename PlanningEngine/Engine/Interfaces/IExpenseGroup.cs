using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core.Interfaces
{
    public interface IExpenseGroup
    {
        int Id { get; set; }
        string Code { get; set; }
        string Description { get; set; }
        bool IsActive { get; set; }
    }
}
