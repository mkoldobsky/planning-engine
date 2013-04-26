using System.Collections;
using System.Text;

namespace Engine.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    public class Rule<T, R> : IRule<T, R>
    {
        public Rule()
        {

        }

        public Rule(string rule)
        {
            this.Parse(rule);
            this.RuleText = rule;
        }

        public IMonthlyParameter<T> I { get; set; }
        public IMonthlyParameter<T> J { get; set; }
        public IOperation Operation { get; set; }
        private String RuleText;
        protected List<IMonthlyParameter<T>> Parameters = new List<IMonthlyParameter<T>>();


        private void Parse(string rule)
        {
            rule = rule.Trim();
            var tokens = SplitWithParameters(rule);
            var parameter = new Regex(@"{(.*)}", RegexOptions.Compiled);
            var operation = new Regex(@"^\s*(<=|>=|==|!=|[=+\-*/^()!<>])", RegexOptions.Compiled);
            foreach (var token in tokens)
            {
                if (parameter.IsMatch(token))
                {
                    var name = token.TrimStart('{').TrimEnd('}');

                    if (Operation == null)
                        I = new MonthlyParameter<T> { Name = name };
                    else
                        J = new MonthlyParameter<T> { Name = name };
                }
                else
                {
                    if (operation.IsMatch(token))
                        Operation = new Operation(token);
                    else
                    {
                        if (Operation == null)
                        {
                            I = new MonthlyParameter<T>();
                            I.SetConstant((T)Convert.ChangeType(token, typeof(T)));
                        }
                        else
                        {
                            J = new MonthlyParameter<T>();
                            J.SetConstant((T)Convert.ChangeType(token, typeof(T)));
                        }
                    }
                }
            }
            if (I == null || Operation == null || J == null)
                throw new FormatException("Invalid operation format.");

        }

        private IEnumerable<string> SplitWithParameters(string rule)
        {
            var result = new List<string>();
            var token = new StringBuilder();
            bool inParameter = false;
            foreach (var character in rule)
            {
                if (character == '{')
                {
                    inParameter = true;
                    if (!string.IsNullOrEmpty(token.ToString()))
                    {
                        result.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    token.Append(character);

                }
                if (character == '}')
                {
                    token.Append(character);
                    result.Add(token.ToString());
                    token = new StringBuilder();
                    inParameter = false;
                }
                if (!inParameter && character == ' ')
                {
                    if (!string.IsNullOrEmpty(token.ToString()))
                    {
                        result.Add(token.ToString());
                        token = new StringBuilder();
                    }
                }
                if (inParameter && character == ' ')
                    token.Append(character);
                if (character != ' ' && character != '}' && character != '{')
                    token.Append(character);

            }
            if (!string.IsNullOrEmpty(token.ToString()))
            {
                result.Add(token.ToString());
                token = new StringBuilder();
            }
            return result;
        }

        public bool Validate()
        {
            if (I == null || J == null)
                throw new RuleException();
            if (Operation == null)
                throw new RuleException();
            return true;
        }

        public Expression<Func<T, T, R>> GetExpression()
        {
            this.Validate();

            var left = Expression.Parameter(typeof(T), "0");
            var right = Expression.Parameter(typeof(T), "1");
            var expression = Expression.MakeBinary(Operation.GetOperator(), left, right);
            return Expression.Lambda<Func<T, T, R>>(expression, new[] { left, right });
        }


        public IMonthlyParameter<R> GetResult()
        {
            var expression = GetExpression();
            var result = new MonthlyParameter<R>();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                T i, j;
                if (I.Value != null && J.Value != null)
                {
                    i = I.Value.ContainsKey(month) ? I.Value[month] : default(T);
                    j = J.Value.ContainsKey(month) ? J.Value[month] : default(T);
                    result.Value[month] = expression.Compile()(i, j);
                }
            }
            return result;
        }

        public void SetParameters(IList parameters)
        {
            if (parameters != null)
                this.Parameters = (List<IMonthlyParameter<T>>)parameters;
            if (I != null)
            {
                var parameter = this.Parameters.FirstOrDefault(x => x.Name == I.Name);
                if (parameter != null)
                    I.Value = parameter.Value;
            }
            if (J != null)
            {
                var parameter = this.Parameters.FirstOrDefault(x => x.Name == J.Name);
                if (parameter != null)
                    J.Value = parameter.Value;
            }
        }

    }
}
