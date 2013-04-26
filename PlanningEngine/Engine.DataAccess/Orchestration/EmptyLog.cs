namespace Engine.DataAccess
{
    using Engine.Core.Interfaces;

    public class EmptyLog : ILog
    {
        public void Log(string message)
        {
            
        }

        public void LogInfo(int planId, string message)
        {
            
        }

        public void LogError(int planId, string message)
        {
            
        }

        public void LogWarning(int planId, string message)
        {
            
        }
    }

}
