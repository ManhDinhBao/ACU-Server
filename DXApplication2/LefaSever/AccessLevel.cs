using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class AccessLevel
    {
        public AccessLevel() { }
        public AccessLevel(string id, string name)
        {
            ID = id;
            Name = name;
        }
        public AccessLevel(string id, string name, List<Door> doors, List<Schedule> schedules)
        {
            ID = id;
            Name = name;
            Doors = doors;
            Schedules = schedules;
        }

        private string id;

        public string  ID
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private List<Door> doors;

        public List<Door> Doors
        {
            get { return doors; }
            set { doors = value; }
        }

        private List<Schedule> schedules;

        public List<Schedule> Schedules
        {
            get { return schedules; }
            set { schedules = value; }
        }

        public List<AccessLevel> GetAccessLevels()
        {
            List<AccessLevel> accessLevels = new List<AccessLevel>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.AccessLevelQry("Q", id, name);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["accessLevelID"].ToString();
                    string name = dr["accessLevelName"].ToString();

                    List<Door> doors = Door.GetDoorsByACSLevel(id);
                    List<Schedule> schedules = Schedule.GetSchedulesByACSLevel(id);

                    AccessLevel accessLevel = new AccessLevel(id, name, doors, schedules);
                    accessLevels.Add(accessLevel);
                }
                return accessLevels;
            }
            catch (Exception ex)
            {
                return accessLevels;
            }
        }
    }
}
