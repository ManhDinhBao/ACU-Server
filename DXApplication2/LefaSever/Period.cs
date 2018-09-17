using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Period
    {
        public Period() { }

        public Period(string dayInWeek, DateTime startTime, DateTime endTime)
        {
            DayInWeek = dayInWeek;
            StartTime = startTime;
            EndTime = endTime;
        }

        private DateTime startTime;

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        private DateTime endTime;

        public DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        private string dayInWeek;

        public string DayInWeek
        {
            get { return dayInWeek; }
            set { dayInWeek = value; }
        }

        public static List<Period> GetPeriodsBySchedule(string ScheduleId)
        {
            List<Period> periods = new List<Period>();
            DataTable dt = null;            
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.PeriodQuery("Q",ScheduleId);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["DayInWeek"].ToString();
                    DateTime startTime =Convert.ToDateTime(dr["StartTime"].ToString());
                    DateTime endTime = Convert.ToDateTime(dr["EndTime"].ToString());

                    Period period = new Period(Id, startTime, endTime);
                    periods.Add(period);
                }
                return periods;

            }
            catch (Exception ex)
            {
                return periods;

            }
        }
    }
}
