using Engine.Core.Interfaces;
using Engine.Core.Models;
using FromDisney;

namespace Engine.Core
{
    using System;
    using System.Collections.Generic;

    public class Global<T> : Entity, IMonthlyParameter<T>, IGlobal<T>
    {
        public Global()
        {
            Value = new Dictionary<Month, T>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Company Company { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool IsConstant { get; set; }
        public bool IsModifiable { get; set; }
        public bool Modified { get; set; }
        public bool IsAccumulator { get; set; }
        public ParameterType ParameterType { get; set; }
        public int? ParameterDataTypeId { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string FixedValue { get; set; }
        public List<MonthlyParameter<decimal>> MonthlyParameter { get; set; }

        #region ForForecastPlanEdit

        public virtual Dictionary<Month, T> Value { get; set; }

        public T Jan
        {
            get { return Value[Month.January]; }
            set { Value[Month.January] = value; }
        }

        public T Feb
        {
            get { return Value[Month.February]; }
            set { Value[Month.February] = value; }
        }
        public T Mar
        {
            get { return Value[Month.March]; }
            set { Value[Month.March] = value; }
        }
        public T Apr
        {
            get { return Value[Month.April]; }
            set { Value[Month.April] = value; }
        }
        public T May
        {
            get { return Value[Month.May]; }
            set { Value[Month.May] = value; }
        }
        public T Jun
        {
            get { return Value[Month.June]; }
            set { Value[Month.June] = value; }
        }
        public T Jul
        {
            get { return Value[Month.July]; }
            set { Value[Month.July] = value; }
        }
        public T Aug
        {
            get { return Value[Month.August]; }
            set { Value[Month.August] = value; }
        }
        public T Sep
        {
            get { return Value[Month.September]; }
            set { Value[Month.September] = value; }
        }
        public T Oct
        {
            get { return Value[Month.October]; }
            set { Value[Month.October] = value; }
        }
        public T Nov
        {
            get { return Value[Month.November]; }
            set { Value[Month.November] = value; }
        }
        public T Dec
        {
            get { return Value[Month.December]; }
            set { Value[Month.December] = value; }
        }

        #endregion

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

        public void SetConstant(T constant)
        {
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                Value[month] = constant;
            }
        }


        public bool IsValid(DateTime date)
        {
            if (IsConstant)
            {
                var result = true;
                if (this.From.HasValue)
                    result = date.CompareTo(this.From.Value) >= 0;
                if (this.To.HasValue)
                    result = result && date.CompareTo(this.To.Value) <= 0;
                return result;
            }
            return true;
        }
    }
}
