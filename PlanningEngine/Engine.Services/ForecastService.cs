namespace Engine.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Engine.Core;
    using Engine.Core.Interfaces;
    using Engine.DataAccess;
    using FromDisney;

    public class ForecastService<T>
    {
        private ILog _log;
        private IForecastDataAccessService _dataService;
        public IForecastPlan Plan;

        private Dictionary<Tuple<string, string>, Scheme<decimal>> _schemeCache =
            new Dictionary<Tuple<string, string>, Scheme<decimal>>();

        private Dictionary<int, IMonthlyParameter<decimal>> _parameterCache =
            new Dictionary<int, IMonthlyParameter<decimal>>();


        private delegate bool RunDelegate();

        public ForecastService()
        {
            if (_dataService == null)
                _dataService = new ForecastDataAccessService();
        }

        public ForecastService(IForecastDataAccessService service)
        {
            _dataService = service;
            _log = service.GetLogger();
        }

        public void LoadPlan(int id)
        {
            this.Plan = _dataService.GetPlan(id);
        }

        public int CountPositions()
        {
            return Plan.CountPositions();
        }

        public int CountActivePositions()
        {
            return Plan.CountActivePositions();
        }

        public int CountOpenPositions()
        {
            return Plan.CountOpenPositions();
        }

        public void Run()
        {
            if (Plan.PositionLogs == null || Plan.PositionLogs.Count == 0)
            {
                _log.LogError(Plan.Id, "There are no positions to execute the forecast");
                throw new ForecastServiceException("There are no positions to execute the forecast");
            }
            if (Plan.GetStatus() == ForecastPlanStatus.Running)
            {
                _log.LogWarning(Plan.Id, "Already Running");
                throw new ForecastServiceException("Already Running");
            }
            _dataService.DeleteResults(Plan.Id);
            Plan.SetStatus(ForecastPlanStatus.Running);
            _dataService.UpdateForecastPlan(Plan);
            //_dataService.DeleteResults(Plan.Id);
            var runDelegate = new RunDelegate(ExecutePositions);
            runDelegate.BeginInvoke(RunCallback, runDelegate);
        }

        public bool ExecutePositions()
        {
            foreach (var position in Plan.PositionLogs)
            {
                //try
                //{
                //    CacheScheme(position);
                //}
                //catch (Exception)
                //{
                //    _log.LogError(Plan.Id, string.Format("There is no scheme for HCType {0} Company {1}", position.HCType.Code, position.Company.Code));
                //}
                //var scheme = _schemeCache[Tuple.Create(position.HCType.Code, position.Company.Code)];
                var scheme = _dataService.GetScheme(Plan.Id, Plan.FiscalYearId, Plan.From, Plan.To, position.PositionLogId, position.EmployeeLogId, position.HCType.Code, position.Company.Code);

                if (scheme != null)
                {
                    try
                    {
                        scheme.StartOver();
                        while (scheme.HasConceptsToRun())
                        {
                            var concept = scheme.GetCurrentConcept();
                            if (concept != null)
                            {
                                _dataService.UpdateConceptWithCurrentFunctionValues(Plan.Id, Plan.FiscalYearId, position.PositionLogId, position.EmployeeLogId,
                                                                                    concept);
                                _dataService.UpdateConceptFilterParametersWithCurrentFunctionValues(Plan.Id, Plan.FiscalYearId, position.PositionLogId, position.EmployeeLogId,
                                                                                    concept);
                                _dataService.SaveInputParameters(Plan.Id, concept);
                                scheme.RunConcept();
                                _dataService.CreateResults(Plan.Id, position.PositionLogId, new List<IConcept> { concept });
                            }
                            scheme.MoveNext();
                        }
                        _log.LogInfo(Plan.Id, string.Format("PositionLogId {0} executed", position.PositionLogId));

                    }
                    catch (Exception e)
                    {

                        _log.LogError(Plan.Id, e.Message);
                        _log.LogInfo(Plan.Id, string.Format("PositionLogId {0} NOT executed", position.PositionLogId));
                        continue;
                    }
                }
                else
                {
                    _log.LogError(Plan.Id, string.Format("There is no scheme for HCType {0} Company {1}", position.HCType.Code, position.Company.Code));
                }
            }
            //Thread.Sleep(100000);
            return true;
        }

        private void CacheScheme(IPosition position)
        {
            try
            {
                if (!_schemeCache.ContainsKey(Tuple.Create(position.HCType.Code, position.Company.Code)))
                    _schemeCache[Tuple.Create(position.HCType.Code, position.Company.Code)] =
                        _dataService.GetScheme(Plan.Id, Plan.FiscalYearId, Plan.From, Plan.To, position.PositionLogId, position.EmployeeLogId, position.HCType.Code, position.Company.Code);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public Scheme<decimal> GetSchemeById(int schemeId)
        {
            return _dataService.GetSchemeById(schemeId);
        }

        public List<Scheme<decimal>> GetSchemes(int? schemeId, int? hcTypeCode, bool? status, String createdBy, DateTime? createdDate, String modifiedBy, DateTime? modifiedDate)
        {
            return _dataService.GetSchemes(schemeId, hcTypeCode, status, createdBy, createdDate, modifiedBy, modifiedDate);
        }

        public void RunCallback(IAsyncResult asyncResult)
        {
            RunDelegate runDelegate = (RunDelegate)asyncResult.AsyncState;
            Plan.SetStatus(ForecastPlanStatus.Run);
            Plan.ExecutionDate = DateTime.Today;
            _dataService.UpdateForecastPlan(Plan);
        }

        public void CreatePlan(string name, string description, int fiscalYearId, string userName, int planFrom, int planTo)
        {
            if (_dataService.PlanExist(name))
                throw new ForecastServiceException("Plan already exists");
            if (_dataService.CreateForecastPlan(name, description, fiscalYearId, userName, planFrom, planTo))
                this.Plan = _dataService.GetPlan(name);
        }

        public ForecastPlanStatus GetStatus()
        {
            return Plan.GetStatus();
        }

        public bool DeletePlan(int id)
        {
            return _dataService.DeletePlan(id);
        }

        public void CopyPlan(int id, string userName)
        {
            try
            {
                String planName = _dataService.GetPlanName(id);
                _dataService.CopyPlan(id, planName + " - Copy - " + DateTime.Now.ToString("yyyyMMddHHmmss"), userName);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteResults()
        {
            _dataService.DeleteResults(Plan.Id);
        }

        public bool PlanExist(string planName)
        {
            return _dataService.PlanExist(planName);
        }

        public List<IForecastPlan> GetPlans()
        {
            return _dataService.GetPlans();
        }

        public bool SaveGlobalParameter(IGlobal<decimal> global)
        {
            return _dataService.SaveGlobalParameter(global);
        }

        public bool SaveConcept(Concept<decimal> concept)
        {
            return _dataService.SaveConcept(concept);
        }

        public List<ParameterDto> GetGlobalParameters(string prefixText, int count)
        {
            return _dataService.GetParameters()
                .Where(e => e.Name.ToLower().Contains(prefixText.ToLower()))
                .OrderBy(e => e.Name.ToLower().IndexOf(prefixText.ToLower()))
                .Take(count)
                .ToList();
        }

        public IGlobal<decimal> GetGlobalParameter(int id)
        {
            return _dataService.GetGlobalParameter(id);
        }

        public List<IGlobal<decimal>> GetGlobalParameters()
        {
            return _dataService.GetGlobalParameters();
        }

        public bool DeleteParameter(int id)
        {
            return _dataService.DeleteParameter(id);
        }

        public List<IGlobal<decimal>> GetGlobalParameters(int parameterId, string title, string description, int companyId, int parameterType)
        {
            var globalParameters = this.GetGlobalParameters();

            if (parameterId != 0)
                return globalParameters.Where(x => x.Id == parameterId).ToList();

            if (!string.IsNullOrEmpty(title))
                globalParameters = globalParameters.Where(x => x.Name.ToUpper().Contains(title.ToUpper())).ToList();

            if (!string.IsNullOrEmpty(description))
                globalParameters = globalParameters.Where(x => x.Description.ToUpper().Contains(description.ToUpper())).ToList();

            if (companyId != int.MinValue)
                globalParameters = globalParameters.Where(x => x.Company.Id == companyId).ToList();

            if (parameterType != int.MinValue)
                globalParameters = globalParameters.Where(x => x.ParameterType == (FromDisney.ParameterType)parameterType).ToList();

            return globalParameters;
        }

        public List<IForecastPlan> GetPlans(int planId, int? employeeId, string title, List<ForecastPlanStatus> statuses, List<int> fiscalYears)
        {
            var plans = this.GetPlans();
            if (planId != 0)
                return plans.Where(x => x.Id == planId).ToList();
            if (employeeId.HasValue)
                plans = plans.Where(x => x.CreationUserId == employeeId).ToList();
            if (!string.IsNullOrEmpty(title))
                plans = plans.Where(x => x.Title.ToUpper().Contains(title.ToUpper())).ToList();
            if (statuses != null && statuses.Count > 0)
                plans = plans.Where(x => statuses.Contains(x.GetStatus())).ToList();
            if (fiscalYears != null && fiscalYears.Count > 0)
                plans = plans.Where(x => fiscalYears.Contains(x.FiscalYearId)).ToList();
            return plans;
        }

        public int CountCurrentOpenPositions()
        {
            return _dataService.CountCurrentOpenPositions();
        }

        public int CountCurrentActivePositions()
        {
            return _dataService.CountCurrentActivePositions();
        }

        public int CountCurrentReadyToHirePositions()
        {
            return _dataService.CountCurrentReadyToHirePositions();
        }

        public int CountCurrentChangePendingPositions()
        {
            return _dataService.CountCurrentChangePendingPositions();
        }

        public List<IGlobal<decimal>> GetModifiableGlobals()
        {
            return _dataService.GetModifiableGlobals();
        }

        public List<IGlobal<decimal>> GetModifiableGlobals(int? planId, string fiscalYear)
        {
            return _dataService.GetModifiableGlobals(planId, fiscalYear);
        }

        public List<string> GetTableFields(string tableName)
        {
            return _dataService.GetTableFields(tableName);
        }

        public void UpdatePlanUploadCSV(string filePath)
        {
            if (Plan != null)
            {
                try
                {
                    Plan.UploadedFileName = filePath;
                    _dataService.UpdateForecastPlan(Plan);

                }
                catch (Exception ex)
                {
                    _log.LogError(Plan.Id, ex.Message);
                    throw ex;
                }
            }
        }

        public void ProcessCSV()
        {
            if (Plan != null && !string.IsNullOrEmpty(Plan.UploadedFileName))
            {
                var lines = File.ReadAllLines(Plan.UploadedFileName);
                if (lines[0].StartsWith("HC"))
                    lines = lines.Skip(1).ToArray();
                if (lines.Count() > 0)
                {
                    _dataService.InsertDataTable(Plan.Id, lines);
                }

            }
        }

        public void UpdatePlanUploadAndProcessCSV(string filePath)
        {
            UpdatePlanUploadCSV(filePath);
            ProcessCSV();
        }

        public void UpdatePlanModifiedGlobals(List<IGlobal<decimal>> globals)
        {
            _dataService.AddModifiedGlobalsToForecastPlan(Plan.Id, globals);
        }

        public List<Concept<decimal>> GetConceptsBySchemeId(int schemeId)
        {
            return _dataService.GetConceptsBySchemeId(schemeId);
        }

        public Concept<decimal> GetConceptById(int conceptId)
        {
            return _dataService.GetConceptById(conceptId);
        }

        #region Scheme

        public void DeleteScheme(int key, string message)
        {
            try
            {
                _dataService.DeleteScheme(key);
                _log.LogInfo(0, string.Format("Scheme {0} deleted properly", key));

            }
            catch (Exception ex)
            {
                _log.LogError(0, ex.Message);
                throw ex;
            }

        }

        public int CopyScheme(int key, Scheme scheme, out string message)
        {
            message = "Scheme Copied";
            return _dataService.CopyScheme(key, scheme);
        }

        public void UpdateScheme(Scheme scheme, out string message)
        {
            _dataService.UpdateScheme(scheme);
            message = "Scheme Updated";
        }

        public void AddScheme(Scheme scheme, out string message)
        {
            _dataService.AddScheme(scheme);
            message = "Scheme Added";
        }

        #endregion

        public void SaveAdjustments(List<SBAdjustmentDto> adjustments)
        {
            _dataService.AddAdjustmentsToPlan(Plan.Id, adjustments);
        }

        public List<SBAdjustmentDto> GetAdjustmentsByPlanId(int? planId)
        {
            return _dataService.GetAdjustmentsByPlanId(planId);
        }

        public void DeleteRelatedInfo()
        {
            _dataService.DeleteRelatedInfo(this.Plan.Id);
        }

        public void UpdatePlan(string planName, string description, int fiscalYearId, string userName, int planFrom, int planTo)
        {
            Plan.FiscalYearId = fiscalYearId;
            Plan.Title = planName;
            Plan.Description = description;
            Plan.From = planFrom;
            Plan.To = planTo;
            _dataService.UpdateForecastPlan(Plan);
        }

        public void DeleteConcept(int? schemeId, int conceptId)
        {
            _dataService.DeleteConcept(schemeId, conceptId);
        }

        public void ValidateCSV(string fileName)
        {
            

            var lines = File.ReadAllLines(fileName);
            if (lines[0].StartsWith("HC"))
                lines = lines.Skip(1).ToArray();
            if (lines.Count() > 0)
            {
                ValidateDataTable(fileName, lines);
            }

        }

        private void ValidateDataTable(string fileName, string[] lines)
        {
             var positions = new Dictionary<Tuple<int, int>, bool>();
            for (int i = 0; i < lines.Count(); i++)
            {
                var tokens = lines[i].Split(';');
                if (tokens.Count() != 14)
                    throw new Exception(string.Format("Invalid line {0} on file {1}.", i, fileName));
                int positionId;
                if (int.TryParse(tokens[0], out positionId))
                {
                    int parameterDataTypeId;
                    if (int.TryParse(tokens[1], out parameterDataTypeId))
                    {
                        if (!positions.ContainsKey(Tuple.Create(positionId, parameterDataTypeId)))
                            positions[Tuple.Create(positionId, parameterDataTypeId)] = true;
                        else
                        {
                            throw new Exception(
                                string.Format("HCPositionId + ParameterDataTypeId duplicated on line {0} on file {1}", i,
                                              fileName));
                        }
                    }
                    else
                    {
                        throw new Exception(
                            string.Format("ParameterDataTypeId invalid data on line {0} on file {1}", i,
                                          fileName));
                    }
                }
                else
                {
                    throw new Exception(
                        string.Format("HCPositionId invalid data on line {0} on file {1}", i,
                                      fileName));
                }

            }
        }

        public void CopyConcept(int? schemeId, int conceptId)
        {
            _dataService.CopyConcept(schemeId, conceptId);
        }

        public List<ParameterDataType> GetParameterDataTypes()
        {
            return _dataService.GetParameterDataTypes();
        }
    }
}
