namespace Engine.Core
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using Engine.Core.Models;

    public class Scheme<T> : Entity
    {
        public string Name { get; set; }
        public string HCTypeCode { get; set; }
        public string CompanyCode { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string ModifiedUserName { get; set; }
        public string UserName { get; set; }
        public HCType HCType { get; set; }
        public Company Company { get; set; }
        public bool IsActive { get; set; }

        public List<IConcept> Concepts { get; set; }

        private int _runConcept;
        private Dictionary<int, IMonthlyParameter<T>> _parameterCache =
            new Dictionary<int, IMonthlyParameter<T>>();

        public Scheme()
        {
            Concepts = new List<IConcept>();
            _runConcept = 0;
        }

        public void AddConcept(IConcept concept)
        {
            Concepts.Add(concept);
        }

        public void DeleteConcept(IConcept concept)
        {
            Concepts.Remove(concept);
        }

        public void Run()
        {
            foreach (var concept in Concepts)
            {
                RunConcept(concept);
            }
        }

        private void RunConcept(IConcept concept)
        {
            try
            {
                var filterParameters = concept.GetFilterParameters();
                UpdateParametersFromCache(filterParameters);
                concept.SetFilterParameters(filterParameters);
                var parameters = concept.GetParameters();
                UpdateParametersFromCache(parameters);
                concept.SetParameters(parameters);
                var outputParameters = concept.GetOutputParameters();
                UpdateParametersFromCache(outputParameters);
                concept.SetOutputParameters(outputParameters);
                concept.Run();
                UpdateParameterCache(concept.GetOutputParameters());

            }
            catch (Exception e)
            {

                throw;
            }
        }

        public void RunConcept()
        {
            if (_runConcept < Concepts.Count)
            {
                RunConcept(Concepts[_runConcept]);

            }
        }

        public void MoveNext()
        {
            _runConcept++;
        }

        public bool HasConceptsToRun()
        {
            return _runConcept < Concepts.Count;
        }

        public IConcept GetCurrentConcept()
        {
            return Concepts[_runConcept];
        }

        private void UpdateParameterCache(IList outputParameters)
        {
            var output = (List<IMonthlyParameter<T>>)outputParameters;
            foreach (IMonthlyParameter<T> monthlyParameter in output)
            {
                if (monthlyParameter is Function<T>) continue;
                _parameterCache[monthlyParameter.Id] = monthlyParameter;
            }
        }

        private void UpdateParametersFromCache(IList parameters)
        {
            foreach (IMonthlyParameter<T> parameter in parameters)
            {
                if (parameter is Function<T>) continue;
                if (_parameterCache.ContainsKey(parameter.Id))
                    parameter.Value = _parameterCache[parameter.Id].Value;
            }
        }

        public bool Validate()
        {
            if (Concepts.Count == 0)
                throw new SchemeException();
            if (Concepts.Where(x => !x.HasSequence()).Count() > 0)
                return false;
            if (Concepts.GroupBy(x => x.Sequence).SelectMany(grp => grp.Skip(1)).Count() > 0)
                return false;
            return true;
        }

        public List<int> GetFunctionsToPopulate()
        {
            var functions = new List<int>();
            foreach (var concept in Concepts)
            {
                functions.AddRange(concept.GetFunctionsIds());
            }
            return functions;
        }

        public List<int> GetModifiableGlobalsToPopulate()
        {
            var modifiableGlobals = new List<int>();
            foreach (var concept in Concepts)
            {
                modifiableGlobals.AddRange(concept.GetModifiableGlobalsIds());
            }
            return modifiableGlobals;
        }

        public List<IMonthlyParameter<T>> GetParameters()
        {
            var parameters = new List<IMonthlyParameter<T>>();
            foreach (var concept in Concepts)
            {
                foreach (IMonthlyParameter<T> parameter in concept.GetParameters())
                {
                    parameters.Add(parameter);
                }
                foreach (IMonthlyParameter<T> outputParameter in concept.GetOutputParameters())
                {
                    parameters.Add(outputParameter);
                }
            }
            return parameters;
        }

        public List<Tuple<int, IMonthlyParameter<T>>> GetResults()
        {
            var results = new List<Tuple<int, IMonthlyParameter<T>>>();

            foreach (var concept in Concepts)
            {
                results.Add(Tuple.Create(((Concept<T>)concept).Id, ((Concept<T>)concept).GetResult()));
            }
            return results;
        }

        public List<IConcept> GetConcepts()
        {
            return Concepts;
        }

        public void CacheParameters()
        {
            var parameters = this.GetParameters();
            foreach (var parameter in parameters)
            {
                var parameter1 = parameter;
                if (!(parameter1 is Global<T>))
                    parameter1.Value = new Dictionary<Month, T>();
                if (!_parameterCache.ContainsKey(parameter.Id))
                    _parameterCache[parameter.Id] = parameter1;
            }
        }

        public List<IMonthlyParameter<T>> GetAccumulableParameters()
        {
            var result = new List<IMonthlyParameter<T>>();
            foreach (var monthlyParameter in _parameterCache.Where(x => x.Value.IsAccumulator).ToList())
            {
                result.Add(monthlyParameter.Value);
            }
            return result;
        }

        public void StartOver()
        {
            _runConcept = 0;
            _parameterCache = new Dictionary<int, IMonthlyParameter<T>>();
            foreach (var concept in Concepts)
            {
                if (concept != null)
                    concept.ClearOutputParameters();
            }
        }
    }
}
