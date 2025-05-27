using System;
using System.Diagnostics;

public static class clsEventLogger
{
    private static DateTime lastLoggedTime = DateTime.MinValue;

    /// <summary>
    /// Logs an event to the Windows Event Log with the specified message and type.
    /// This method is suitable for use in the Data Access Layer.
    /// </summary>
    /// <param name="eventMessage">The message to log in the Event Log.</param>
    /// <param name="eventLogEntryType">The type of the event (e.g., Information, Warning, Error).</param>
    /// <param name="sourceName">The source name for the event log (default is "DVLD_App").</param>
    /// <param name="exception">An optional exception to include in the message.</param>
    /// <returns>True if the event was logged successfully; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when eventMessage or sourceName is null or empty.</exception>
    public static bool LogEvent(string eventMessage, EventLogEntryType eventLogEntryType, string sourceName = "DVLD_App", Exception exception = null)
    {
        // التحقق من صحة المدخلات
        if (string.IsNullOrWhiteSpace(eventMessage))
        {
            throw new ArgumentNullException(nameof(eventMessage), "Event message cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new ArgumentNullException(nameof(sourceName), "Source name cannot be null or empty.");
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

            // تقييد طول الرسالة إذا كانت طويلة جدًا
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
            EventLog.WriteEntry(sourceName, "Exception in LogException method: " + ex.Message, EventLogEntryType.Error);

            return false;
        }
    }
}