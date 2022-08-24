//sample application for demonstrating Sql Server Profiling
//writen by Locky, 2009.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using Wickes.Logging;

namespace ExpressProfiler
{
    public class ExpressProfiler
    {
        private static readonly ILogger Logger = AppLogger.CreateLogger<ExpressProfiler>();

        internal const string versionString = "Express Profiler v2.2";

        public class PerfColumn
        {
            public string Caption;
            public int Column;
            public string Format;
        }

        private RawTraceReader m_Rdr;

        private readonly  YukonLexer m_Lex = new YukonLexer();
        private SqlConnection m_Conn;
        private readonly SqlCommand m_Cmd = new SqlCommand();
        private Thread m_Thr;
        private bool m_NeedStop = true;
        private int m_EventCount;
        private readonly ProfilerEvent m_EventStarted = new ProfilerEvent();
        private readonly ProfilerEvent m_EventStopped = new ProfilerEvent();
        private readonly ProfilerEvent m_EventPaused = new ProfilerEvent();
        private string m_servername = "";
        private string m_username = "";
        private string m_userpassword = "";
        internal int lastpos = -1;
        internal string lastpattern = "";
        Queue<ProfilerEvent> m_events = new Queue<ProfilerEvent>(10);
        internal TraceProperties.TraceSettings m_currentsettings;
        private readonly List<PerfColumn> m_columns = new List<PerfColumn>();
        internal bool matchCase = false;
        internal bool wholeWord = false;
        private Timer m_timer;

        public ExpressProfiler()
        {
            m_servername = Properties.Settings.Default.ServerName;
            m_username = Properties.Settings.Default.UserName;
            m_currentsettings = GetDefaultSettings();
            m_timer = new Timer(timer_Elapsed, null, 0, Timeout.Infinite);
            ParseCommandLine();
            InitLV();
        }

        private TraceProperties.TraceSettings GetDefaultSettings()
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(TraceProperties.TraceSettings));
                using (StringReader sr = new StringReader(Properties.Settings.Default.TraceSettings))
                {
                    return (TraceProperties.TraceSettings)x.Deserialize(sr);
                    
                }
            }
            catch (Exception)
            {
                
            }
            return TraceProperties.TraceSettings.GetDefaultSettings();
        }



