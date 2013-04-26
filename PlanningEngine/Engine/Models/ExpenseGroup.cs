using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    public class ExpenseGroup : Entity, IExpenseGroup
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
