using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    public class ExpenseType : Entity, IExpenseType
    {
        public ExpenseGroup ExpenseGroup { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
