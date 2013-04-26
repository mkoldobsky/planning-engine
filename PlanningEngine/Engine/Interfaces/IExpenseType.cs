using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine.Core.Interfaces
{
    public interface IExpenseType
    {
        int Id { get; set; }
        ExpenseGroup ExpenseGroup { get; set; }
        string Code { get; set; }
        string Description { get; set; }
        bool IsActive { get; set; }
    }
}
