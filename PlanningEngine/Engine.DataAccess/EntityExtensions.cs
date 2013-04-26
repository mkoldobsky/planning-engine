using System;
using System.Collections.Generic;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Text;
using Engine.Core;
using Engine.Core.Interfaces;

namespace Engine.DataAccess
{
    public static class EntityExtensions
    {

        public static Engine.Core.GLAccount ToModel(this GLAccount glAccount)
        {
            return new Engine.Core.GLAccount
                       {
                           Id = glAccount.GLAccountId,
                           Title = glAccount.Title,
                           SAPCode = glAccount.SAPCode,
                           IsActive = glAccount.IsActive
                       };
        }

        public static Engine.Core.ExpenseGroup ToModel(this ExpenseGroup expenseGroup)
        {
            return new Engine.Core.ExpenseGroup()
            {
                Id = expenseGroup.ExpenseGroupId,
                Code = expenseGroup.Code,
                Description = expenseGroup.Description,
                IsActive = expenseGroup.IsActive
            };
        }

        public static Engine.Core.ExpenseType ToModel(this ExpenseType expenseType)
        {
            var context = new DisneyHCMLatamPlanningEntities();

            return new Engine.Core.ExpenseType()
            {
                Id = expenseType.ExpenseTypeId,
                Code = expenseType.Code,
                Description = expenseType.Description,
                IsActive = expenseType.IsActive,
                ExpenseGroup = (context.ExpenseGroups.FirstOrDefault(e => e.ExpenseGroupId == expenseType.ExpenseGroupId)).ToModel()
            };
        }

        public static Scheme<decimal> ToModel(this Scheme scheme)
        {
            return new Scheme<decimal>
                    {
                        Id = scheme.SchemeId,
                        HCTypeCode = scheme.HCType.Code,
                        CompanyCode = scheme.Company.Description,
                        CreatedBy = scheme.UserName,
                        CreationDate = scheme.CreationDate,
                        ModifiedUserName = scheme.ModifiedUserName,
                        ModificationDate = scheme.ModificationDate,
                        IsActive = scheme.IsActive
                    };
        }

        public static Concept ToDbModel(this Concept<decimal> concept, DisneyHCMLatamPlanningEntities context)
        {
            var conceptDb = context
                .Concepts
                .Include("Parameter1")
                .Include("Parameter2")
                .Include("Parameter3")
                .Include("Parameter4")
                .Include("Output1")
                .Include("Output2")
                .Include("ConceptFilters.Filter")
                .Include("ConceptFilters.Filter.Parameter")
                .Include("ConceptFilters.Filter.Parameter1")
                .FirstOrDefault(p => p.ConceptId == concept.Id);

            conceptDb = conceptDb ?? new Concept();

            conceptDb.Scheme = context.Schemes.FirstOrDefault(s => s.SchemeId == concept.Scheme.Id);

            conceptDb.Description = concept.Description;

            conceptDb.Title = concept.Title;

            conceptDb.Sequence = concept.Sequence;

            conceptDb.Operation = concept.GetOperationText();

            if (concept.GLAccount != null)
                conceptDb.GLAccount = context.GLAccounts.FirstOrDefault(gl => gl.GLAccountId == concept.GLAccount.Id);
            else
                conceptDb.GLAccount = null;

            #region Valid Months

            SyncValidMonths(conceptDb, concept, context);

            #endregion

            #region Filters

            conceptDb.ConceptFilters.ToList().ForEach(r => context.ConceptFilters.DeleteObject(r));

            foreach (var filter in concept.ConceptFilters)
            {
                Filter<decimal, bool> filter1 = filter;
                var conceptFilter = new ConceptFilter();

                conceptFilter.Filter = new Filter();

                if (filter.Connector != null)
                    conceptFilter.Connector = new Operation().GetOperator(filter.Connector.GetOperator().ToString());

                conceptFilter.Sequence = filter.Sequence;

                conceptFilter.Filter.Description = filter.Description;
                conceptFilter.Filter.Operation = new Operation().GetOperator(filter.Operation.GetOperator().ToString());
                conceptFilter.Filter.Title = filter.Name;

                conceptFilter.Filter.FilterType =
                    context.FilterTypes.FirstOrDefault(ft => ft.FilterTypeId == filter1.FilterType);

                conceptFilter.Filter.Parameter =
                    context.Parameters.FirstOrDefault(p => p.ParameterId == filter1.Parameter1.Id);

                if (filter.Parameter2.ParameterType == FromDisney.ParameterType.FixedValue)
                {
                    Parameter parameter = null;
                    if (filter.Parameter2.Id == 0)
                    {
                        parameter = context.Parameters.FirstOrDefault(x => x.FixedValue == filter1.Parameter2.FixedValue);
                        if (parameter != null)
                        {
                            conceptFilter.Filter.Parameter1 = parameter;
                        }
                        else
                        {
                            conceptFilter.Filter.Parameter1 = new Parameter
                                                                  {
                                                                      Name =
                                                                          "CONSTANT_" +
                                                                          filter1.Parameter2.FixedValue.ToString(),
                                                                      Description = "Created by System",
                                                                      IsAccumulator = false,
                                                                      Company = conceptFilter.Filter.Parameter.Company
                                                                  };
                        }

                    }
                    else
                    {
                        conceptFilter.Filter.Parameter1 = context.Parameters.FirstOrDefault(p => p.ParameterId == filter1.Parameter2.Id);
                    }
                    
                    conceptFilter.Filter.Parameter1.ParameterType =
                        context.ParameterTypes.FirstOrDefault(
                            pt => pt.ParameterTypeId == (int)filter1.Parameter2.ParameterType);

                    conceptFilter.Filter.Parameter1.FixedValue = filter1.Parameter2.FixedValue;
                }
                else
                {
                    conceptFilter.Filter.Parameter1 = context.Parameters.FirstOrDefault(p => p.ParameterId == filter1.Parameter2.Id);
                }

                conceptDb.ConceptFilters.Add(conceptFilter);

            }

            #endregion

            #region Input y Output Parameters

            conceptDb.Output1 = concept.Output1 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Output1.Id) : null;

