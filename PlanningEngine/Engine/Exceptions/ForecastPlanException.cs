namespace Engine.Core
{
    using System;

    public class ForecastPlanException : Exception
    {
        public ForecastPlanException()
        {
            
        }

        public ForecastPlanException(string message) : base(message)
        {
        }
    }
}
