namespace Engine.DataAccess
{
    using System;
    using Engine.Core.Interfaces;

    public class TableLog : ILog
    {
        public void Log(string message)
        {
            
        }

        public void LogInfo(int planId, string message)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var log = new SBForecastPlanLog
                              {Date = DateTime.Now, Message = message, SBForecastPlanId = planId, TypeId = 1};
                context.SBForecastPlanLogs.AddObject(log);
                context.SaveChanges();

            }
        }

        public void LogError(int planId, string message)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var log = new SBForecastPlanLog { Date = DateTime.Now, Message = message, SBForecastPlanId = planId, TypeId = 3 };
                context.SBForecastPlanLogs.AddObject(log);
                context.SaveChanges();

            }
        }

        public void LogWarning(int planId, string message)
        {
            using (var context = new DisneyHCMLatamPlanningEntities())
            {
                var log = new SBForecastPlanLog { Date = DateTime.Now, Message = message, SBForecastPlanId = planId, TypeId = 2 };
                context.SBForecastPlanLogs.AddObject(log);
                context.SaveChanges();

            }
        }
    }
}
