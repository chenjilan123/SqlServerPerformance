using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerPerf
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlserver = new SqlServerFaster();
            sqlserver.Faster();
        }
    }
}