//DatabaseName = Filters.DatabaseName,
//LoginName = Filters.LoginName,
//HostName = Filters.HostName,
//TextData = Filters.TextData,
//ApplicationName = Filters.ApplicationName,



        private bool ParseFilterParam(string[] args, int idx)
        {
            string condition = idx + 1 < args.Length ? args[idx + 1] : "";
            string value = idx + 2 < args.Length ? args[idx + 2] : "";

            switch (args[idx].ToLower())
            {
                case "-cpu":
                    m_currentsettings.Filters.CPU = Int32.Parse(value);
                    m_currentsettings.Filters.CpuFilterCondition = TraceProperties.ParseIntCondition(condition);
                    break;
                case "-duration":
                    m_currentsettings.Filters.Duration = Int32.Parse(value);
                    m_currentsettings.Filters.DurationFilterCondition = TraceProperties.ParseIntCondition(condition);
                    break;
                case "-reads":
                    m_currentsettings.Filters.Reads = Int32.Parse(value);
                    m_currentsettings.Filters.ReadsFilterCondition = TraceProperties.ParseIntCondition(condition);
                    break;
                case "-writes":
                    m_currentsettings.Filters.Writes = Int32.Parse(value);
                    m_currentsettings.Filters.WritesFilterCondition = TraceProperties.ParseIntCondition(condition);
                    break;
                case "-spid":
                    m_currentsettings.Filters.SPID = Int32.Parse(value);
                    m_currentsettings.Filters.SPIDFilterCondition = TraceProperties.ParseIntCondition(condition);
                    break;

                case "-databasename":
                    m_currentsettings.Filters.DatabaseName = value;
                    m_currentsettings.Filters.DatabaseNameFilterCondition = TraceProperties.ParseStringCondition(condition);
                    break;
                case "-loginname":
                    m_currentsettings.Filters.LoginName = value;
                    m_currentsettings.Filters.LoginNameFilterCondition = TraceProperties.ParseStringCondition(condition);
                    break;
                case "-hostname":
                    m_currentsettings.Filters.HostName = value;
                    m_currentsettings.Filters.HostNameFilterCondition = TraceProperties.ParseStringCondition(condition);
                    break;
                case "-textdata":
                    m_currentsettings.Filters.TextData = value;
                    m_currentsettings.Filters.TextDataFilterCondition = TraceProperties.ParseStringCondition(condition);
                    break;
                case "-applicationname":
                    m_currentsettings.Filters.ApplicationName = value;
                    m_currentsettings.Filters.ApplicationNameFilterCondition = TraceProperties.ParseStringCondition(condition);
                    break;

            }
            return false;
        }

        private void ParseCommandLine()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                int i = 1;
                while (i < args.Length)
                {
                    string ep = i + 1 < args.Length ? args[i + 1] : "";
                    switch (args[i].ToLower())
                    {
                        case "-s":
                        case "-server":
                            m_servername = ep;
                            i++;
                            break;
                        case "-u":
                        case "-user":
                            m_username = ep;
                            i++;
                            break;
                        case "-p":
                        case "-password":
                            m_userpassword = ep;
                            i++;
                            break;
                        case "-m":
                        case "-maxevents":
                            int m;
                            if (!Int32.TryParse(ep, out m)) m = 500;
                            m_currentsettings.Filters.MaximumEventCount = m;
                            break;
                        case "-d":
                        case "-duration":
                            int d;
                            if (Int32.TryParse(ep, out d))
                            {
                                m_currentsettings.Filters.DurationFilterCondition = TraceProperties.IntFilterCondition.GreaterThan;
                                m_currentsettings.Filters.Duration = d;
                            }

                            break;
                        case "-batchcompleted":
                            m_currentsettings.EventsColumns.BatchCompleted = true;
                            break;
                        case "-batchstarting":
                            m_currentsettings.EventsColumns.BatchStarting = true;
                            break;
                        case "-existingconnection":
                            m_currentsettings.EventsColumns.ExistingConnection = true;
                            break;
                        case "-loginlogout":
                            m_currentsettings.EventsColumns.LoginLogout = true;
                            break;
                        case "-rpccompleted":
                            m_currentsettings.EventsColumns.RPCCompleted = true;
                            break;
                        case "-rpcstarting":
                            m_currentsettings.EventsColumns.RPCStarting = true;
                            break;
                        case "-spstmtcompleted":
                            m_currentsettings.EventsColumns.SPStmtCompleted = true;
                            break;
                        case "-spstmtstarting":
                            m_currentsettings.EventsColumns.SPStmtStarting = true;
                            break;
                        default:
                            if (ParseFilterParam(args, i)) i++;
                            break;

                    }
                    i++;
                }

                if (m_servername.Length == 0)
                {
                    m_servername = @".";
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error");
            }
        }
    
        private void InitLV()
        {
            InitColumns();
        }

        private void InitColumns()
        {
            m_columns.Clear();
            m_columns.Add(new PerfColumn{ Caption = "Event Class", Column = ProfilerEventColumns.EventClass });
            m_columns.Add(new PerfColumn { Caption = "Text Data", Column = ProfilerEventColumns.TextData });
            m_columns.Add(new PerfColumn { Caption = "Login Name", Column = ProfilerEventColumns.LoginName });
            m_columns.Add(new PerfColumn { Caption = "CPU", Column = ProfilerEventColumns.CPU, Format = "#,0" });
            m_columns.Add(new PerfColumn { Caption = "Reads", Column = ProfilerEventColumns.Reads, Format = "#,0" });
            m_columns.Add(new PerfColumn { Caption = "Writes", Column = ProfilerEventColumns.Writes, Format = "#,0" });
            m_columns.Add(new PerfColumn { Caption = "Duration, ms", Column = ProfilerEventColumns.Duration, Format = "#,0" });
            m_columns.Add(new PerfColumn { Caption = "SPID", Column = ProfilerEventColumns.SPID });

            if (m_currentsettings.EventsColumns.StartTime) m_columns.Add(new PerfColumn { Caption = "Start time", Column = ProfilerEventColumns.StartTime, Format = "yyyy-MM-dd hh:mm:ss.ffff" });
            if (m_currentsettings.EventsColumns.EndTime) m_columns.Add(new PerfColumn { Caption = "End time", Column = ProfilerEventColumns.EndTime, Format = "yyyy-MM-dd hh:mm:ss.ffff" });
            if (m_currentsettings.EventsColumns.DatabaseName) m_columns.Add(new PerfColumn { Caption = "DatabaseName", Column = ProfilerEventColumns.DatabaseName });
            if (m_currentsettings.EventsColumns.ObjectName) m_columns.Add(new PerfColumn { Caption = "Object name", Column = ProfilerEventColumns.ObjectName });
            if (m_currentsettings.EventsColumns.ApplicationName) m_columns.Add(new PerfColumn { Caption = "Application name", Column = ProfilerEventColumns.ApplicationName });
            if (m_currentsettings.EventsColumns.HostName) m_columns.Add(new PerfColumn { Caption = "Host name", Column = ProfilerEventColumns.HostName });

            m_columns.Add(new PerfColumn { Caption = "#", Column = -1 });
        }

        private string GetEventCaption(ProfilerEvent evt)
        {
            return ProfilerEvents.Names[evt.EventClass];
        }

        private string GetFormattedValue(ProfilerEvent evt,int column,string format)
        {
            return ProfilerEventColumns.Duration == column ? (evt.Duration / 1000).ToString(format) : evt.GetFormattedData(column, format);
        }

        private void NewEventArrived(ProfilerEvent evt)
        {
            m_EventCount++;
            string caption = GetEventCaption(evt);
            object[] data = new object[m_columns.Count]; 
            for (int i = 0; i < m_columns.Count - 1; i++)
            {
                PerfColumn pc = m_columns[i];
                data[i] = pc.Column == -1 ? m_EventCount.ToString("#,0") : GetFormattedValue(evt, pc.Column, pc.Format) ?? "";
            }
            Logger.LogInformation(m_EventCount.ToString("#,0"), data);
        }

        private void ProfilerThread(Object state)
        {
            try
            {
                while (!m_NeedStop && m_Rdr.TraceIsActive)
                {
                    ProfilerEvent evt = m_Rdr.Next();
                    if (evt != null)
                    {
                        lock (this)
                        {
                            m_events.Enqueue(evt);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "profiler thread exception");
            }
        }

        private  SqlConnection GetConnection()
        {
            var security = (m_username != null && m_userpassword != null) ? string.Format("User Id={0}; Password='{1}'", m_username, m_userpassword) : "Integrated Security=SSPI";
            return new SqlConnection
            {
                ConnectionString = string.Format(@"Data Source={0}; Initial Catalog=master; Application Name=Express Profiler; {1}", m_servername, security)
            };
        }

        public void StartProfiling()
        {
            try
            {
                if (m_Conn != null && m_Conn.State == ConnectionState.Open)
                {
                    m_Conn.Close();
                }
                m_EventCount = 0;
                m_Conn = GetConnection();
                m_Conn.Open();
                m_Rdr = new RawTraceReader(m_Conn);

                m_Rdr.CreateTrace();
                if (true)
                {
                    if (m_currentsettings.EventsColumns.LoginLogout)
                    {
                        m_Rdr.SetEvent(ProfilerEvents.SecurityAudit.AuditLogin,
                                       ProfilerEventColumns.TextData,
                                       ProfilerEventColumns.LoginName,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.HostName
                            );
                        m_Rdr.SetEvent(ProfilerEvents.SecurityAudit.AuditLogout,
                                       ProfilerEventColumns.CPU,
                                       ProfilerEventColumns.Reads,
                                       ProfilerEventColumns.Writes,
                                       ProfilerEventColumns.Duration,
                                       ProfilerEventColumns.LoginName,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.ApplicationName,
                                       ProfilerEventColumns.HostName
                            );
                    }

                    if (m_currentsettings.EventsColumns.ExistingConnection)
                    {
                        m_Rdr.SetEvent(ProfilerEvents.Sessions.ExistingConnection,
                                       ProfilerEventColumns.TextData,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.ApplicationName,
                                       ProfilerEventColumns.HostName
                            );
                    }
                    if (m_currentsettings.EventsColumns.BatchCompleted)
                    {
                        m_Rdr.SetEvent(ProfilerEvents.TSQL.SQLBatchCompleted,
                                       ProfilerEventColumns.TextData,
                                       ProfilerEventColumns.LoginName,
                                       ProfilerEventColumns.CPU,
                                       ProfilerEventColumns.Reads,
                                       ProfilerEventColumns.Writes,
                                       ProfilerEventColumns.Duration,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.DatabaseName,
                                       ProfilerEventColumns.ApplicationName,
                                       ProfilerEventColumns.HostName
                            );
                    }
                    if (m_currentsettings.EventsColumns.BatchStarting)
                    {
                        m_Rdr.SetEvent(ProfilerEvents.TSQL.SQLBatchStarting,
                                       ProfilerEventColumns.TextData,
                                       ProfilerEventColumns.LoginName,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.DatabaseName,
                                       ProfilerEventColumns.ApplicationName,
                                       ProfilerEventColumns.HostName
                            );
                    }
                    if (m_currentsettings.EventsColumns.RPCStarting)
                    {
                        m_Rdr.SetEvent(ProfilerEvents.StoredProcedures.RPCStarting,
                                       ProfilerEventColumns.TextData,
                                       ProfilerEventColumns.LoginName,
                                       ProfilerEventColumns.SPID,
                                       ProfilerEventColumns.StartTime,
                                       ProfilerEventColumns.EndTime,
                                       ProfilerEventColumns.DatabaseName,
                                       ProfilerEventColumns.ObjectName,
                                       ProfilerEventColumns.ApplicationName,
                                       ProfilerEventColumns.HostName

                            );
                    }

                }
                if (m_currentsettings.EventsColumns.RPCCompleted)
                {
                    m_Rdr.SetEvent(ProfilerEvents.StoredProcedures.RPCCompleted,
                                   ProfilerEventColumns.TextData, ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU, ProfilerEventColumns.Reads,
                                   ProfilerEventColumns.Writes, ProfilerEventColumns.Duration,
                                   ProfilerEventColumns.SPID
                                   , ProfilerEventColumns.StartTime, ProfilerEventColumns.EndTime
                                   , ProfilerEventColumns.DatabaseName
                                   , ProfilerEventColumns.ObjectName
                                   , ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName

                        );
                }
                if (m_currentsettings.EventsColumns.SPStmtCompleted)
                {
                    m_Rdr.SetEvent(ProfilerEvents.StoredProcedures.SPStmtCompleted,
                                   ProfilerEventColumns.TextData, ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU, ProfilerEventColumns.Reads,
                                   ProfilerEventColumns.Writes, ProfilerEventColumns.Duration,
                                   ProfilerEventColumns.SPID
                                   , ProfilerEventColumns.StartTime, ProfilerEventColumns.EndTime
                                   , ProfilerEventColumns.DatabaseName
                                   , ProfilerEventColumns.ObjectName
                                   , ProfilerEventColumns.ObjectID
                                   , ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );
                }
                if (m_currentsettings.EventsColumns.SPStmtStarting)
                {
                    m_Rdr.SetEvent(ProfilerEvents.StoredProcedures.SPStmtStarting,
                                   ProfilerEventColumns.TextData, ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU, ProfilerEventColumns.Reads,
                                   ProfilerEventColumns.Writes, ProfilerEventColumns.Duration,
                                   ProfilerEventColumns.SPID
                                   , ProfilerEventColumns.StartTime, ProfilerEventColumns.EndTime
                                   , ProfilerEventColumns.DatabaseName
                                   , ProfilerEventColumns.ObjectName
                                   , ProfilerEventColumns.ObjectID
                                   , ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );
                }
                if (m_currentsettings.EventsColumns.UserErrorMessage)
                {
                    m_Rdr.SetEvent(ProfilerEvents.ErrorsAndWarnings.UserErrorMessage,
                                   ProfilerEventColumns.TextData,
                                   ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU,
                                   ProfilerEventColumns.SPID,
                                   ProfilerEventColumns.StartTime,
                                   ProfilerEventColumns.DatabaseName,
                                   ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );
                }
                if (m_currentsettings.EventsColumns.BlockedProcessPeport)
                {
                    m_Rdr.SetEvent(ProfilerEvents.ErrorsAndWarnings.Blockedprocessreport,
                                   ProfilerEventColumns.TextData,
                                   ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU,
                                   ProfilerEventColumns.SPID,
                                   ProfilerEventColumns.StartTime,
                                   ProfilerEventColumns.DatabaseName,
                                   ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );

                }

                if (m_currentsettings.EventsColumns.SQLStmtStarting)
                {
                    m_Rdr.SetEvent(ProfilerEvents.TSQL.SQLStmtStarting,
                                   ProfilerEventColumns.TextData, ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU, ProfilerEventColumns.Reads,
                                   ProfilerEventColumns.Writes, ProfilerEventColumns.Duration,
                                   ProfilerEventColumns.SPID
                                   , ProfilerEventColumns.StartTime, ProfilerEventColumns.EndTime
                                   , ProfilerEventColumns.DatabaseName
                                   , ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );
                }
                if (m_currentsettings.EventsColumns.SQLStmtCompleted)
                {
                    m_Rdr.SetEvent(ProfilerEvents.TSQL.SQLStmtCompleted,
                                   ProfilerEventColumns.TextData, ProfilerEventColumns.LoginName,
                                   ProfilerEventColumns.CPU, ProfilerEventColumns.Reads,
                                   ProfilerEventColumns.Writes, ProfilerEventColumns.Duration,
                                   ProfilerEventColumns.SPID
                                   , ProfilerEventColumns.StartTime, ProfilerEventColumns.EndTime
                                   , ProfilerEventColumns.DatabaseName
                                   , ProfilerEventColumns.ApplicationName
                                   , ProfilerEventColumns.HostName
                        );
                }

                if (null != m_currentsettings.Filters.Duration)
                {
                    SetIntFilter(m_currentsettings.Filters.Duration*1000,
                                 m_currentsettings.Filters.DurationFilterCondition, ProfilerEventColumns.Duration);
                }
                SetIntFilter(m_currentsettings.Filters.Reads, m_currentsettings.Filters.ReadsFilterCondition,ProfilerEventColumns.Reads);
                SetIntFilter(m_currentsettings.Filters.Writes, m_currentsettings.Filters.WritesFilterCondition,ProfilerEventColumns.Writes);
                SetIntFilter(m_currentsettings.Filters.CPU, m_currentsettings.Filters.CpuFilterCondition,ProfilerEventColumns.CPU);
                SetIntFilter(m_currentsettings.Filters.SPID, m_currentsettings.Filters.SPIDFilterCondition, ProfilerEventColumns.SPID);

                SetStringFilter(m_currentsettings.Filters.LoginName, m_currentsettings.Filters.LoginNameFilterCondition,ProfilerEventColumns.LoginName);
                SetStringFilter(m_currentsettings.Filters.HostName, m_currentsettings.Filters.HostNameFilterCondition, ProfilerEventColumns.HostName);
                SetStringFilter(m_currentsettings.Filters.DatabaseName,m_currentsettings.Filters.DatabaseNameFilterCondition, ProfilerEventColumns.DatabaseName);
                SetStringFilter(m_currentsettings.Filters.TextData, m_currentsettings.Filters.TextDataFilterCondition,ProfilerEventColumns.TextData);
                SetStringFilter(m_currentsettings.Filters.ApplicationName, m_currentsettings.Filters.ApplicationNameFilterCondition, ProfilerEventColumns.ApplicationName);


                m_Cmd.Connection = m_Conn;
                m_Cmd.CommandTimeout = 0;
                m_Rdr.SetFilter(ProfilerEventColumns.ApplicationName, LogicalOperators.AND, ComparisonOperators.NotLike,
                                "Express Profiler");
                m_events.Clear();
                SaveDefaultSettings();
                StartProfilerThread();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error");
            }
        }

	    private void SaveDefaultSettings()
	    {
		    Properties.Settings.Default.ServerName = m_servername;
            Properties.Settings.Default.UserName = (m_username != null && m_userpassword != null) ? m_username : "";
            Properties.Settings.Default.UserPassword = (m_username != null && m_userpassword != null) ? m_userpassword : "";
            Properties.Settings.Default.Save();
	    }

	    private void SetIntFilter(int? value, TraceProperties.IntFilterCondition condition, int column)
        {
            int[] com = new[] { ComparisonOperators.Equal, ComparisonOperators.NotEqual, ComparisonOperators.GreaterThan, ComparisonOperators.LessThan};
            if ((null != value))
            {
                long? v = value;
                m_Rdr.SetFilter(column, LogicalOperators.AND, com[(int)condition], v);
            }
        }

        private void SetStringFilter(string value,TraceProperties.StringFilterCondition condition,int column)
        {
            if (!String.IsNullOrEmpty(value))
            {
                m_Rdr.SetFilter(column, LogicalOperators.AND
                    , condition == TraceProperties.StringFilterCondition.Like ? ComparisonOperators.Like : ComparisonOperators.NotLike
                    , value
                    );
            }

        }

        private void StartProfilerThread()
        { 
            if(m_Rdr!=null)
            {
                m_Rdr.Close();
            }
            m_Rdr.StartTrace();
            m_Thr = new Thread(ProfilerThread) {IsBackground = true, Priority = ThreadPriority.Lowest};
            m_NeedStop = false;
            m_Thr.Start();
        }

        public void StopProfiling()
        {
            using (SqlConnection cn = GetConnection())
            {
                cn.Open();
                m_Rdr.StopTrace(cn);
                m_Rdr.CloseTrace(cn);
                cn.Close();
            }
            m_NeedStop = true;
            if (m_Thr.IsAlive)
            {
                m_Thr.Abort();
            }
            m_Thr.Join();
        }

        private void timer_Elapsed(object state)
        {
            Queue<ProfilerEvent> saved;
            lock (this)
            {
                saved = m_events;
                m_events = new Queue<ProfilerEvent>(10);
                while (0 != saved.Count)
                {
                    NewEventArrived(saved.Dequeue());
                }
            }
        }
    }
}
