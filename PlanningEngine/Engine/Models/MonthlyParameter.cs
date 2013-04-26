using FromDisney;

namespace Engine.Core
{
    using System;
    using System.Collections.Generic;

    public class MonthlyParameter<T> : Entity, IMonthlyParameter<T>
    {
        public int FiscalYearId { get; set; }
        public string FiscalYearCode { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public Company Company { get; set; }
        public ParameterType ParameterType { get; set; }
        public string FixedValue { get; set; }
        public bool IsAccumulator { get; set; }
        public Dictionary<Month, T> Value { get; set; }

        public T Total
        {
            get
            {
                T result = default(T);
                foreach (Month month in Enum.GetValues(typeof(Month)))
                {
                    dynamic value = Value[month];
                    result += value;
                }
                return result;
            }
            set { }
        }

        public MonthlyParameter()
        {
            this.IsAccumulator = false;
            this.Value = new Dictionary<Month, T>();
        }

        public void SetConstant(T constant)
        {
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                Value[month] = constant;
            }
        }
    }
}
