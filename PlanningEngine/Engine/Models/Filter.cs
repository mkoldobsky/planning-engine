namespace Engine.Core
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    public class Filter<T, R> : Entity, IFIlter<R>
    {
        public Filter()
        {
            Parameters = new List<IMonthlyParameter<T>>();
        }

        public Filter(string rule)
            : this()
        {
            this.Parse(rule);
            this.RuleText = rule;
        }

        public virtual IMonthlyParameter<T> Parameter1 { get; set; }
        public virtual IMonthlyParameter<T> Parameter2 { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public virtual IOperation Operation { get; set; }
        public IOperation Connector { get; set; }
        private String RuleText;
        public int FilterType { get; set; }
        public int? Sequence { get; set; }
        public virtual IList<IMonthlyParameter<T>> Parameters { get; set; }
        
        private void Parse(string rule)
        {
            rule = rule.Trim();
            var tokens = rule.Split(' ');
            var parameter = new Regex(@"{(.*)}", RegexOptions.Compiled);
            var operation = new Regex(@"^\s*(<=|>=|==|!=|[=+\-*/^()!<>])", RegexOptions.Compiled);
            foreach (var token in tokens)
            {
                if (parameter.IsMatch(token))
                {
                    var name = token.TrimStart('{').TrimEnd('}');

                    if (Operation == null)
                        Parameter1 = new MonthlyParameter<T> { Name = name };
                    else
                        Parameter2 = new MonthlyParameter<T> {Name = name};
                }
                else
                {
                    if (operation.IsMatch(token))
                        Operation = new Operation(token);
                    else
                    {
                        if (Operation == null)
                        {
                            Parameter1 = new MonthlyParameter<T> { Value = SetDictionaryFromValue((T)Convert.ChangeType(token, typeof(T))) };
                        }
                        else
                        {
                            Parameter2 = new MonthlyParameter<T> { Value = SetDictionaryFromValue((T)Convert.ChangeType(token, typeof(T))) };
                        }
                    }
                }


            }
        }

        public bool Validate()
        {
            if (Parameter1 == null || Parameter2 == null)
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
            var result = new MonthlyParameter<R>();
            var expression = GetExpression();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                
                T i = Parameter1.Value.ContainsKey(month) ? Parameter1.Value[month] : default(T);
                T j = Parameter2.Value.ContainsKey(month) ? Parameter2.Value[month] : default(T);
                result.Value[month] = expression.Compile()(i, j);
            }
            return result;
        }

        public void SetParameters(IList parameters)
        {
            if (parameters != null)
                this.Parameters = (List<IMonthlyParameter<T>>)parameters;

            if (Parameter1 != null)
            {
                var parameter = this.Parameters.FirstOrDefault(x => x.Name == Parameter1.Name);
                if (parameter != null)
                    Parameter1.Value = parameter.Value;
            }
            if (Parameter2 != null)
            {
                var parameter = this.Parameters.FirstOrDefault(x => x.Name == Parameter2.Name);
                if (parameter != null)
                    Parameter2.Value = parameter.Value;
            }
        }

        public IList GetParameters()
        {
            var result = new List<IMonthlyParameter<T>>();

            if (Parameter1 != null)
                result.Add(Parameter1);
            
            if (Parameter2 != null)
                result.Add(Parameter2);

            return result;
        }

        public void SetConnector(string connector)
        {
            Connector = new Operation(connector);
        }

        private static Dictionary<Month, T> SetDictionaryFromValue(T value)
        {
            var result = new Dictionary<Month, T>();
            result[Month.January] = value;
            result[Month.February] = value;
            result[Month.March] = value;
            result[Month.April] = value;
            result[Month.May] = value;
            result[Month.June] = value;
            result[Month.July] = value;
            result[Month.August] = value;
            result[Month.September] = value;
            result[Month.October] = value;
            result[Month.November] = value;
            result[Month.December] = value;
            return result;
        }
    }
}
