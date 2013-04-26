using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    public class Function<T> : Entity, IMonthlyParameter<T>
    {
        public string Name { get; set; }

        public bool IsAccumulator { get; set; }

        public Dictionary<Month, T> Value { get; set; }

        public T Total { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public void SetConstant(T constant)
        {
            Total = constant;
        }

        public string Description { get; set; }

        public Company Company { get; set; }

        public FromDisney.ParameterType ParameterType { get; set; }
        
        public DateTime? From { get; set; }
       
        public DateTime? To { get; set; }
      
        public bool IsConstant { get; set; }
        
        public bool IsModifiable { get; set; }
      
        public bool Modified { get; set; }

        public string FixedValue { get; set; } 
    }
}
