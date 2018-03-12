using System.Net;

namespace Automation.Api.Utilities
{
    public static class Headers
    {
        public static WebHeaderCollection LoginHeaderCollection { get; set; }
        public static WebHeaderCollection StandardHeaderCollection { get; set; }
    }
}