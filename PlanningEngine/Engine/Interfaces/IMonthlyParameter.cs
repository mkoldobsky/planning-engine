using FromDisney;

namespace Engine.Core
{
    using System.Collections.Generic;

    public interface IMonthlyParameter<T>
    {
        int Id { get; set; }
        string Name { get; set; }
        bool IsAccumulator { get; set; }
        ParameterType ParameterType { get; set; }
        string FixedValue { get; set; }
        Dictionary<Month, T> Value { get; set; }
        T Total { get; set; }
        void SetConstant(T constant);
    }
}
