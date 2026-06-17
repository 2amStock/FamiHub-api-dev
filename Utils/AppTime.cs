using System;

namespace FamiHub.API.Utils
{
    public static class AppTime
    {
        // Set time to UTC+7 (Vietnam Time)
        public static DateTime Now => DateTime.UtcNow.AddHours(7);
    }
}
