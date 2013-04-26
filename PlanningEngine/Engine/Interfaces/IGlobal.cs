using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Core.Models;
using FromDisney;

namespace Engine.Core.Interfaces
{
    public interface IGlobal<T>
    {
        string Name { get; set; }
        string Description { get; set; }
        Company Company { get; set; }
        ParameterType ParameterType { get; set; }
        DateTime? From { get; set; }
        DateTime? To { get; set; }
        bool IsConstant { get; set; }
        bool IsModifiable { get; set; }
        bool Modified { get; set; }
        bool IsAccumulator { get; set; }
        int? ParameterDataTypeId { get; set; }
        string TableName { get; set; }
        string ColumnName { get; set; }
        string FixedValue { get; set; }
        List<MonthlyParameter<decimal>> MonthlyParameter { get; set; }
        Dictionary<Month, T> Value { get; set; }
        T Jan { get; set; }
        T Feb { get; set; }
        T Mar { get; set; }
        T Apr { get; set; }
        T May { get; set; }
        T Jun { get; set; }
        T Jul { get; set; }
        T Aug { get; set; }
        T Sep { get; set; }
        T Oct { get; set; }
        T Nov { get; set; }
        T Dec { get; set; }
        T Total { get; set; }
        int Id { get; set; }
        void SetConstant(T constant);
        bool IsValid(DateTime date);
    }
}
