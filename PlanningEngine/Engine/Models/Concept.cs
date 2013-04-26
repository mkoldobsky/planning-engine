using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Engine.Core.Interfaces;
using System.Linq.Expressions;

namespace Engine.Core
{

    public class Concept<T> : Entity, IConcept
    {
        private bool _completed = false;
        private string _operationText;
        public String Title { get; set; }
        public String Description { get; set; }
        public int Sequence { get; set; }
        public int? GLAccountId { get; set; }
        public GLAccount GLAccount { get; set; }
        public ExpenseType ExpenseType { get; set; }
        public Scheme<T> Scheme { get; set; }

        public List<IFIlter<bool>> Filters { get; set; }
        public List<Filter<T, bool>> ConceptFilters { get; set; }
        public Dictionary<Month, bool> ValidMonths { get; set; }

        public IMonthlyParameter<T> Parameter1 { get; set; }
        public IMonthlyParameter<T> Parameter2 { get; set; }
        public IMonthlyParameter<T> Parameter3 { get; set; }
        public IMonthlyParameter<T> Parameter4 { get; set; }

        public virtual int? From { get; set; }
        public virtual int? To { get; set; }
        public virtual IMonthlyParameter<T> ConceptResult { get; set; }
        public virtual IMonthlyParameter<T> Output1 { get; set; }
        public virtual IMonthlyParameter<T> Output2 { get; set; }
        protected IRule<T, T> Operation { get; set; }
        public Dictionary<Month, bool> Filtered { get; set; }

        public Concept()
        {
            Filters = new List<IFIlter<bool>>();
        }

