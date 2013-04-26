using System.Linq;

namespace Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class Operation : IOperation
    {
        private readonly Dictionary<String, ExpressionType> map = new Dictionary<string, ExpressionType> 
        {
            {"+", ExpressionType.Add},
            {"-", ExpressionType.Subtract},
            {"*", ExpressionType.Multiply},
            {"/", ExpressionType.Divide},
            {"<", ExpressionType.LessThan},
            {">", ExpressionType.GreaterThan},
            {"<=", ExpressionType.LessThanOrEqual},
            {">=", ExpressionType.GreaterThanOrEqual},
            {"=", ExpressionType.Equal},
            {"==", ExpressionType.Equal},
            {"!=", ExpressionType.NotEqual},
            {"<>", ExpressionType.NotEqual},
            {"AND", ExpressionType.And},
            {"and", ExpressionType.And},
            {"OR", ExpressionType.Or},
            {"or", ExpressionType.Or}
        };


        public Operation()
        {

        }

        public List<string> GetComparators()
        {
            return new List<string>{"<", ">", "=", "<>", "<=", ">="};
        }

        public List<string> GetOperators()
        {
            var arr = new string[map.Values.Count];

            map.Keys.CopyTo(arr, 0);

            return arr.OfType<string>().ToList();
        }


        public Operation(string token)
        {
            if (String.IsNullOrEmpty(token)) return;

            if (map.ContainsKey(token))
                Operator = map[token];
            else
            {
                throw new RuleException();
            }
        }

        public ExpressionType Operator { get; set; }

        public ExpressionType GetOperator()
        {
            return this.Operator;
        }

        public string GetOperator(string value)
        {
            var expressionType = map.FirstOrDefault(m => m.Value.ToString() == value);

            return expressionType.Key;
        }
    }
}
