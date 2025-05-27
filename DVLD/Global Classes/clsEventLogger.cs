using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DVLD.Global_Classes
{
    public static class clsEventLogger
    {
        private static DateTime lastLoggedTime = DateTime.MinValue;

        /// <summary>
        /// Logs an event to the Windows Event Log with the specified message and type.
        /// </summary>
        /// <param name="eventMessage">The message to log in the Event Log.</param>
        /// <param name="eventLogEntryType">The type of the event (e.g., Information, Warning, Error).</param>
        /// <param name="sourceName">The source name for the event log (default is "DVLD_App").</param>
        /// <param name="exception">An optional exception to include in the message.</param>
        /// <returns>True if the event was logged successfully; otherwise, false.</returns>
        public static async Task<bool> LogEvent(string eventMessage, EventLogEntryType eventLogEntryType, string sourceName = "DVLD_App", Exception exception = null)
        {
            // التحقق من صحة المدخلات
            if (string.IsNullOrWhiteSpace(eventMessage))
            {
                MessageBox.Show("Event message cannot be empty.", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                MessageBox.Show("Source name cannot be empty.", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            try
            {
                // إنشاء مصدر الحدث إذا لم يكن موجودًا
                if (!EventLog.SourceExists(sourceName))
                {
                    EventLog.CreateEventSource(sourceName, "Application");
                }

                // إعداد الرسالة مع إضافة تفاصيل الاستثناء إذا كان موجودًا
                string fullMessage = exception != null
                    ? $"{eventMessage} | Exception: {exception.Message} | StackTrace: {exception.StackTrace}"
                    : eventMessage;

                // تقييد طول الرسالة إذا كانت طويلة جدًا (اختياري)
                if (fullMessage.Length > 32766) // الحد الأقصى المسموح للرسالة في Event Log
                {
                    fullMessage = fullMessage.Substring(0, 32760) + "...";
                }

                // كتابة الحدث في السجل
                EventLog.WriteEntry(sourceName, fullMessage, eventLogEntryType);

                // تحديث وقت آخر تسجيل
                lastLoggedTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to log event: {ex.Message}", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await clsEventLogger.LogEvent(ex.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }
    }
}
