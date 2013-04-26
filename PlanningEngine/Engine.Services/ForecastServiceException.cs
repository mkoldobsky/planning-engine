namespace Engine.Services
{
    using System;

    public class ForecastServiceException : Exception
    {
        public ForecastServiceException()
        {
            
        }

        public ForecastServiceException(string message) : base(message)
        {
        }
    }
}
