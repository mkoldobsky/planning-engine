using Engine.Core.Interfaces;

namespace Engine.Core
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IConcept
    {
        int Id { get; set; }
        void Run();
        bool HasSequence();
        int Sequence { get; set; }
        List<int> GetFunctionsIds();
        List<int> GetModifiableGlobalsIds();
        IList GetParameters();
        void SetParameters(IList parameters);

        string GetName();
        bool HasGLAccount();
        IList GetOutputParameters();
        bool ShouldSave();
        IList GetFilterParameters();
        void SetFilterParameters(IList filterParameters);
        void SetOutputParameters(IList outputParameters);
        IList GetFilters();
        void ClearOutputParameters();
        bool IsMonthFiltered(Month month);
        string GetOperationText();
    }
}