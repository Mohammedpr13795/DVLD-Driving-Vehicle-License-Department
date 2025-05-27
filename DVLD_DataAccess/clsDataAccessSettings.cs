using System;
using System.Configuration;
using System.Diagnostics;
namespace DVLD_DataAccess
{
    static class clsDataAccessSettings
    {
        //public static string ConnectionString = "Server=.;Database=DVLD; User ID=sa;Password=sa123456";
        //public static string ConnectionString = ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    try
                    {
                        var connectionStringSetting = ConfigurationManager.ConnectionStrings["MyDbConnection"];
                        if (connectionStringSetting == null)
                        {
                            throw new ConfigurationErrorsException("Connection string 'MyDbConnection' not found in configuration.");
                        }
                        _connectionString = connectionStringSetting.ConnectionString;
                    }
                    catch (Exception ex)
                    {
                        // معالجة الخطأ: يمكن تسجيله أو رمي استثناء للطبقة العليا
                        string errorMessage = $"Failed to load connection string: {ex.Message}";
                        // تسجيل الخطأ في Event Log (إذا كان متاحًا)
                        clsEventLogger.LogEvent(errorMessage, EventLogEntryType.Error);
                        // رمي استثناء للطبقة العليا للتعامل معه
                        throw new InvalidOperationException(errorMessage, ex);
                    }
                }
                return _connectionString;
            }
        }
    }
}
