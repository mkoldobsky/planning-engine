using System.Collections;

namespace Engine.Core
{
    using System.Collections.Generic;

    public interface IFIlter<R>
    {
        string Name { get; set; }
        string Description { get; set; }
        int Id { get; set; }
        IOperation Operation { get; set; }
        IOperation Connector { get; set; }
        int? Sequence { get; set; }
        int FilterType { get; set; }

        bool Validate();
        IMonthlyParameter<R> GetResult();
        void SetParameters(IList parameters);
        IList GetParameters();
    }
}
