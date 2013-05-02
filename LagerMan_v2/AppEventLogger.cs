using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security;

namespace LagerMan_v2
{
    class AppEventLogger
    {

        public void writeWarning(string trace)
        {

            if (!EventLog.SourceExists(Properties.Settings.Default.logSource))
                EventLog.CreateEventSource(Properties.Settings.Default.logSource, Properties.Settings.Default.eventlog);

            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace,
                EventLogEntryType.Warning, 200);
        }

        public void writeWarning(string trace, string stacktrace)
        {

            if (!EventLog.SourceExists(Properties.Settings.Default.logSource))
                EventLog.CreateEventSource(Properties.Settings.Default.logSource, Properties.Settings.Default.eventlog);

            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace);
            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace,
                EventLogEntryType.Warning, 200);
            EventLog.WriteEntry(Properties.Settings.Default.logSource, stacktrace);
            EventLog.WriteEntry(Properties.Settings.Default.logSource, stacktrace,
                EventLogEntryType.Warning, 210);
        }

        public void writeError(string trace)
        {

            if (!EventLog.SourceExists(Properties.Settings.Default.logSource))
                EventLog.CreateEventSource(Properties.Settings.Default.logSource, Properties.Settings.Default.eventlog);

            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace);
            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace,
                EventLogEntryType.Error, 100);
        }

        public void writeError(string trace, string stacktrace)
        {

            if (!EventLog.SourceExists(Properties.Settings.Default.logSource))
                EventLog.CreateEventSource(Properties.Settings.Default.logSource, Properties.Settings.Default.eventlog);

            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace,
                EventLogEntryType.Error, 100);
            EventLog.WriteEntry(Properties.Settings.Default.logSource, stacktrace,
                EventLogEntryType.Error, 110);
        }

        public void writeInfo(string trace)
        {

            if (!EventLog.SourceExists(Properties.Settings.Default.logSource))
                EventLog.CreateEventSource(Properties.Settings.Default.logSource, Properties.Settings.Default.eventlog);

            EventLog.WriteEntry(Properties.Settings.Default.logSource, trace,
                EventLogEntryType.Information, 10);
        }
    }
}