        public Concept(string operationText)
        {
            try
            {
                this.Operation = new RuleTree<T, T>(operationText);
                this._operationText = operationText;
                Filters = new List<IFIlter<bool>>();
                Filtered = new Dictionary<Month, bool>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddFilter(IFIlter<bool> filter)
        {
            Filters.Add(filter);
        }

        public bool Filter(Month month)
        {
            bool result = true;
            for (int i = 0; i < Filters.Count; i++)
            {
                var tempResult = Filters[i].GetResult();
                if (Filters[i].Connector == null)
                {
                    result = tempResult.Value.ContainsKey(month) ? tempResult.Value[month] : true;
                }
                else
                {
                    var x = Expression.Parameter(typeof(bool));
                    var y = Expression.Parameter(typeof(bool));
                    result = Expression.Lambda<Func<bool, bool, bool>>(Expression.MakeBinary(Filters[i].Connector.GetOperator(), x, y), new[] { x, y }).Compile()(result, tempResult.Value.ContainsKey(month) ? tempResult.Value[month] : true);
                }
            }
            return result;
        }

        public void Run()
        {
            if (this.Operation == null)
                throw new RuleException();

            if (this.ConceptResult == null)
                this.ConceptResult = new Global<T> { Name = this.Title };

            var value = this.Operation.GetResult();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                Filtered[month] = Filter(month);
                dynamic result = Filtered[month] ? value.Value[month] : default(T);
                if (this.ConceptResult.IsAccumulator)
                {
                    if (!ConceptResult.Value.ContainsKey(month))
                        ConceptResult.Value[month] = default(T);
                    this.ConceptResult.Value[month] += result;
                }
                else if (result != default(T))
                    this.ConceptResult.Value[month] = result;
                if (this.Output1 != null)
                {
                    if (this.Output1.IsAccumulator)
                    {
                        if (!Output1.Value.ContainsKey(month))
                            Output1.Value[month] = default(T);
                        this.Output1.Value[month] += result;
                    }
                    else if (result != default(T))
                        this.Output1.Value[month] = result;
                }
                if (Output2 != null)
                {
                    if (this.Output2.IsAccumulator)
                    {
                        if (!Output2.Value.ContainsKey(month))
                            Output2.Value[month] = default(T);
                        this.Output2.Value[month] += result;
                    }
                    else
                        if (result != default(T))
                            this.Output2.Value[month] = result;
                }

            }
            _completed = true;
        }

        public bool HasSequence()
        {
            return Sequence > 0;
        }

        public void AddOperation(IRule<T, T> rule)
        {
            this.Operation = rule;
        }

        public string GetOperationText()
        {
            return this._operationText;
        }

        public bool ValidateOperation()
        {
            return Operation.Validate();
        }

        public void AddOutputParameter1(IMonthlyParameter<T> output)
        {
            this.Output1 = output;
        }

        public void AddOutputParameter2(IMonthlyParameter<T> output)
        {
            this.Output2 = output;
        }

        public void SetParameters(IList parameters)
        {
            this.Operation.SetParameters(parameters);
        }

        public List<int> GetFunctionsIds()
        {
            var result = new List<int>();

            if (Parameter1 is Function<T>)
                result.Add(Parameter1.Id);

            if (Parameter2 is Function<T>)
                result.Add(Parameter2.Id);

            if (Parameter3 is Function<T>)
                result.Add(Parameter3.Id);

            if (Parameter4 is Function<T>)
                result.Add(Parameter4.Id);

            return result;
        }

        public List<int> GetModifiableGlobalsIds()
        {
            var result = new List<int>();

            var global = Parameter1 as Global<T>;

            if (global != null && global.IsModifiable)
                result.Add(Parameter1.Id);

            global = Parameter2 as Global<T>;

            if (global != null && global.IsModifiable)
                result.Add(Parameter2.Id);

            global = Parameter3 as Global<T>;

            if (global != null && global.IsModifiable)
                result.Add(Parameter3.Id);

            global = Parameter4 as Global<T>;

            if (global != null && global.IsModifiable)
                result.Add(Parameter4.Id);

            return result;
        }

        public IList GetParameters()
        {
            var result = new List<IMonthlyParameter<T>>();

            if (Parameter1 != null)
                result.Add(Parameter1);

            if (Parameter2 != null)
                result.Add(Parameter2);

            if (Parameter3 != null)
                result.Add(Parameter3);

            if (Parameter4 != null)
                result.Add(Parameter4);

            return result;
        }

        public string GetName()
        {
            return Title;
        }

        public bool HasGLAccount()
        {
            return GLAccountId.HasValue;
        }

        public IList GetOutputParameters()
        {
            var result = new List<IMonthlyParameter<T>>();

            if (Output1 != null)
                result.Add(Output1);

            if (Output2 != null)
                result.Add(Output2);

            return result;
        }

        public bool ShouldSave()
        {
            return HasGLAccount() && _completed; // && (Output1.Value.Where(x=>EqualityComparer<T>.Default.Equals(x.Value,  default(T))).Count() == 12);
        }

        public IList GetFilterParameters()
        {
            var result = new List<IMonthlyParameter<T>>();
            foreach (var filter in Filters)
            {
                result.AddRange(filter.GetParameters().Cast<IMonthlyParameter<T>>());
            }
            return result;
        }

        public void SetFilterParameters(IList filterParameters)
        {
            foreach (var filter in Filters)
            {
                filter.SetParameters(filterParameters);
            }
        }

        public void SetOutputParameters(IList outputParameters)
        {
            var output = (List<IMonthlyParameter<T>>)outputParameters;
            if (this.Output1 != null)
            {
                var output1 = output.FirstOrDefault(x => x.Name == this.Output1.Name);
                if (output1 != null)
                {
                    foreach (Month month in Enum.GetValues(typeof(Month)))
                    {
                        this.Output1.Value[month] = output1.Value[month];
                    }
                }
            }
            if (this.Output2 != null)
            {
                var output2 = output.FirstOrDefault(x => x.Name == this.Output2.Name);
                if (output2 != null)
                    foreach (Month month in Enum.GetValues(typeof(Month)))
                    {
                        this.Output2.Value[month] = output2.Value[month];
                    }
            }
        }

        public IList GetFilters()
        {
            return this.Filters;
        }

        public void ClearOutputParameters()
        {
            if (Output1 != null)
                Output1.Value = ClearValues();
            if (Output2 != null)
                Output2.Value = ClearValues();
        }

        public bool IsMonthFiltered(Month month)
        {
            return Filtered.ContainsKey(month) && Filtered[month];
        }

        private Dictionary<Month, T> ClearValues()
        {
            var result = new Dictionary<Month, T>();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                result[month] = default(T);
            }
            return result;
        }

        public IMonthlyParameter<T> GetResult()
        {
            return this.Output1;
        }

        public bool HasFilters
        {
            get
            {
                return this.Filters.Count > 0;
            }
        }
    }
}
