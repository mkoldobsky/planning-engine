namespace Engine.Core
{
    using System.Collections;

    public interface IRule<T, R>
    {
        IOperation Operation { get; set; }

        bool Validate();
        IMonthlyParameter<R> GetResult();
        void SetParameters(IList parameters);
      
    }
}
