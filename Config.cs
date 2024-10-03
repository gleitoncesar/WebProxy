using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebProxy
{
    public static class Config
    {
        public static string TargetSite
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["targetSite"];
            }
        }
    }
}