using System;
using System.Collections.Generic;
using System.Text;

namespace SQLiteTest.Data
{
    /// <summary>
    /// Determines which data context type to create
    /// </summary>
    public enum DataContextType
    {
        SQLite = 0,
        InMemory = 1
    }
}
