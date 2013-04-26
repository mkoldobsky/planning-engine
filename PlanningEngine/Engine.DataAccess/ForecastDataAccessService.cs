namespace Engine.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Disney.HR.HCM.Contract;
    using Engine.Core;
    using Engine.Core.Interfaces;
    using Engine.Core.Models;
    using FromDisney;
    using CompanyDto = FromDisney.CompanyDto;
    using HCTypeDto = FromDisney.HCTypeDto;
    using System.Configuration;

    public class ForecastDataAccessService : IForecastDataAccessService
    {
        private ILog _log;

        public ForecastDataAccessService()
        {
            _log = new EmptyLog();
        }

        public ForecastDataAccessService(ILog log)
        {
            _log = log;
        }

        public bool CreateForecastPlan(string name, string description, int fiscalYearId, string userName, int planFrom, int planTo)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                context.CommandTimeout = 30;
                var fiscalYear = context.FiscalYears.FirstOrDefault(x => x.FiscalYearID == fiscalYearId);
                var forecastPlan = new SBForecastPlan
                                       {
                                           Name = name,
                                           Description = description,
                                           CreationDate = DateTime.Today,
                                           UserName = userName,
                                           SBForecastPlanStatusId = (int)ForecastPlanStatus.Creating,
                                           FiscalYear = fiscalYear,
                                           MonthFrom = planFrom,
                                           MonthTo = planTo,
                                       };
                context.AddToSBForecastPlans(forecastPlan);
                context.SaveChanges();
                context.AddCurrentPositionsToSBForecastPlan(forecastPlan.SBForecastPlanId);
                return true;
            }
            return false;
        }

        public IForecastPlan GetPlan(int id)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var forecastPlan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == id);
                if (forecastPlan != null)
                {
                    return CreateForecastPlanFromDb(context, forecastPlan);
                }
            }
            throw new Exception();
        }

        private IForecastPlan CreateForecastPlanFromDb(DisneyHCMLatamPlanningEntities context, SBForecastPlan forecastPlan)
        {
            ForecastPlan<decimal> result = CreateForecastPlan(context, forecastPlan);

            foreach (var sbForecastPlanPosition in forecastPlan.SBForecastPlanPositions.ToList())
            {
                var hcPositionLog = sbForecastPlanPosition.HCPositionLog;
                if (hcPositionLog != null)
                {
                    var position = new Position
                                       {
                                           PositionLogId = sbForecastPlanPosition.HCPositionLogId,
                                           //AnnualSalary = hcPositionLog.AnnualSalary,
                                           Company = hcPositionLog.Company != null
                                                         ? new CompanyDto { Code = hcPositionLog.Company.Code }
                                                         : new CompanyDto(),
                                           HCType = hcPositionLog.HCType != null
                                                        ? new HCTypeDto { Code = hcPositionLog.HCType.Code }
                                                        : new HCTypeDto(),
                                           Status = (HCPositionStatus)hcPositionLog.HCStatusID,
                                           EmployeeLogId = sbForecastPlanPosition.EmployeeLogId
                                       };
                    result.AddPosition(position);
                }
            }

            return result;
        }

        private ForecastPlan<decimal> CreateForecastPlan(DisneyHCMLatamPlanningEntities context, SBForecastPlan forecastPlan)
        {
            var result = new ForecastPlan<decimal>();
            result.Id = forecastPlan.SBForecastPlanId;
            result.Title = forecastPlan.Name;
            result.Description = forecastPlan.Description;
            result.Status = (ForecastPlanStatus)forecastPlan.SBForecastPlanStatusId;
            result.CreationDate = forecastPlan.CreationDate;
            result.ExecutionDate = forecastPlan.ExecutionDate;
            result.CreationUserName = forecastPlan.UserName;
            var employee = context.Employees.FirstOrDefault(x => x.UserName == forecastPlan.UserName);
            result.CreationUserName = employee != null ? employee.FullName : string.Empty;
            result.CreationUserId = employee != null ? employee.EmployeeID : 0;

            result.FiscalYearId = forecastPlan.FiscalYearId;
            result.FiscalYearCode = forecastPlan.FiscalYear.Code;
            result.From = forecastPlan.MonthFrom;
            result.To = forecastPlan.MonthTo;
            return result;
        }

        public Scheme<decimal> GetSchemeById(int schemeId)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var scheme = context.Schemes.Include("Concepts").FirstOrDefault(
                            x => x.SchemeId == schemeId);
                if (scheme != null)
                {
                    return new Scheme<decimal>
                    {
                        Id = scheme.SchemeId,
                        HCType = new Engine.Core.HCType
                        {
                            ADMWAccountID = scheme.HCType.ADMWAccountID,
                            ADMWAccountType = scheme.HCType.ADMWAccountType,
                            Id = scheme.HCType.HCTypeID,
                            Code = scheme.HCType.Code,
                            IsAdjust = scheme.HCType.IsAdjust,
                            IsTemporal = scheme.HCType.IsAdjust
                        },
                        Company = new Engine.Core.Company
                        {
                            Code = scheme.Company.Code,
                            Description = scheme.Company.Description,
                            Id = scheme.Company.CompanyID,
                            IsActive = scheme.Company.IsActive,
                            Territory = scheme.Company.TerritoryID.ToString()
                        },
                        CompanyCode = scheme.Company.Description,
                        CreatedBy = scheme.UserName,
                        CreationDate = scheme.CreationDate,
                        ModifiedUserName = scheme.ModifiedUserName,
                        ModificationDate = scheme.ModificationDate,
                        IsActive = scheme.IsActive,
                    };
                }
            }
            _log.LogError(schemeId, string.Format("There is no scheme for SchemeId {0}", schemeId));
            return null;
        }

        public Scheme<decimal> GetScheme(int planId, int fiscalYear, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, string hCTypeCode, string companyCode)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var hcType = context.HCTypes.FirstOrDefault(x => x.Code == hCTypeCode);
                var company = context.Companies.FirstOrDefault(x => x.Code == companyCode);
                if (hcType != null && company != null)
                {

                    var scheme =
                        context.Schemes.Include("Concepts").FirstOrDefault(
                            x => x.HCTypeId == hcType.HCTypeID && x.CompanyId == company.CompanyID);
                    if (scheme != null)
                        return CreateSchemeFromDb(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, scheme);
                }
            }
            _log.LogError(planId, string.Format("There is no scheme for HcTypeCode {0} CompanyCode {1}", hCTypeCode, companyCode));
            return null;
        }

        private Scheme<decimal> CreateSchemeFromDb(DisneyHCMLatamPlanningEntities context, int planId, int fiscalYear, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, Scheme scheme)
        {
            var result = new Scheme<decimal> { HCTypeCode = scheme.HCType.Code, CompanyCode = scheme.Company.Code, Id = scheme.SchemeId };
            foreach (var concept in scheme.Concepts.OrderBy(x => x.Sequence).ToList())
            {
                result.AddConcept(CreateConceptFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept));
            }
            return result;

        }

        private IConcept CreateConceptFromDB(DisneyHCMLatamPlanningEntities context, int planId, int fiscalYear, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, Concept concept)
        {
            try
            {
                var result = new Concept<decimal>(concept.Operation)
                {
                    Title = concept.Title,
                    From = monthFrom,
                    To = monthTo,
                    Description = concept.Description,
                    Id = concept.ConceptId,
                    Sequence = concept.Sequence,
                    GLAccountId = concept.GLAccountID
                };

                result.Filters = CreateFiltersFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.ConceptFilters);

                var parameters = new List<IMonthlyParameter<decimal>>();
                result.Parameter1 = CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Parameter1, null);
                if (result.Parameter1 != null)
                    parameters.Add(result.Parameter1);
                result.Parameter2 = CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Parameter2, null);
                if (result.Parameter2 != null)
                    parameters.Add(result.Parameter2);
                result.Parameter3 = CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Parameter3, null);
                if (result.Parameter3 != null)
                    parameters.Add(result.Parameter3);
                result.Parameter4 = CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Parameter4, null);
                if (result.Parameter4 != null)
                    parameters.Add(result.Parameter4);
                result.SetParameters(parameters);
                if (concept.Output1 != null)
                    result.AddOutputParameter1(CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Output1, null));
                if (concept.Output2 != null)
                    result.AddOutputParameter2(CreateParameterFromDB(context, planId, fiscalYear, monthFrom, monthTo, positionLogId, employeeLogId, concept.Output2, null));


                return result;

            }
            catch (Exception ex)
            {
                _log.LogError(planId, ex.Message);
            }
            return null;
        }

        private List<IFIlter<bool>> CreateFiltersFromDB(DisneyHCMLatamPlanningEntities context, int planId, int fiscalYearId, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, System.Data.Objects.DataClasses.EntityCollection<ConceptFilter> conceptFilters)
        {
            var result = new List<IFIlter<bool>>();

            foreach (var conceptFilter in conceptFilters.ToList())
            {
                var filter = conceptFilter.Filter;
                IMonthlyParameter<decimal> parameter1 = null, parameter2 = null;
                if (filter.Parameter != null)
                    parameter1 = CreateParameterFromDB(context, planId, fiscalYearId, monthFrom, monthTo, positionLogId, employeeLogId,
                                                      filter.Parameter, filter.FilterTypeId);
                if (filter.Parameter1 != null)
                    parameter2 = CreateParameterFromDB(context, planId, fiscalYearId, monthFrom, monthTo, positionLogId, employeeLogId,
                                                      filter.Parameter1, filter.FilterTypeId);

                if (parameter1 != null && parameter2 != null)
                {
                    result.Add(new Core.Filter<decimal, bool>
                                   {
                                       Sequence = conceptFilter.Sequence,
                                       Operation = new Operation(filter.Operation),
                                       Connector = conceptFilter.Connector != null ? new Operation(conceptFilter.Connector) : null,
                                       Id = filter.FilterId,
                                       Parameter1 = parameter1,
                                       Parameter2 = parameter2,
                                       FilterType = filter.FilterTypeId
                                   });
                }
            }
            return result;
        }

        private IMonthlyParameter<decimal> CreateParameterFromDB(DisneyHCMLatamPlanningEntities context, int planId, int fiscalYearId, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, Parameter parameter, int? filterType)
        {
            IMonthlyParameter<decimal> result = null;
            if (parameter != null)
            {
                switch (parameter.ParameterType.ParameterTypeId)
                {
                    case (int)FromDisney.ParameterType.FixedValue:
                        result = new MonthlyParameter<decimal>
                        {
                            Name = parameter.Name,
                            Id = parameter.ParameterId,
                            IsAccumulator = parameter.IsAccumulator,
                        };
                        decimal value;
                        if (!Decimal.TryParse(parameter.FixedValue, out value))
                            value = 0;
                        result.Value = SetDictionaryFromValue(value);
                        break;
                    case (int)FromDisney.ParameterType.Dynamic:
                        result = new MonthlyParameter<decimal>
                                     {
                                         Name = parameter.Name,
                                         Id = parameter.ParameterId,
                                         IsAccumulator = parameter.IsAccumulator,
                                         Value = SetDictionaryFromValue(0)
                                     };
                        break;
                    case (int)FromDisney.ParameterType.Constant:
                        List<ModifiedGlobalsMonthValue> modifiedGlobal = null;
                        if (parameter.IsModifiable)
                        {
                            var globalValues =
                                context.SBForecastPlanModifiedGlobals.FirstOrDefault(
                                    x => x.SBForecastPlanId == planId && x.ParameterId == parameter.ParameterId);
                            if (globalValues != null)
                                modifiedGlobal = globalValues.ModifiedGlobalsMonthValues.ToList();

                        }
                        result = new Global<decimal>
                                     {
                                         Name = parameter.Name,
                                         Id = parameter.ParameterId,
                                         IsAccumulator = parameter.IsAccumulator,
                                         IsConstant = parameter.IsConstant,
                                         IsModifiable = parameter.IsModifiable,
                                         Value =
                                             modifiedGlobal == null
                                                 ? AssignValue(parameter.ParameterValues.FirstOrDefault(x => x.FiscalYearID == fiscalYearId), monthFrom, monthTo)
                                                 : SetDictionaryFromModifiedValues(modifiedGlobal)
                                     };
                        break;
                    case (int)FromDisney.ParameterType.Function:
                        result = new Function<decimal>
                                     {
                                         Name = parameter.Name,
                                         Id = parameter.ParameterId,
                                         IsAccumulator = parameter.IsAccumulator,
                                         ColumnName = parameter.ColumnName,
                                         TableName = parameter.TableName,
                                     };
                        if (parameter.ParameterDataType != null)
                        {
                            result.Value = GetMonthlyDataValue(context, planId, fiscalYearId, positionLogId, parameter);
                        }
                        else
                        {
                            result.Value =
                                /*GetMonthlyFunctionValue(context, positionLogId, employeeLogId, parameter.TableName,
                                                        parameter.ColumnName, filterType);*/
                                GetMonthlyFunctionValue(context, positionLogId, employeeLogId, parameter.TableName,
                                                    parameter.ColumnName);
                        }
                        break;
                }

                return result;
            }
            return null;
        }

        private Dictionary<Month, decimal> SetDictionaryFromModifiedValues(List<ModifiedGlobalsMonthValue> modifiedGlobals)
        {
            var result = new Dictionary<Month, decimal>();
            var global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.January);
            result[Month.January] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.February);
            result[Month.February] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.March);
            result[Month.March] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.April);
            result[Month.April] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.May);
            result[Month.May] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.June);
            result[Month.June] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.July);
            result[Month.July] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.August);
            result[Month.August] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.September);
            result[Month.September] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.October);
            result[Month.October] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.November);
            result[Month.November] = global == null ? 0 : global.Value;
            global = modifiedGlobals.FirstOrDefault(x => x.Month == (int)Month.December);
            result[Month.December] = global == null ? 0 : global.Value;
            return result;

        }

        private Dictionary<Month, decimal> GetMonthlyDataValue(DisneyHCMLatamPlanningEntities context, int planId, int fiscalYear, int positionId, Parameter parameter)
        {
            var parameterDataType = parameter.ParameterDataType;
            if (!parameterDataType.IsActive)
            {
                _log.LogError(planId, string.Format("ParameterDataType {0} inactive", parameterDataType.Name));
                return null;
            }
            var planPosition =
                context.SBForecastPlanPositions.FirstOrDefault(
                    x => x.HCPositionLogId == positionId && x.SBForecastPlanId == planId);
            if (planPosition != null)
            {
                var planData =
                    parameterDataType.SBForecastPlanDatas.Where(
                        x =>
                        x.SBForecastPlanPositionId == planPosition.SBForecastPlanPositionId &&
                        x.SBForecastPlanPosition.SBForecastPlan.SBForecastPlanId == planId).ToList();
                if (planData.Count == 0)
                {
                    _log.LogWarning(planId,
                                    string.Format(
                                        "ParameterDataType {0} active but not related to positionLogId {1} and SBForecastPlanId {2}",
                                        parameterDataType.Name, positionId, planId));
                    return null;
                }
                return SetDictionaryFromPlanData(planData, fiscalYear);
            }
            return null;

        }

        private static Dictionary<Month, decimal> SetDictionaryFromPlanData(List<SBForecastPlanData> planData, int fiscalYear)
        {
            var result = new Dictionary<Month, decimal>();
            foreach (Month month in Enum.GetValues(typeof(Month)))
            {
                var data = planData.FirstOrDefault(x => x.Month == (int)month);
                if (data != null)
                    result[month] = data.Value;
            }
            return result;
        }

        public static Dictionary<Month, decimal> SetDictionaryFromValue(decimal value)
        {
            var result = new Dictionary<Month, decimal>();
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

        /*private Dictionary<Month, decimal> GetMonthlyFunctionValue(DisneyHCMLatamPlanningEntities context, int positionLogId, int? employeeLogId, string tableName, string columnName, int? filterType)
        {
            var id = positionLogId;
            string query = "select [{0}] from [{1}] where HCPositionLogId = {2}";

            if (tableName == "EmployeeLogs")
            {
                query = "select [{0}] from [{1}] where EmployeeLogId = {2}";
                id = employeeLogId.HasValue ? employeeLogId.Value : 0;
            }

            if (columnName == "AnnualSalary")
            {
                query = "select CONVERT (varchar(50),DECRYPTBYPASSPHRASE('{0}', AnnualSalary)) from [{1}] where HCPositionLogId = {2}";
                var result = context.ExecuteStoreQuery<string>(
                    string.Format(query, ConfigurationManager.AppSettings["SQLPassPhrase"], tableName, id), null).FirstOrDefault();
                return SetDictionaryFromValue(Convert.ToDecimal(result));
            }

            decimal? value = null;
            if (filterType == null)
            {
                try
                {
                    var result = context.ExecuteStoreQuery<decimal?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!result.HasValue)
                        result = 0;
                    return SetDictionaryFromValue(result.Value);
                }
                catch (Exception ex)
                {
                    var it = context.ExecuteStoreQuery<int?>(string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!it.HasValue)
                        it = 0;
                    return SetDictionaryFromValue(Convert.ToDecimal(it.Value));
                }
            }
            switch (filterType)
            {
                case (int)FilterTypes.Integer:
                    var it = context.ExecuteStoreQuery<int?>(string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!it.HasValue)
                        it = 0;
                    return SetDictionaryFromValue(Convert.ToDecimal(it.Value));
                    break;
                case (int)FilterTypes.Byte:
                    var bt = context.ExecuteStoreQuery<byte?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!bt.HasValue)
                        bt = 0;

                    return SetDictionaryFromValue(Convert.ToDecimal(bt.Value));
                    break;
                case (int)FilterTypes.Decimal:
                    var dc = context.ExecuteStoreQuery<decimal?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!dc.HasValue)
                        dc = 0;

                    return SetDictionaryFromValue(Convert.ToDecimal(dc.Value));
                    break;
                case (int)FilterTypes.Bit:
                    var bit = context.ExecuteStoreQuery<bool?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!bit.HasValue)
                        bit = false;

                    return SetDictionaryFromValue(bit.Value ? 1 : 0);
                    break;
                default:
                    return null;
            }
        }*/

        private Dictionary<Month, decimal> GetMonthlyFunctionValue(DisneyHCMLatamPlanningEntities context, int positionLogId, int? employeeLogId, string tableName, string columnName)
        {
            var id = positionLogId;
            string query = "select [{0}] from [{1}] where HCPositionLogId = {2}";
            string query2 = string.Format("SELECT c.name FROM sys.objects a, sys.columns b, sys.types c where a.name ='{0}' and a.OBJECT_ID = b.OBJECT_ID and c.system_type_id = b.system_type_id and b.name ='{1}'", tableName, columnName);
            var tipoDeDato = context.ExecuteStoreQuery<string>(query2, null).FirstOrDefault();

            if (tableName == "EmployeeLogs")
            {
                query = "select [{0}] from [{1}] where EmployeeLogId = {2}";
                id = employeeLogId.HasValue ? employeeLogId.Value : 0;
            }

            if (columnName == "AnnualSalary")
            {
                query = "select CONVERT (varchar(50),DECRYPTBYPASSPHRASE('{0}', AnnualSalary)) from [{1}] where HCPositionLogId = {2}";
                var result = context.ExecuteStoreQuery<string>(
                    string.Format(query, ConfigurationManager.AppSettings["SQLPassPhrase"], tableName, id), null).FirstOrDefault();
                return SetDictionaryFromValue(Convert.ToDecimal(result));
            }

            switch (tipoDeDato)
            {
                case "int":
                    var it = context.ExecuteStoreQuery<int?>(string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!it.HasValue)
                        it = 0;
                    return SetDictionaryFromValue(Convert.ToDecimal(it.Value));
                case "tinyint":
                    var tnit = context.ExecuteStoreQuery<byte?>(string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!tnit.HasValue)
                        tnit = 0;
                    return SetDictionaryFromValue(Convert.ToDecimal(tnit.Value));
                case "decimal":
                    var dc = context.ExecuteStoreQuery<decimal?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!dc.HasValue)
                        dc = 0;

                    return SetDictionaryFromValue(Convert.ToDecimal(dc.Value));
                case "bit":
                    var bit = context.ExecuteStoreQuery<bool?>(
                        string.Format(query, columnName, tableName, id), null).FirstOrDefault();
                    if (!bit.HasValue)
                        bit = false;

                    return SetDictionaryFromValue(bit.Value ? 1 : 0);
                default:
                    return null;
            }
        }

        private Dictionary<Month, decimal> AssignValue(ParameterValue parameterValue, int? monthFrom, int? monthTo)
        {

            var result = new Dictionary<Month, decimal>();
            if (parameterValue == null)
                return result;
            var value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.January);
            result[Month.January] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.February);
            result[Month.February] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.March);
            result[Month.March] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.April);
            result[Month.April] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.May);
            result[Month.May] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.June);
            result[Month.June] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.July);
            result[Month.July] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.August);
            result[Month.August] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.September);
            result[Month.September] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.October);
            result[Month.October] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.November);
            result[Month.November] = value != null ? value.Value : 0;

            value = parameterValue.ParameterMonthValues.FirstOrDefault(x => x.Month == (int)Month.December);
            result[Month.December] = value != null ? value.Value : 0;

            return result;
        }

        public bool DeletePlan(int id)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans
                    .Include("SBForecastPlanPositions")
                    .Include("SBForecastPlanAdjustments")
                    .Include("SBForecastPlanModifiedGlobals")
                    .FirstOrDefault(x => x.SBForecastPlanId == id);
                if (plan != null)
                {
                    foreach (var sbForecastPlanAdjustment in plan.SBForecastPlanAdjustments.ToList())
                    {
                        context.DeleteObject(sbForecastPlanAdjustment);
                    }
                    foreach (var sbForecastPlanModifiedGlobal in plan.SBForecastPlanModifiedGlobals.ToList())
                    {
                        var values = sbForecastPlanModifiedGlobal.ModifiedGlobalsMonthValues.ToList();
                        foreach (var modifiedGlobalsMonthValue in values)
                        {
                            context.ModifiedGlobalsMonthValues.DeleteObject(modifiedGlobalsMonthValue);
                        }
                        context.DeleteObject(sbForecastPlanModifiedGlobal);
                    }
                    foreach (var sbForecastPlanPosition in plan.SBForecastPlanPositions.ToList())
                    {
                        SBForecastPlanPosition position = sbForecastPlanPosition;
                        var conceptResults =
                            context.ConceptResults.Where(
                                x => x.SBForecastPlanPositionId == position.SBForecastPlanPositionId);
                        foreach (var conceptResult in conceptResults.ToList())
                        {
                            context.ConceptResults.DeleteObject(conceptResult);
                        }
                        foreach (var planData in position.SBForecastPlanDatas.ToList())
                        {
                            context.SBForecastPlanDatas.DeleteObject(planData);
                        }
                        context.DeleteObject(sbForecastPlanPosition);
                    }
                    context.DeleteObject(plan);
                    context.SaveChanges();
                }
                return true;
            }
        }

        public bool CopyPlan(int id, string newName, string userName)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                try
                {
                    var plan = context.SBForecastPlans
                        .Include("SBForecastPlanModifiedGlobals")
                        .Include("SBForecastPlanModifiedGlobals.ModifiedGlobalsMonthValues")
                        .FirstOrDefault(x => x.SBForecastPlanId == id);
                    if (plan == null)
                        return false;
                    var newPlan = new SBForecastPlan
                                      {
                                          CreationDate = DateTime.Today,
                                          Description = plan.Description + " - Copy",
                                          ExecutionDate = null,
                                          FiscalYear = plan.FiscalYear,
                                          MonthFrom = plan.MonthFrom,
                                          MonthTo = plan.MonthTo,
                                          Name = newName,
                                          SBForecastPlanStatusId = (int)ForecastPlanStatus.Creating,
                                          UserName = userName,
                                      };
                    this.CopyPlanPositions(plan, newPlan);
                    this.CopyAdjustments(context, plan, newPlan);
                    this.CopyModifiedGlobals(context, plan, newPlan);
                    // do not copy results
                    context.SBForecastPlans.AddObject(newPlan);
                    context.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    _log.LogError(id, ex.Message);
                    throw ex;
                }
            }

        }

        private void CopyModifiedGlobals(DisneyHCMLatamPlanningEntities context, SBForecastPlan plan, SBForecastPlan newPlan)
        {
            foreach (var modifiedGlobal in plan.SBForecastPlanModifiedGlobals.ToList())
            {
                SBForecastPlanModifiedGlobal global1 = modifiedGlobal;
                var newModifiedGlobal = context.SBForecastPlanModifiedGlobals.FirstOrDefault(x => x.SBForecastPlanModifiedGlobalId == global1.SBForecastPlanModifiedGlobalId);
                newModifiedGlobal.SBForecastPlan = null;
                context.Detach(newModifiedGlobal);
                var modifiedGlobalMonthValues =
                    context.ModifiedGlobalsMonthValues.Where(
                        x => x.SBForecastPlanModifiedGlobalId == global1.SBForecastPlanModifiedGlobalId);
                foreach (var modifiedGlobalMonthValue in modifiedGlobalMonthValues)
                {
                    var newGlobalMonthValue = modifiedGlobalMonthValue;
                    newGlobalMonthValue.SBForecastPlanModifiedGlobal = null;
                    context.Detach(newGlobalMonthValue);
                    newModifiedGlobal.ModifiedGlobalsMonthValues.Add(newGlobalMonthValue);
                }

                newPlan.SBForecastPlanModifiedGlobals.Add(newModifiedGlobal);
            }
        }

        private void CopyAdjustments(DisneyHCMLatamPlanningEntities context, SBForecastPlan plan, SBForecastPlan newPlan)
        {
            foreach (var adjustment in plan.SBForecastPlanAdjustments.ToList())
            {
                var newAdjustment = adjustment;
                newAdjustment.SBForecastPlan = null;
                context.Detach(newAdjustment);

                newPlan.SBForecastPlanAdjustments.Add(adjustment);
            }
        }

        private void CopyPlanPositions(SBForecastPlan plan, SBForecastPlan newPlan)
        {
            foreach (var position in plan.SBForecastPlanPositions)
            {
                newPlan.SBForecastPlanPositions.Add(new SBForecastPlanPosition { HCPositionLog = position.HCPositionLog, EmployeeLog = position.EmployeeLog });
            }
        }

        public Dictionary<Month, decimal> GetFunctionValuesByPlanAndPosition(int planId, int positionId, int functionId)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var function = context.Parameters.FirstOrDefault(x => x.ParameterId == functionId);
                if (function != null)
                {
                    var result = context.ExecuteStoreQuery<Decimal>(
                        string.Format("select [{0}] from [{1}] where HCPositionLogId = {2}", function.ColumnName,
                                      function.TableName, positionId), null).FirstOrDefault();
                    return SetDictionaryFromValue(result);
                }
            }
            return null;
        }

        public void CreateResults(int planId, int positionLogId, List<IConcept> concepts)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                foreach (var concept in concepts)
                {
                    if (concept.ShouldSave())
                    {
                        try
                        {
                            var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == planId);
                            var positionLog = context.HCPositionLogs.FirstOrDefault(x => x.HCPositionLogID == positionLogId);
                            var planPosition =
                                context.SBForecastPlanPositions.FirstOrDefault(
                                    x => x.SBForecastPlanId == planId && x.HCPositionLogId == positionLogId);
                            IConcept concept1 = concept;
                            var conceptOriginal = context.Concepts.FirstOrDefault(x => x.ConceptId == concept1.Id);
                            //SaveParameters(planId, concept1, " post run ");
                            foreach (Month month in Enum.GetValues(typeof(Month)))
                            {
                                if (!MonthInPlan(plan, month))
                                    continue;
                                if (conceptOriginal.ConceptsValidMonths.FirstOrDefault(x => x.MonthId == (int)month) != null && concept1.IsMonthFiltered(month))
                                {
                                    var value = ((Concept<decimal>)concept1).ConceptResult.Value;
                                    if (!value.ContainsKey(month))
                                        continue;
                                    if (value[month] != default(decimal))
                                        //SaveNotEncripted(concept1, month, conceptOriginal, ((Concept<decimal>)concept1).ConceptResult.Value[month], planPosition);
                                        SaveEncripted(context, concept1, month, conceptOriginal, ((Concept<decimal>)concept1).ConceptResult.Value[month], planPosition);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _log.LogError(planId, e.Message);

                            throw;
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        private void SaveEncripted(DisneyHCMLatamPlanningEntities context, IConcept concept, Month month, Concept conceptOriginal, decimal value, SBForecastPlanPosition planPosition)
        {
            const string query = "insert into ConceptResults (ConceptId, SBForecastPlanPositionId, CalendarMonthId, Value) values ({0}, {1}, {2}, ENCRYPTBYPASSPHRASE('{3}', '{4}'))";

            var result = context.ExecuteStoreCommand(string.Format(query, conceptOriginal.ConceptId, planPosition.SBForecastPlanPositionId, (int)month, ConfigurationManager.AppSettings["SQLPassPhrase"], value.ToString()), null);
        }

        //private void SaveNotEncripted(IConcept concept1, Month month, Concept conceptOriginal, decimal value, SBForecastPlanPosition planPosition)
        //{
        //    var conceptResult = new ConceptResult
        //                            {
        //                                SBForecastPlanPosition = planPosition,
        //                                Concept = conceptOriginal,
        //                                CalendarMonthId = (int)month,
        //                                Value = value,

        //                            };

        //    planPosition.ConceptResults.Add(conceptResult);
        //}

        private bool MonthInPlan(SBForecastPlan plan, Month month)
        {
            return plan.MonthFrom <= (int)month && plan.MonthTo >= (int)month;
        }

        public bool DeleteResults(int planId)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == planId);

                var planPositions = plan.SBForecastPlanPositions.ToList();

                if (planPositions != null)
                {
                    foreach (var sbForecastPlanPosition in planPositions)
                    {
                        foreach (var conceptResult in sbForecastPlanPosition.ConceptResults.ToList())
                        {
                            ConceptResult result = conceptResult;
                            var resultParameters =
                                context.ResultParameters.Where(
                                    x => x.ConceptResultId == result.ConceptResultId);
                            foreach (var resultParameter in resultParameters.ToList())
                            {
                                context.ResultParameters.DeleteObject(resultParameter);
                                context.ConceptResults.DeleteObject(conceptResult);
                            }
                            context.DeleteObject(conceptResult);
                        }

                    }
                    context.SaveChanges();
                }
                return true;
            }

        }

        public bool PlanExist(string planName)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.SBForecastPlans.FirstOrDefault(x => x.Name == planName) != null;
            }
        }

        public IForecastPlan GetPlan(string name)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var forecastPlan = context.SBForecastPlans.FirstOrDefault(x => x.Name == name);
                if (forecastPlan != null)
                {
                    return CreateForecastPlanFromDb(context, forecastPlan);
                }
            }
            throw new Exception();
        }

        public List<IForecastPlan> GetPlans()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var result = new List<IForecastPlan>();
                var plans = context.SBForecastPlans.ToList();
                foreach (var sbForecastPlan in plans)
                {
                    result.Add(CreateForecastPlanFromDbWithoutPositions(context, sbForecastPlan));
                }
                return result;
            }
        }

        public int CountCurrentOpenPositions()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.HCPositions.Count(x => x.HCStatusID == (int)HCPositionStatus.Open);
            }
        }

        public int CountCurrentActivePositions()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.HCPositions.Count(x => x.HCStatusID == (int)HCPositionStatus.Active);
            }
        }

        public int CountCurrentReadyToHirePositions()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.HCPositions.Count(x => x.HCStatusID == (int)HCPositionStatus.ReadyToHire);
            }
        }

        public int CountCurrentChangePendingPositions()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.HCPositions.Count(x => x.HCStatusID == (int)HCPositionStatus.ChangePending);
            }
        }

        public void UpdateForecastPlan(IForecastPlan plan)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var forecastPlan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == plan.Id);
                if (forecastPlan == null) return;
                var newStatusId = (int)plan.GetStatus();
                var status = context.SBForecastPlanStatuses.FirstOrDefault(x => x.SBForecastPlanStatusID == newStatusId);
                if (status != null)
                    forecastPlan.SBForecastPlanStatus = status;
                forecastPlan.Description = plan.Description;
                forecastPlan.Name = plan.Title;
                forecastPlan.ExecutionDate = plan.ExecutionDate;
                forecastPlan.UploadedFileName = plan.UploadedFileName;
                if (plan.From.HasValue)
                    forecastPlan.MonthFrom = plan.From.Value;
                if (plan.To.HasValue)
                    forecastPlan.MonthTo = plan.To.Value;
                _log.LogInfo(forecastPlan.SBForecastPlanId, string.Format("Plan updated -- new status {0}", forecastPlan.SBForecastPlanStatus.Description));
                context.SaveChanges();
            }
        }

        public List<IGlobal<decimal>> GetModifiableGlobals()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var globals =
                    context.Parameters.Where(x => x.ParameterTypeId == (int)FromDisney.ParameterType.Constant && x.IsModifiable
                    || x.ParameterTypeId == (int)FromDisney.ParameterType.Dynamic && x.IsModifiable).ToList();

                var list = new List<IGlobal<decimal>>();

                globals.ForEach(x => list.Add(CreateGlobalsFromDb(x)));

                return list;
            }
        }

        public List<IGlobal<decimal>> GetModifiableGlobals(int? planId, String fiscalYear)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var globals =
                    context.Parameters.Where(
                        x => x.ParameterTypeId == (int)FromDisney.ParameterType.Constant && x.IsModifiable).ToList();

                var list = new List<IGlobal<decimal>>();

                globals.ForEach(x => list.Add(CreateGlobalsFromDb(context, planId, x, fiscalYear)));

                return list;
            }
        }

        public void InsertDataTable(int planId, string[] lines)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == planId);
                if (plan != null)
                {
                    foreach (var line in lines)
                    {
                        var tokens = line.Split(';');
                        if (tokens.Count() == 14)
                        {
                            int positionId;
                            if (int.TryParse(tokens[0], out positionId))
                            {
                                var planPosition =
                                    plan.SBForecastPlanPositions.FirstOrDefault(
                                        x => x.HCPositionLog.HCPositionID == positionId);
                                _log.LogError(planId, "There are no positions to execute the forecast");
                                if (planPosition != null)
                                {
                                    int parameterDataTypeId;
                                    if (int.TryParse(tokens[1], out parameterDataTypeId))
                                    {
                                        var parameterDataType = context.ParameterDataTypes.FirstOrDefault(
                                            x => x.ParameterDataTypeID == parameterDataTypeId);
                                        for (int i = 1; i < 13; i++)
                                        {
                                            decimal value;
                                            if (decimal.TryParse(tokens[i + 1], out value))
                                            {
                                                var planData = new SBForecastPlanData
                                                                   {
                                                                       ParameterDataType = parameterDataType,
                                                                       Month = i,
                                                                       Value = value,
                                                                   };
                                                planPosition.SBForecastPlanDatas.Add(planData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        public void AddModifiedGlobalsToForecastPlan(int planId, List<IGlobal<decimal>> globals)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans
                    .Include("SBForecastPlanModifiedGlobals")
                    .Include("SBForecastPlanModifiedGlobals.ModifiedGlobalsMonthValues")
                    .FirstOrDefault(x => x.SBForecastPlanId == planId);
                if (plan != null)
                {
                    foreach (var global in globals.ToList())
                    {
                        IGlobal<decimal> global1 = global;
                        var parameter = context.Parameters.FirstOrDefault(x => x.ParameterId == global1.Id);
                        var modifiedGlobal = new SBForecastPlanModifiedGlobal { SBForecastPlan = plan, Parameter = parameter };
                        context.AddToSBForecastPlanModifiedGlobals(modifiedGlobal);
                        context.SaveChanges();
                        foreach (Month month in Enum.GetValues(typeof(Month)))
                        {
                            var monthValue = new ModifiedGlobalsMonthValue { Month = (int)month, Value = global1.Value[month], SBForecastPlanModifiedGlobal = modifiedGlobal };
                            modifiedGlobal.ModifiedGlobalsMonthValues.Add(monthValue);
                        }

                        context.SaveChanges();
                    }
                }
            }
        }

        public string GetPlanName(int id)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == id);
                return plan != null ? plan.Name : null;
            }
        }

        public List<string> GetTableFields(string tableName)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                //string query = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('dbo." + tableName + "')";
                string query = "SELECT b.name FROM sys.objects a, sys.columns b where a.object_id = OBJECT_ID('dbo." + tableName + "') and a.OBJECT_ID = b.OBJECT_ID and b.system_type_id in (48,56,104,165)";
                var result = context.ExecuteStoreQuery<string>(
                string.Format(query), null);
                return result.ToList();
            }
        }

        public ILog GetLogger()
        {
            return _log;
        }

        public void CleanAccumulableParameters(List<IMonthlyParameter<decimal>> accumulableParameters)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                foreach (var accumulableParameter in accumulableParameters)
                {
                    IMonthlyParameter<decimal> parameter1 = accumulableParameter;
                    var parameter = context.Parameters.FirstOrDefault(x => x.ParameterId == parameter1.Id);
                    if (parameter != null)
                    {
                        foreach (var parameterValue in parameter.ParameterValues.ToList())
                        {
                            context.DeleteObject(parameterValue);
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        public void SaveInputParameters(int planId, IConcept concept)
        {
            //TODO implement SaveInputParameters
            //SaveParameters(planId, concept, " pre run ");
        }

        public bool SaveConcept(Concept<decimal> concept)
        {
            try
            {
                using (var context = new DisneyHCMLatamPlanningEntities())
                {
                    var scheme = context.Schemes
                        .Include("Concepts").FirstOrDefault(x => x.SchemeId == concept.Scheme.Id);
                    var conceptWithSameSequence = scheme.Concepts.FirstOrDefault(x => x.Sequence == concept.Sequence);
                    if (concept.Id > 0 && conceptWithSameSequence != null)
                    {
                        var originalConcept = context.Concepts.FirstOrDefault(x => x.ConceptId == concept.Id);
                        if (originalConcept != null)
                            UpdateSequences(scheme, originalConcept, concept.Sequence);
                        scheme.Concepts.Remove(originalConcept);
                    }
                    else
                        if (conceptWithSameSequence != null)
                        {
                            InsertSequence(scheme, concept.Sequence);
                        }

                    var model = concept.ToDbModel(context);

                    if (model.ConceptId == 0)
                    {
                        scheme.Concepts.Add(model);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private void UpdateSequences(Scheme scheme, Concept originalConcept, int sequence)
        {
            if (originalConcept.Sequence == sequence)
                return;
            if (originalConcept.Sequence < sequence)
                foreach (var concept in scheme.Concepts.ToList())
                {
                    if (concept.Sequence > originalConcept.Sequence && concept.Sequence <= sequence)
                        concept.Sequence--;
                }
            if (originalConcept.Sequence > sequence)
                foreach (var concept in scheme.Concepts.ToList())
                {
                    if (concept.Sequence < originalConcept.Sequence && concept.Sequence >= sequence)
                        concept.Sequence++;
                }
        }


        private void InsertSequence(Scheme scheme, int sequence)
        {
            foreach (var concept in scheme.Concepts.ToList())
            {
                if (concept.Sequence >= sequence)
                    concept.Sequence++;
            }
        }

        private void SaveParameters(int planId, IConcept concept, string message)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var parameters = concept.GetParameters();
                foreach (IMonthlyParameter<decimal> parameter in parameters)
                {
                    foreach (Month month in Enum.GetValues(typeof(Month)))
                    {
                        if (parameter.Value != null && parameter.Value.ContainsKey(month))
                            _log.LogInfo(planId, string.Format("Concept {0} - {1} - Input parameter {2} month {3} value {4}", concept.GetName(), message, parameter.Name, month, parameter.Value != null && parameter.Value.Count > 0 ? parameter.Value[month].ToString() : "no value"));
                    }
                }
                parameters = concept.GetOutputParameters();
                foreach (IMonthlyParameter<decimal> parameter in parameters)
                {
                    foreach (Month month in Enum.GetValues(typeof(Month)))
                    {
                        if (parameter.Value != null && parameter.Value.ContainsKey(month))
                            _log.LogInfo(planId, string.Format("Concept {0} - pre run - Output parameter {1} month {2} value {3}", concept.GetName(), parameter.Name, month, parameter.Value != null && parameter.Value.Count > 0 ? parameter.Value[month].ToString() : "no value"));
                    }
                }
            }
        }

        public void UpdateConceptWithCurrentFunctionValues(int planId, int fiscalYearId, int positionLogId, int? employeeLogId, IConcept concept)
        {
            var parameters = concept.GetParameters();
            foreach (var parameter in parameters)
            {
                var function = parameter as Function<decimal>;
                if (function == null) continue;
                using (var context = new DisneyHCMLatamPlanningEntities())
                {
                    var parameterFromDB = context.Parameters.FirstOrDefault(x => x.ParameterId == function.Id);
                    if (parameterFromDB.ParameterDataType != null)
                    {
                        function.Value = GetMonthlyDataValue(context, planId, fiscalYearId, positionLogId, parameterFromDB);
                    }
                    else
                    {
                        /*function.Value = GetMonthlyFunctionValue(context, positionLogId, employeeLogId, function.TableName,
                                                                 function.ColumnName, null);*/
                        function.Value = GetMonthlyFunctionValue(context, positionLogId, employeeLogId, function.TableName,
                                                                 function.ColumnName);
                    }
                }
            }
        }

        private IGlobal<decimal> CreateGlobalsFromDb(Parameter parameter)
        {
            var context = new DisneyHCMLatamPlanningEntities();

            var global = new Global<decimal>
            {
                Name = parameter.Name,
                Description = parameter.Description,
                Id = parameter.ParameterId,
                IsModifiable = parameter.IsModifiable,
                IsAccumulator = parameter.IsAccumulator,
                IsConstant = parameter.IsConstant,
                ParameterType = (FromDisney.ParameterType)parameter.ParameterTypeId,
                Company = (context.Companies.FirstOrDefault(p => p.CompanyID == parameter.CompanyId).ToModel()),
                Value = SetDictionaryFromValue(0)
            };

            if (global.ParameterType == FromDisney.ParameterType.Constant)
            {
                global.MonthlyParameter = new List<MonthlyParameter<decimal>>();

                var monthlyValues = context.ParameterValues
                                    .Include("ParameterMonthValues.ParameterValue.FiscalYear")
                                    .Where(p => p.ParameterId == parameter.ParameterId)
                                    .ToList();

                monthlyValues.ForEach(x =>
                {
                    var monthlyParameter = new MonthlyParameter<decimal>
                    {
                        FiscalYearId = x.FiscalYear.FiscalYearID,
                        FiscalYearCode = x.FiscalYear.Code,
                        Value = new Dictionary<Month, decimal>()
                    };

                    for (int i = 1; i <= 12; i++)
                    {
                        decimal value = 0;

                        try
                        {
                            value = x.ParameterMonthValues.ToList()[i - 1].Value;
                        }
                        catch (Exception)
                        {
                            value = 0;
                        }

                        monthlyParameter.Value.Add((Month)i, value);
                    }

                    global.MonthlyParameter.Add(monthlyParameter);
                });
            }
            return global;
        }

        private IGlobal<decimal> CreateGlobalsFromDb(DisneyHCMLatamPlanningEntities context, int? planId, Parameter parameter, string fiscalYear)
        {
            List<ModifiedGlobalsMonthValue> modifiedGlobal = null;
            if (parameter.IsModifiable)
            {
                var globalValues =
                    context.SBForecastPlanModifiedGlobals.FirstOrDefault(
                        x => x.SBForecastPlanId == planId && x.ParameterId == parameter.ParameterId);
                if (globalValues != null)
                    modifiedGlobal = globalValues.ModifiedGlobalsMonthValues.ToList();

            }
            var global = new Global<decimal>
            {
                Name = parameter.Name,
                Id = parameter.ParameterId,
                IsAccumulator = parameter.IsAccumulator,
                IsConstant = parameter.IsConstant,
                IsModifiable = parameter.IsModifiable,
                Value = SetDictionaryFromValue(0)
            };

            var parameterValues =
                parameter.ParameterValues.FirstOrDefault(x => x.FiscalYear.FiscalYearID.ToString() == fiscalYear);
            if (modifiedGlobal == null && parameterValues == null)
                return global;
            global.Value =
                modifiedGlobal == null
                    ? AssignValue(parameterValues, null, null)
                    : SetDictionaryFromModifiedValues(modifiedGlobal);
            global.Modified = modifiedGlobal != null;
            return global;
        }

        private Global<decimal> updateGridMonthValues(Global<decimal> parameter, string fiscalYear)
        {
            var monthlyParams = parameter.MonthlyParameter;

            foreach (MonthlyParameter<Decimal> val in monthlyParams)
            {
                if (val.FiscalYearId.ToString().Trim().Equals(fiscalYear))
                {
                    parameter.Jan = val.Value.ElementAt(0).Value;
                    parameter.Feb = val.Value.ElementAt(1).Value;
                    parameter.Mar = val.Value.ElementAt(2).Value;
                    parameter.Apr = val.Value.ElementAt(3).Value;
                    parameter.May = val.Value.ElementAt(4).Value;
                    parameter.Jun = val.Value.ElementAt(5).Value;
                    parameter.Jul = val.Value.ElementAt(6).Value;
                    parameter.Aug = val.Value.ElementAt(7).Value;
                    parameter.Sep = val.Value.ElementAt(8).Value;
                    parameter.Oct = val.Value.ElementAt(9).Value;
                    parameter.Nov = val.Value.ElementAt(10).Value;
                    parameter.Dec = val.Value.ElementAt(11).Value;
                }
            }
            return parameter;
        }

        private IForecastPlan CreateForecastPlanFromDbWithoutPositions(DisneyHCMLatamPlanningEntities context, SBForecastPlan forecastPlan)
        {
            return this.CreateForecastPlan(context, forecastPlan);
        }


        public IGlobal<decimal> GetGlobalParameter(int id)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var parameter = context.Parameters.FirstOrDefault(x => x.ParameterId == id);

                return parameter.ToModel();
            }
        }

        public List<ParameterDto> GetParameters()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var globals = context.Parameters.ToList();

                return (from p in globals
                        select new ParameterDto
                                   {
                                       Id = p.ParameterId,
                                       Name = p.Name
                                   }).ToList();
            }
        }

        public List<IGlobal<decimal>> GetGlobalParameters()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var globals =
                    context.Parameters.ToList();

                var list = new List<IGlobal<decimal>>();

                globals.ForEach(x => list.Add(x.ToModel()));

                return list;
            }
        }

        public List<IExpenseType> GetExpenseTypes()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var expenseTypes =
                    context.ExpenseTypes
                            .Include("ExpenseGroup")
                            .ToList();

                var list = new List<IExpenseType>();

                expenseTypes.ForEach(x => list.Add(x.ToModel()));

                return list;
            }
        }

        public List<IExpenseGroup> GetExpenseGroups()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var expenseGroups =
                    context.ExpenseGroups.ToList();

                var list = new List<IExpenseGroup>();

                expenseGroups.ForEach(x => list.Add(x.ToModel()));

                return list;
            }
        }

        public List<IGLAccount> GetGLAccounts()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var glAccounts =
                    context.GLAccounts.ToList();

                var list = new List<IGLAccount>();

                glAccounts.ForEach(x => list.Add(x.ToModel()));

                return list;
            }
        }

        public bool SaveGlobalParameter(IGlobal<decimal> global)
        {
            try
            {
                using (var context = new DisneyHCMLatamPlanningEntities())
                {
                    if (global.Id > 0)
                        DeleteParameterValues(context, global.Id);
                    var model = global.ToDbModel(context);

                    if (model.ParameterId == 0)
                        context.Parameters.AddObject(model);

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }

        private void DeleteParameterValues(DisneyHCMLatamPlanningEntities context, int id)
        {
            var parameter =
                context.Parameters.Include("ParameterValues").Include("ParameterValues.ParameterMonthValues").
                    FirstOrDefault(x => x.ParameterId == id);
            if (parameter != null)
            {
                var parameterValues = parameter.ParameterValues.ToList();
                foreach (var parameterValue in parameterValues)
                {
                    var parameterMonthValues = parameterValue.ParameterMonthValues.ToList();
                    foreach (var parameterMonthValue in parameterMonthValues)
                    {
                        context.DeleteObject(parameterMonthValue);
                    }
                    context.DeleteObject(parameterValue);
                }
                context.SaveChanges();
            }
        }

        public bool DeleteParameter(int id)
        {
            bool result = false;

            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var param =
                    context
                    .Parameters
                    .Include("ParameterValues")
                    .Include("ParameterValues.ParameterMonthValues")
                    .FirstOrDefault(p => p.ParameterId == id);

                if (param != null)
                {
                    var values = param.ParameterValues.ToList();
                    foreach (var parameterValue in values)
                    {
                        var monthValues = parameterValue.ParameterMonthValues.ToList();
                        foreach (var parameterMonthValue in monthValues)
                        {
                            context.DeleteObject(parameterMonthValue);
                        }
                        context.DeleteObject(parameterValue);
                    }
                    context.DeleteObject(param);
                    context.SaveChanges();
                    result = true;
                }
            }

            return result;
        }


        public List<Scheme<decimal>> GetSchemes(int? schemeId, int? hcTypeCode, bool? status, String createdBy, DateTime? createdDate, String modifiedBy, DateTime? modifiedDate)
        {
            List<Scheme<decimal>> schemes = new List<Scheme<decimal>>();

            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var hcType = context.HCTypes.FirstOrDefault(x => x.HCTypeID == hcTypeCode);
                IQueryable<Scheme> schemeQuery = context.Schemes.Include("Company");

                if (schemeId != null)
                {
                    schemeQuery = schemeQuery.Where(s => s.SchemeId == schemeId);
                }

                if (hcType != null)
                {
                    schemeQuery = schemeQuery.Where(s => s.HCTypeId == hcType.HCTypeID);
                }

                if (status != null)
                {
                    schemeQuery = schemeQuery.Where(s => s.IsActive == status);
                }

                if (!String.IsNullOrEmpty(createdBy))
                {
                    schemeQuery = schemeQuery.Where(s => s.UserName == createdBy);
                }

                if (createdDate != null)
                {
                    schemeQuery = schemeQuery.Where(s => s.CreationDate == createdDate);
                }

                if (!String.IsNullOrEmpty(modifiedBy))
                {
                    schemeQuery = schemeQuery.Where(s => s.ModifiedUserName == modifiedBy);
                }

                if (modifiedDate != null)
                {
                    schemeQuery = schemeQuery.Where(s => s.ModificationDate == modifiedDate);
                }

                List<Scheme> dbSchemes = schemeQuery.ToList();
                schemes.AddRange(dbSchemes.Select(s => s.ToModel()));
            }
            return schemes;
        }

        public Concept<decimal> GetConceptById(int conceptId)
        {
            Concept<decimal> concept = null;

            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var conceptDB = context.Concepts
                   .Include("GLAccount")
                   .Include("Output1")
                   .Include("Output2")
                   .Include("Parameter1")
                   .Include("Parameter2")
                   .Include("Parameter3")
                   .Include("Parameter4")
                   .Include("ConceptsValidMonths")
                   .Include("ConceptFilters")
                   .FirstOrDefault(x => x.ConceptId == conceptId);
                if (conceptDB != null)
                {
                    concept = conceptDB.ToModel();
                }

                return concept;
            }
        }

        public List<Concept<decimal>> GetConceptsBySchemeId(int schemeId)
        {
            List<Concept<decimal>> concepts = new List<Concept<decimal>>();
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var conceptQuery = context.Concepts.Include("GLAccount").Where(c => c.SchemaId == schemeId);
                List<Concept> dbConcepts = conceptQuery.ToList();
                if (concepts != null)
                {
                    foreach (Concept c in dbConcepts)
                    {
                        Concept<decimal> concept = c.ToModel();
                        concepts.Add(concept);
                    }
                }
            }
            return concepts;
        }

        public void UpdateScheme(Scheme newScheme)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var scheme = context.Schemes.Where(s => s.SchemeId == newScheme.SchemeId).SingleOrDefault();
                if (scheme != null)
                {
                    scheme.CompanyId = newScheme.CompanyId;
                    scheme.HCTypeId = newScheme.HCTypeId;
                    scheme.ModifiedUserName = newScheme.ModifiedUserName;
                    scheme.ModificationDate = DateTime.Now;
                    scheme.IsActive = newScheme.IsActive;
                }
                context.SaveChanges();
            }
        }

        public void AddScheme(Scheme newScheme)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                context.Schemes.AddObject(newScheme);
                context.SaveChanges();
            }
        }


        public void DeleteScheme(int key)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var scheme = context.Schemes.Where(s => s.SchemeId == key).SingleOrDefault();
                context.Schemes.DeleteObject(scheme);
                context.SaveChanges();
            }
        }


        public int CopyScheme(int key, Scheme scheme)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var concepts = context.Concepts
                    .Include("GLAccount")
                    .Include("ConceptsValidMonths").Where(c => c.SchemaId == key).ToList();

                foreach (var concept in concepts)
                {
                    var newConcept = concept;
                    Concept concept1 = concept;
                    var newConceptValidMonths = context.ConceptsValidMonths.Where(x => x.ConceptId == concept1.ConceptId);
                    context.Detach(newConcept);
                    newConcept.Scheme = null;
                    foreach (var newConceptValidMonth in newConceptValidMonths)
                    {
                        context.Detach(newConceptValidMonth);
                        newConceptValidMonth.Concept = null;
                        newConcept.ConceptsValidMonths.Add(newConceptValidMonth);
                    }
                    scheme.Concepts.Add(newConcept);
                }
                context.Schemes.AddObject(scheme);
                context.SaveChanges();
                return scheme.SchemeId;
            }
        }

        public void UpdateConceptFilterParametersWithCurrentFunctionValues(int planId, int fiscalYearId, int positionLogId, int? employeeLogId, IConcept concept)
        {
            var filters = concept.GetFilters();
            foreach (Core.Filter<Decimal, bool> filter in filters)
            {
                foreach (var parameter in filter.GetParameters())
                {
                    var function = parameter as Function<decimal>;
                    if (function == null) continue;
                    using (var context = new DisneyHCMLatamPlanningEntities())
                    {
                        var parameterFromDB = context.Parameters.FirstOrDefault(x => x.ParameterId == function.Id);
                        if (parameterFromDB.ParameterDataType != null)
                        {
                            function.Value = GetMonthlyDataValue(context, planId, fiscalYearId, positionLogId, parameterFromDB);
                        }
                        else
                        {
                            /*function.Value = GetMonthlyFunctionValue(context, positionLogId, employeeLogId, function.TableName,
                                                                     function.ColumnName, filter.FilterType);*/
                            function.Value = GetMonthlyFunctionValue(context, positionLogId, employeeLogId, function.TableName,
                                                                    function.ColumnName);
                        }
                    }
                }

            }
        }

        public void AddAdjustmentsToPlan(int id, List<SBAdjustmentDto> adjustments)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == id);
                if (plan != null)
                {
                    foreach (var sbAdjustmentDto in adjustments)
                    {
                        SBAdjustmentDto dto = sbAdjustmentDto;
                        var company = context.Companies.FirstOrDefault(x => x.CompanyID == dto.CCode) ??
                                      context.Companies.First();
                        var costCenter =
                            context.CostCenters.FirstOrDefault(x => x.CostCenterID == dto.CCenter) ??
                            context.CostCenters.First();

                        var expType = context.ExpenseTypes.FirstOrDefault(x => x.ExpenseTypeId == dto.ExpType) ??
                                      context.ExpenseTypes.First();
                        var glAccount = context.GLAccounts.FirstOrDefault(x => x.GLAccountId == dto.GLAccount) ??
                                        context.GLAccounts.First();
                        var hcType = context.HCTypes.FirstOrDefault(x => x.HCTypeID == dto.HCType) ??
                                     context.HCTypes.First();
                        var lob = context.Lobs.FirstOrDefault(x => x.LobID == dto.Lob) ??
                                  context.Lobs.First();
                        var oct = new SBForecastPlanAdjustment
                                             {
                                                 SBForecastPlan = plan,
                                                 CalendarMonthId = (int)Month.October,
                                                 Company = company,
                                                 CostCenter = costCenter,
                                                 ExpenseType = expType,
                                                 GLAccount = glAccount,
                                                 HCType = hcType,
                                                 Lob = lob,
                                                 Title = dto.Title,
                                                 Value = dto.Oct
                                             };
                        context.SBForecastPlanAdjustments.AddObject(oct);
                        var nov = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.November,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Nov
                                      };
                        context.SBForecastPlanAdjustments.AddObject(nov);
                        var dec = new SBForecastPlanAdjustment
                                             {
                                                 SBForecastPlan = plan,
                                                 CalendarMonthId = (int)Month.December,
                                                 Company = company,
                                                 CostCenter = costCenter,
                                                 ExpenseType = expType,
                                                 GLAccount = glAccount,
                                                 HCType = hcType,
                                                 Lob = lob,
                                                 Title = dto.Title,
                                                 Value = dto.Dec
                                             };
                        context.SBForecastPlanAdjustments.AddObject(dec);
                        var jan = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.January,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Jan
                                      };
                        context.SBForecastPlanAdjustments.AddObject(jan);
                        var feb = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.February,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Feb
                                      };
                        context.SBForecastPlanAdjustments.AddObject(feb);
                        var mar = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.March,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Mar
                                      };
                        context.SBForecastPlanAdjustments.AddObject(mar);
                        var apr = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.April,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Apr
                                      };
                        context.SBForecastPlanAdjustments.AddObject(apr);
                        var may = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.May,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.May
                                      };
                        context.SBForecastPlanAdjustments.AddObject(may);
                        var jun = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.June,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Jun
                                      };
                        context.SBForecastPlanAdjustments.AddObject(jun);
                        var jul = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.July,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Jul
                                      };
                        context.SBForecastPlanAdjustments.AddObject(jul);
                        var aug = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.August,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Aug
                                      };
                        context.SBForecastPlanAdjustments.AddObject(aug);
                        var sep = new SBForecastPlanAdjustment
                                      {
                                          SBForecastPlan = plan,
                                          CalendarMonthId = (int)Month.September,
                                          Company = company,
                                          CostCenter = costCenter,
                                          ExpenseType = expType,
                                          GLAccount = glAccount,
                                          HCType = hcType,
                                          Lob = lob,
                                          Title = dto.Title,
                                          Value = dto.Sep
                                      };
                        context.SBForecastPlanAdjustments.AddObject(sep);

                        context.SaveChanges();


                    }

                }
            }
        }

        public List<SBAdjustmentDto> GetAdjustmentsByPlanId(int? planId)
        {
            List<SBAdjustmentDto> result = new List<SBAdjustmentDto>();
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context.SBForecastPlans.FirstOrDefault(x => x.SBForecastPlanId == planId);

                if (plan != null)
                {
                    var adjustments = context.SBForecastPlanAdjustments.Where(x => x.SBForecastPlanId == planId).ToList();
                    if (adjustments.Count > 0)
                    {
                        int index = 0;
                        foreach (var title in adjustments.Select(x => x.Title).Distinct().ToList())
                        {
                            string title1 = title;

                            var adjustmentDto = new SBAdjustmentDto { TempId = index };
                            index++;

                            var adjustment = adjustments.FirstOrDefault(x => x.Title == title1);
                            adjustmentDto.Title = adjustment.Title;
                            adjustmentDto.CCenter = adjustment.CostCenterId.HasValue ? adjustment.CostCenterId.Value : 0;
                            adjustmentDto.CCenterName = adjustment.CostCenter != null
                                                            ? adjustment.CostCenter.Description
                                                            : string.Empty;
                            adjustmentDto.CCode = adjustment.CompanyId.HasValue ? adjustment.CompanyId.Value : 0;
                            adjustmentDto.CompanyName = adjustment.Company != null
                                                            ? adjustment.Company.Code + " " + adjustment.Company.Description
                                                            : string.Empty;
                            adjustmentDto.ExpType = adjustment.ExpenseTypeId;
                            adjustmentDto.ExpTypeName = adjustment.ExpenseType != null
                                                            ? adjustment.ExpenseType.Description
                                                            : string.Empty;
                            adjustmentDto.GLAccount = adjustment.GLAccountID;
                            //adjustmentDto.GLAccountName = adjustment.GLAccount != null ? adjustment.GLAccount.
                            adjustmentDto.HCType = adjustment.HCTypeId;
                            adjustmentDto.HCTypeName = adjustment.HCType != null ? adjustment.HCType.Description : string.Empty;
                            adjustmentDto.Lob = adjustment.LobId;
                            adjustmentDto.LobName = adjustment.Lob != null ? adjustment.Lob.Description : string.Empty;

                            var value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.January);
                            adjustmentDto.Jan = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.February);
                            adjustmentDto.Feb = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.March);
                            adjustmentDto.Mar = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.April);
                            adjustmentDto.Apr = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.May);
                            adjustmentDto.May = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.June);
                            adjustmentDto.Jun = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.July);
                            adjustmentDto.Jul = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.August);
                            adjustmentDto.Aug = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.September);
                            adjustmentDto.Sep = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.October);
                            adjustmentDto.Oct = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.November);
                            adjustmentDto.Nov = value != null ? value.Value : 0;
                            value = adjustments.FirstOrDefault(x => x.Title == title1 && x.CalendarMonthId == (int)Month.December);
                            adjustmentDto.Dec = value != null ? value.Value : 0;
                            result.Add(adjustmentDto);
                        }

                    }
                }
                return result;
            }
        }

        public void DeleteRelatedInfo(int planId)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var plan = context
                    .SBForecastPlans
                    .Include("SBForecastPlanModifiedGlobals")
                    .Include("SBForecastPlanModifiedGlobals.ModifiedGlobalsMonthValues")
                    .FirstOrDefault(x => x.SBForecastPlanId == planId);
                if (plan != null)
                {
                    var adjustments = plan.SBForecastPlanAdjustments.ToList();
                    foreach (var sbForecastPlanAdjustment in adjustments)
                    {
                        context.SBForecastPlanAdjustments.DeleteObject(sbForecastPlanAdjustment);
                    }
                    var globalsModified = plan.SBForecastPlanModifiedGlobals.ToList();
                    foreach (var sbForecastPlanModifiedGlobal in globalsModified)
                    {
                        var values = sbForecastPlanModifiedGlobal.ModifiedGlobalsMonthValues.ToList();
                        foreach (var modifiedGlobalsMonthValue in values)
                        {
                            context.ModifiedGlobalsMonthValues.DeleteObject(modifiedGlobalsMonthValue);
                        }

                        context.SBForecastPlanModifiedGlobals.DeleteObject(sbForecastPlanModifiedGlobal);

                    }
                    foreach (var sbForecastPlanPosition in plan.SBForecastPlanPositions.ToList())
                    {
                        foreach (var sbForecastPlanData in sbForecastPlanPosition.SBForecastPlanDatas.ToList())
                        {
                            context.SBForecastPlanDatas.DeleteObject(sbForecastPlanData);
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        public void DeleteConcept(int? schemeId, int conceptId)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var scheme = context.Schemes.FirstOrDefault(x => x.SchemeId == schemeId);
                var concept = context.Concepts.FirstOrDefault(x => x.ConceptId == conceptId);
                if (scheme != null && concept != null)
                {
                    var sequence = concept.Sequence;
                    scheme.Concepts.Remove(concept);
                    context.Concepts.DeleteObject(concept);
                    foreach (var concept1 in scheme.Concepts.ToList())
                    {
                        if (concept1.Sequence > sequence)
                            concept1.Sequence--;
                    }
                }
                context.SaveChanges();

            }
        }

        public void CopyConcept(int? schemeId, int conceptId)
        {
            if (schemeId.HasValue)
            {
                using (var context = new DisneyHCMLatamPlanningEntities())
                {
                    var scheme = context.Schemes
                        .Include("Concepts")
                        .FirstOrDefault(x => x.SchemeId == schemeId);
                    var concept =
                        context.Concepts.Include("ConceptsValidMonths").FirstOrDefault(x => x.ConceptId == conceptId);
                    if (scheme != null && concept != null)
                    {
                        var newConceptValidMonths = concept.ConceptsValidMonths.ToList();
                        var newConceptFilters = concept.ConceptFilters.ToList();
                        var newConcept = concept;
                        newConcept.Sequence = scheme.Concepts.Count + 1;
                        newConcept.Title = "Copy of " + concept.Title;
                        newConcept.Scheme = null;
                        context.Detach(newConcept);
                        foreach (var conceptFilter in newConceptFilters)
                        {
                            var newConceptFilter = conceptFilter;
                            newConceptFilter.Concept = null;
                            var newFilter = conceptFilter.Filter;
                            context.Detach(newConceptFilter);
                            context.Detach(newFilter);
                            newConceptFilter.Filter = newFilter;
                            newConcept.ConceptFilters.Add(newConceptFilter);
                        }
                        foreach (var conceptValidMonth in newConceptValidMonths)
                        {
                            var newConceptValidMonth = conceptValidMonth;
                            newConceptValidMonth.Concept = null;
                            context.Detach(newConceptValidMonth);
                            newConcept.ConceptsValidMonths.Add(newConceptValidMonth);
                        }

                        scheme.Concepts.Add(newConcept);
                        context.SaveChanges();
                    }
                }
            }
        }

        public List<ParameterDataType> GetParameterDataTypes()
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                return context.ParameterDataTypes.Where(x => x.IsActive).ToList();
            }
        }
    }
}

