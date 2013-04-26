namespace Engine.Core
{
    using System.Linq.Expressions;

    public interface IOperation
    {
        ExpressionType GetOperator();
    }
}
