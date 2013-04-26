using FromDisney;

namespace Engine.DataAccess
{
    using System.Collections.Generic;
    using Engine.Core;
    using Engine.Core.Interfaces;
    using System;

    public interface IForecastDataAccessService
    {
        bool CreateForecastPlan(string name, string description, int fiscalYearId, string userName, int planFrom, int planTo);
        IForecastPlan GetPlan(int id);
        Scheme<decimal> GetSchemeById(int schemeId);
        Scheme<decimal> GetScheme(int planId, int fiscalYear, int? monthFrom, int? monthTo, int positionLogId, int? employeeLogId, string hCTypeCode, string companyCode);
        List<Scheme<decimal>> GetSchemes(int? schemeId, int? hcTypeCode, bool? status, String createdBy, DateTime? createdDate, String modifiedBy, DateTime? modifiedDate);
        bool DeletePlan(int id);
        bool CopyPlan(int id, string newName, string userName);
        Dictionary<Month, decimal> GetFunctionValuesByPlanAndPosition(int planId, int positionId, int functionId);
        void CreateResults(int planId, int positionLogId, List<IConcept> concepts);
        bool DeleteResults(int planId);
        bool PlanExist(string planName);

        IForecastPlan GetPlan(string name);
        List<IForecastPlan> GetPlans();
        int CountCurrentOpenPositions();
        int CountCurrentActivePositions();
        int CountCurrentReadyToHirePositions();
        int CountCurrentChangePendingPositions();
        void UpdateForecastPlan(IForecastPlan plan);
        List<IGlobal<decimal>> GetModifiableGlobals();
        List<IGlobal<decimal>> GetModifiableGlobals(int? planId, String fiscalYear);
        void InsertDataTable(int planId, string[] lines);
        void AddModifiedGlobalsToForecastPlan(int planId, List<IGlobal<decimal>> globals);
        string GetPlanName(int id);
        ILog GetLogger();
        void CleanAccumulableParameters(List<IMonthlyParameter<decimal>> accumulableParameters);
        void SaveInputParameters(int planId, IConcept concept);
        void UpdateConceptWithCurrentFunctionValues(int planId, int fiscalYearId, int positionLogId, int? employeeLogid, IConcept concept);

        List<ParameterDto> GetParameters();
        IGlobal<decimal> GetGlobalParameter(int id);
        List<IGlobal<decimal>> GetGlobalParameters();
        bool DeleteParameter(int id);
        bool SaveGlobalParameter(IGlobal<decimal> global);
        List<string> GetTableFields(string tableName);
        List<Concept<decimal>> GetConceptsBySchemeId(int schemeId);
        Concept<decimal> GetConceptById(int conceptId);
        bool SaveConcept(Concept<decimal> concept);


        void UpdateScheme(Scheme scheme);
        void AddScheme(Scheme scheme);
        void DeleteScheme(int key);

        int CopyScheme(int key, Scheme scheme);
        void UpdateConceptFilterParametersWithCurrentFunctionValues(int planId, int fiscalYearId, int positionlogId, int? employeeLogId, IConcept concept);
        void AddAdjustmentsToPlan(int id, List<SBAdjustmentDto> adjustments);
        List<SBAdjustmentDto> GetAdjustmentsByPlanId(int? planId);
        void DeleteRelatedInfo(int planId);
        void DeleteConcept(int? schemeId, int conceptId);
        void CopyConcept(int? schemeId, int conceptId);
        List<ParameterDataType> GetParameterDataTypes();
    }
}
