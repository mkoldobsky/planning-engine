using System.Collections;
using Engine.Core.Interfaces;

namespace Engine.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Text.RegularExpressions;

    public class RuleTree<T, R> : IRule<T, R>
    {
        private string RuleText;
        public List<IMonthlyParameter<T>> Parameters = new List<IMonthlyParameter<T>>();


        public IRule<T, R> I { get; set; }
        public IRule<T, R> J { get; set; }
        public IOperation Operation { get; set; }

        public RuleTree()
        {

        }

        public RuleTree(string rule)
        {
            this.Parse(rule);
            this.RuleText = rule;
        }

        private void Parse(string rule)
        {
            try
            {
                var rules = SplitRules(rule);
                if (rules.Count() == 3)
                {
                    I = new Rule<T, R>(rules[0]);
                    Operation = new Operation(rules[1]);
                    J = new Rule<T, R>(rules[2]);
                    return;
                }
                if (rules.Count() == 1)
                {
                    I = new Rule<T, R>(rules[0]);
                    return;
                }

            }
            catch (Exception)
            {
                throw new ForecastPlanException(string.Format("Parse error - operation {0}", rule));
            }
        }

        private static String[] SplitRules(string rule)
        {
            var operation = new Regex(@"^\s*(<=|>=|==|!=|[=+\-*/^!<>])", RegexOptions.Compiled);
            var result = new List<string>();
            var str = new StringBuilder();
            for (int i = 0; i < rule.Length; i++)
            {
                if (rule[i] == ')')
                {
                    result.Add(str.ToString());
                    str = new StringBuilder();
                }
                if (operation.IsMatch(rule[i].ToString()) && result.Count == 1)
                {
                    result.Add(rule[i].ToString());
                    str = new StringBuilder();
                }
                else
                    if (rule[i] != '(' && rule[i] != ')')
                        str.Append(rule[i]);
            }
            if (!string.IsNullOrWhiteSpace(str.ToString()))
                result.Add(str.ToString());
            return result.ToArray();
        }


        public bool Validate()
        {
            //if (I == null && J == null)
            //    throw new RuleException("There are no operat");
            //if (Operation == null)
            //    throw new RuleException();
            return true;
        }

        public Expression<Func<R, R, R>> GetExpression()
        {
            this.Validate();

            var left = Expression.Parameter(typeof(R), "0");
            var right = Expression.Parameter(typeof(R), "1");
            var expression = Expression.MakeBinary(Operation.GetOperator(), left, right);
            return Expression.Lambda<Func<R, R, R>>(expression, new[] { left, right });
        }

        public IMonthlyParameter<R> GetResult()
        {
            var result = new MonthlyParameter<R>();
            if (J != null)
            {
                var expression = GetExpression();
                foreach (Month month in Enum.GetValues(typeof (Month)))
                {
                    var iResult = I.GetResult();
                    R i = iResult.Value.ContainsKey(month) ? iResult.Value[month] : default(R);
                    var jResult = J.GetResult();
                    R j = jResult.Value.ContainsKey(month) ? jResult.Value[month] : default(R);
                    result.Value[month] = expression.Compile()(i, j);
                }
            }
            else
            {
                foreach (Month month in Enum.GetValues(typeof(Month)))
                {
                    var iResult = I.GetResult();
                    R i = iResult.Value.ContainsKey(month) ? iResult.Value[month] : default(R);
                    result.Value[month] = i;
                } 
            }
            return result;
        }

        public void SetParameters(IList parameters)
        {
            if (parameters == null)
                throw new RuleException();
            this.Parameters = (List<IMonthlyParameter<T>>)parameters;

            if (I != null)
            {
                I.SetParameters(parameters);
            }
            if (J != null)
            {
                J.SetParameters(parameters);
            }

        }
    }
}
