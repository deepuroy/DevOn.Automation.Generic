using System;

namespace Automation.Api.Utilities
{
    public class WebServiceUtils
    {
        public string GenerateRandomNumber()
        {
            var random = new Random();
            return Convert.ToString(random.Next(500));
        }
    }
}