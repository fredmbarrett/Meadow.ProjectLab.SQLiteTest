using Meadow;
using SQLiteTest.Controllers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Utils
{
    public class Log
    {
        const bool FORCE_COLLECTION = false;
        static DisplayController display = DisplayController.Instance;

        
        public static void Debug(string message)
        {
            var mem = $"{GC.GetTotalMemory(FORCE_COLLECTION):N0}";
            display.TotalMemUsage = mem;
            Resolver.Log.Debug($"{message} | MEM_USAGE = {mem}");
        }

        public static void Error(Exception ex, string message)
        {
            Resolver.Log.Error($"{message}: {ex.Message} | MEM_USAGE = {GC.GetTotalMemory(FORCE_COLLECTION):N0}");
            if (ex.InnerException != null)
            {
                Resolver.Log.Error($"...inner exception: {ex.InnerException.Message}");
            }
        }
    }
}