            conceptDb.Output2 = concept.Output2 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Output2.Id) : null;

            conceptDb.Parameter1 = concept.Parameter1 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Parameter1.Id) : null;

            conceptDb.Parameter2 = concept.Parameter2 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Parameter2.Id) : null;

            conceptDb.Parameter3 = concept.Parameter3 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Parameter3.Id) : null;

            conceptDb.Parameter4 = concept.Parameter4 != null ? context.Parameters.FirstOrDefault(p => p.ParameterId == concept.Parameter4.Id) : null;

            #endregion

            if (concept.ExpenseType != null)
            {
                conceptDb.ExpenseType =
                    context.ExpenseTypes.FirstOrDefault(et => et.ExpenseTypeId == concept.ExpenseType.Id);

            }

            return conceptDb;
        }

        private static void SyncValidMonths(Concept conceptDb, Concept<decimal> concept, DisneyHCMLatamPlanningEntities context)
        {
            var months = Enum.GetValues(typeof(Month)).Cast<Month>().ToList();

            foreach (var month in months)
            {
                var conceptValidMonth = conceptDb.ConceptsValidMonths.FirstOrDefault(cvm => cvm.MonthId == (int)month);

                var value = concept.ValidMonths[month];

                if (value)
                {
                    if (conceptValidMonth == null)
                    {
                        conceptDb.ConceptsValidMonths.Add(new ConceptsValidMonth
                        {
                            MonthId = (int)month
                        });
                    }
                }
                else
                {
                    if (conceptValidMonth != null)
                    {
                        context.ConceptsValidMonths.DeleteObject(conceptValidMonth);
                    }
                }
            }
        }


        public static Concept<decimal> ToModel(this Concept conceptDb)
        {
            var concept = new Concept<decimal>(conceptDb.Operation)
                              {
                                  Id = conceptDb.ConceptId,
                                  Description = conceptDb.Description,
                                  GLAccountId = conceptDb.GLAccountID,
                                  Title = conceptDb.Title,
                                  Sequence = conceptDb.Sequence
                              };

            //var parameters = new List<IMonthlyParameter<decimal>>();

            if (conceptDb.Output1 != null)
            {
                concept.Output1 = new Global<decimal>()
                {
                    Id = conceptDb.Output1.ParameterId,
                    Name = conceptDb.Output1.Name,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Output1.ParameterTypeId
                };

                //parameters.Add(concept.Output1);
            }

            if (conceptDb.Output2 != null)
            {
                concept.Output2 = new Global<decimal>()
                {
                    Id = conceptDb.Output2.ParameterId,
                    Name = conceptDb.Output2.Name,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Output2.ParameterTypeId
                };
                //parameters.Add(concept.Output2);
            }

            if (conceptDb.Parameter1 != null)
            {
                concept.Parameter1 = new Global<decimal>()
                {
                    Id = conceptDb.Parameter1.ParameterId,
                    Name = conceptDb.Parameter1.Name,
                    Description = conceptDb.Parameter1.Description,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Parameter1.ParameterTypeId
                };
                //parameters.Add(concept.Parameter1);
            }

            if (conceptDb.Parameter2 != null)
            {
                concept.Parameter2 = new Global<decimal>()
                {
                    Id = conceptDb.Parameter2.ParameterId,
                    Name = conceptDb.Parameter2.Name,
                    Description = conceptDb.Parameter2.Description,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Parameter2.ParameterTypeId
                };
                //parameters.Add(concept.Parameter2);
            }

            if (conceptDb.Parameter3 != null)
            {
                concept.Parameter3 = new Global<decimal>()
                {
                    Id = conceptDb.Parameter3.ParameterId,
                    Name = conceptDb.Parameter3.Name,
                    Description = conceptDb.Parameter3.Description,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Parameter3.ParameterTypeId
                };
                //parameters.Add(concept.Parameter3);
            }

            if (conceptDb.Parameter4 != null)
            {
                concept.Parameter4 = new Global<decimal>()
                {
                    Id = conceptDb.Parameter4.ParameterId,
                    Name = conceptDb.Parameter4.Name,
                    Description = conceptDb.Parameter4.Description,
                    ParameterType = (FromDisney.ParameterType)conceptDb.Parameter4.ParameterTypeId
                };
                //parameters.Add(concept.Parameter4);
            }

            if (conceptDb.GLAccount != null)
            {
                concept.GLAccount = new Core.GLAccount
                {
                    Title = conceptDb.GLAccount.Title,
                    SAPCode = conceptDb.GLAccount.SAPCode,
                };
            }

            if (conceptDb.ExpenseType != null)
            {
                concept.ExpenseType = new Engine.Core.ExpenseType
                {
                    Id = conceptDb.ExpenseType.ExpenseTypeId,
                    Code = conceptDb.ExpenseType.Code,
                    Description = conceptDb.ExpenseType.Description,
                    IsActive = conceptDb.ExpenseType.IsActive
                };

                if (conceptDb.ExpenseType.ExpenseGroup != null)
                {
                    concept.ExpenseType.ExpenseGroup = new Engine.Core.ExpenseGroup
                    {
                        Id = conceptDb.ExpenseType.ExpenseGroup.ExpenseGroupId,
                        Code = conceptDb.ExpenseType.ExpenseGroup.Code,
                        Description = conceptDb.ExpenseType.ExpenseGroup.Description,
                        IsActive = conceptDb.ExpenseType.ExpenseGroup.IsActive
                    };
                }
            }

            var cvm = conceptDb.ConceptsValidMonths.OrderBy(m => m.MonthId).ToList();

            concept.ValidMonths = new Dictionary<Month, bool>
                                          {
                                              {Month.January, cvm.Any(m => m.MonthId == (int) Month.January)},
                                              {Month.February, cvm.Any(m => m.MonthId == (int) Month.February)},
                                              {Month.March, cvm.Any(m => m.MonthId == (int) Month.March)},
                                              {Month.April, cvm.Any(m => m.MonthId == (int) Month.April)},
                                              {Month.May, cvm.Any(m => m.MonthId == (int) Month.May)},
                                              {Month.June, cvm.Any(m => m.MonthId == (int) Month.June)},
                                              {Month.July, cvm.Any(m => m.MonthId == (int) Month.July)},
                                              {Month.August, cvm.Any(m => m.MonthId == (int) Month.August)},
                                              {Month.September, cvm.Any(m => m.MonthId == (int) Month.September)},
                                              {Month.October, cvm.Any(m => m.MonthId == (int) Month.October)},
                                              {Month.November, cvm.Any(m => m.MonthId == (int) Month.November)},
                                              {Month.December, cvm.Any(m => m.MonthId == (int) Month.December)}
                                          };


            if (conceptDb.ConceptFilters != null)
            {
                foreach (ConceptFilter cf in conceptDb.ConceptFilters)
                {
                    MonthlyParameter<decimal> parameter1 = null, parameter2 = null;

                    if (cf.Filter.Parameter != null)
                    {
                        parameter1 = new MonthlyParameter<decimal>();

                        parameter1.Id = cf.Filter.Parameter.ParameterId;
                        parameter1.Name = cf.Filter.Parameter.Name;
                        parameter1.Description = cf.Filter.Description;
                        parameter1.FixedValue = cf.Filter.Parameter.FixedValue;
                        parameter1.ParameterType = (FromDisney.ParameterType)cf.Filter.Parameter.ParameterTypeId;
                    }

                    if (cf.Filter.Parameter1 != null)
                        parameter2 = new MonthlyParameter<decimal>()
                        {
                            Id = cf.Filter.Parameter1.ParameterId,
                            Name = cf.Filter.Parameter1.Name,
                            FixedValue = cf.Filter.Parameter1.FixedValue,
                            ParameterType = (FromDisney.ParameterType)cf.Filter.Parameter1.ParameterTypeId
                        };

                    if (parameter1 != null && parameter2 != null)
                    {
                        IFIlter<bool> filter = new Filter<decimal, bool>
                        {
                            Operation = new Operation(cf.Filter.Operation),
                            Description = cf.Filter.Description,
                            Sequence = cf.Sequence,
                            Connector = new Operation(cf.Connector),
                            Name = cf.Filter.Title,
                            Id = cf.FilterId,
                            Parameter1 = parameter1,
                            Parameter2 = parameter2,
                            FilterType = cf.Filter.FilterType.FilterTypeId
                        };

                        concept.AddFilter(filter);
                    }

                }
            }

            return concept;
        }

        private static IParameter<decimal> ConvertMonthlyParameterToParameter(IMonthlyParameter<decimal> parameter)
        {
            return new Parameter<decimal> { Value = parameter.Value != null && parameter.Value.Count > 0 ? parameter.Value[Month.January] : 0 };
        }

        public static IGlobal<decimal> ToModel(this Parameter parameter)
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
                Value = ForecastDataAccessService.SetDictionaryFromValue(0),
                TableName = parameter.TableName,
                ColumnName = parameter.ColumnName,
                FixedValue = parameter.FixedValue,
            };

            if (parameter.ParameterDataTypeID.HasValue)
                global.ParameterDataTypeId = parameter.ParameterDataTypeID.Value;

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

                        var result = x.ParameterMonthValues.FirstOrDefault(y => y.Month == i);
                        //try
                        //{
                        //    value = x.ParameterMonthValues.ToList()[i - 1].Value;
                        //}
                        //catch (Exception)
                        //{
                        //    value = 0;
                        //}

                        monthlyParameter.Value.Add((Month)i, result == null ? 0: result.Value);
                    }

                    global.MonthlyParameter.Add(monthlyParameter);
                });
            }

            return global;
        }

        public static Parameter ToDbModel(this IGlobal<decimal> global, DisneyHCMLatamPlanningEntities context)
        {
            Parameter entity = context.Parameters.FirstOrDefault(p => p.ParameterId == global.Id);

            entity = entity ?? new Parameter();

            entity.ParameterId = global.Id;
            entity.Name = global.Name;
            entity.Description = global.Description;
            entity.ParameterTypeId = (int)global.ParameterType;
            entity.CompanyId = global.Company.Id;
            entity.IsAccumulator = global.IsAccumulator;
            entity.IsConstant = global.IsConstant;
            entity.IsModifiable = global.IsModifiable;
            entity.TableName = global.TableName;
            entity.ColumnName = global.ColumnName;
            entity.FixedValue = global.FixedValue;

            if (global.ParameterDataTypeId.HasValue)
                entity.ParameterDataTypeID = (int)global.ParameterDataTypeId.Value;

            if (global.ParameterType == FromDisney.ParameterType.Constant)
            {
                entity.ParameterValues.ToList().ForEach(r =>
                {
                    r.ParameterMonthValues.ToList().ForEach(pv => context.ParameterMonthValues.DeleteObject(pv));

                    context.ParameterValues.DeleteObject(r);
                });

                global.MonthlyParameter.ForEach(p =>
                {
                    #region MonthValues

                    var parameterMonthValues = new EntityCollection<ParameterMonthValue>();

                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.January, Value = p.Value[Month.January] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.February, Value = p.Value[Month.February] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.March, Value = p.Value[Month.March] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.April, Value = p.Value[Month.April] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.May, Value = p.Value[Month.May] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.June, Value = p.Value[Month.June] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.July, Value = p.Value[Month.July] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.August, Value = p.Value[Month.August] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.September, Value = p.Value[Month.September] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.October, Value = p.Value[Month.October] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.November, Value = p.Value[Month.November] });
                    parameterMonthValues.Add(new ParameterMonthValue { Month = (int)Month.December, Value = p.Value[Month.December] });

                    #endregion MonthValues

                    entity.ParameterValues.Add(new ParameterValue
                    {
                        FiscalYearID = p.FiscalYearId,
                        ParameterId = p.Id,
                        ParameterMonthValues = parameterMonthValues
                    });
                });
            }


            return entity;
        }

        public static Engine.Core.Company ToModel(this Company company)
        {
            Engine.Core.Company companyModel = null;

            if (company != null)
            {
                companyModel = new Engine.Core.Company
                {
                    Id = company.CompanyID,
                    Code = company.Code,
                    Description = company.Description,
                    IsActive = company.IsActive,
                    Territory = company.TerritoryID.ToString()
                };
            }

            return companyModel;
        }
    }
}
