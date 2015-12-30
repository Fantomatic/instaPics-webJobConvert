using instaPicsWebJob.Model;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace instaPicsWebJob
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new JobHostConfiguration();
            configuration.DashboardConnectionString = ConfigurationManager.ConnectionStrings[Constants.DashboardConnectionKey].ConnectionString;
            configuration.StorageConnectionString = ConfigurationManager.ConnectionStrings[Constants.StorageConnectionKey].ConnectionString;

            var jobHost = new JobHost(configuration);
            jobHost.RunAndBlock();
        }
    }
}
