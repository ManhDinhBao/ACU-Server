using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Schedule
    {
        public Schedule() { }

        public Schedule(string scheduleId, string scheduleName, string description)
        {
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            Description = description;
        }

        public Schedule(string scheduleId, string scheduleName)
        {
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
        }

        public Schedule(string scheduleId)
        {
            ScheduleId = scheduleId;
        }

        public Schedule(string scheduleId, string scheduleName, string description, List<Period> listPeriod)
        {
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            Description = description;
            ListPeriod = listPeriod;
        }

        private string scheduleId;

        public string ScheduleId
        {
            get { return scheduleId; }
            set { scheduleId = value; }
        }

        private string scheduleName;

        public string ScheduleName
        {
            get { return scheduleName; }
            set { scheduleName = value; }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private List<Period> listPeriod;

        public List<Period> ListPeriod
        {
            get { return listPeriod; }
            set { listPeriod = value; }
        }

        public List<Schedule> GetSchedule()
        {
            List<Schedule> schedules = new List<Schedule>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.ScheduleQry("Q", scheduleId, scheduleName);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["scheduleId"].ToString();
                    string Name = dr["scheduleName"].ToString();
                    string description = dr["description"].ToString();

                    List<Period> periods = Period.GetPeriodsBySchedule(Id);
                    Schedule schedule = new Schedule(Id, Name, description, periods);
                    schedules.Add(schedule);
                }
                return schedules;
            }
            catch (Exception ex)
            {
                return schedules;
            }
        }

        public static List<Schedule> GetSchedulesByACSLevel(string accessLevelId)
        {
            List<Schedule> schedules = new List<Schedule>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.AccessLevelSQry("S", accessLevelId);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["scheduleId"].ToString();
                    string Name = dr["scheduleName"].ToString();
                    string Description = dr["description"].ToString();

                    List<Period> periods = Period.GetPeriodsBySchedule(Id);

                    Schedule schedule = new Schedule(Id, Name, Description, periods);
                    schedules.Add(schedule);

                }
                return schedules;
            }

            catch (Exception ex)
            {
                return schedules;
            }
        }
    }
}
