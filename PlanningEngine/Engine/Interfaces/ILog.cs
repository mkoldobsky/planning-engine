namespace Engine.Core.Interfaces
{
    public interface ILog
    {
        void Log(string message);
        void LogInfo(int planId, string message);
        void LogError(int planId, string message);
        void LogWarning(int planId, string message);

    }
}
