using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Door
    {

        public Door() { }
        public Door(string doorId, string doorName, int readerNumber, int inputNumber, int outputNumber, int relayNumber)
        {
            DoorID = doorId;
            DoorName = doorName;
            ReaderNumber = readerNumber;
            InputNumber = inputNumber;
            OutputNumber = outputNumber;
            RelayNumber = relayNumber;
        }

        public Door(string doorId, string doorName, int readerNumber, int inputNumber, int outputNumber, int relayNumber, Device device)
        {
            DoorID = doorId;
            DoorName = doorName;
            ReaderNumber = readerNumber;
            InputNumber = inputNumber;
            OutputNumber = outputNumber;
            RelayNumber = relayNumber;
            Device = device;
        }
        private string doorId;

        public string  DoorID
        {
            get { return doorId; }
            set { doorId = value; }
        }

        private string doorName;

        public string DoorName
        {
            get { return doorName; }
            set { doorName = value; }
        }

        private int readerNumber;

        public int ReaderNumber
        {
            get { return readerNumber; }
            set { readerNumber = value; }
        }

        private int inputNumber;

        public int InputNumber
        {
            get { return inputNumber; }
            set { inputNumber = value; }
        }

        private int outputNumber;

        public int OutputNumber
        {
            get { return outputNumber; }
            set { outputNumber = value; }
        }

        private int relayNumber;

        public int RelayNumber
        {
            get { return relayNumber; }
            set { relayNumber = value; }
        }

        private Device device;

        public Device Device
        {
            get { return device; }
            set { device = value; }
        }


        public static List<Door> GetDoorsByACSLevel(string accessLevelId)
        {
            List<Door> doors = new List<Door>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.AccessLevelDQry("D", accessLevelId);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["doorId"].ToString();
                    string Name = dr["doorName"].ToString();
                    string deviceId = dr["deviceId"].ToString();
                    int readerNo = Convert.ToInt32(dr["readerNumber"]);
                    int inputNo = Convert.ToInt32(dr["inputNumber"]);
                    int outputNo = Convert.ToInt32(dr["outputNumber"]);
                    int relayNo = Convert.ToInt32(dr["relayNumber"]);

                    Device device = Device.GetDeviceByDoorId(Id);
                    Door door = new Door(Id, Name, readerNo, inputNo, outputNo, readerNo, device);
                    doors.Add(door);

                }
                return doors;
            }

            catch (Exception ex)
            {
                return doors;
            }
        }

        public static List<Door> GetDoorsByDevice(string deviceId)
        {
            List<Door> doors = new List<Door>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.DeviceQuery("D", deviceId,"","");
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string Id = dr["doorId"].ToString();
                    string Name = dr["doorName"].ToString();                    
                    int readerNo = Convert.ToInt32(dr["readerNumber"]);
                    int inputNo = Convert.ToInt32(dr["inputNumber"]);
                    int outputNo = Convert.ToInt32(dr["outputNumber"]);
                    int relayNo = Convert.ToInt32(dr["relayNumber"]);

                    Device device = Device.GetDeviceByDoorId(Id);
                    Door door = new Door(Id, Name, readerNo, inputNo, outputNo, readerNo, device);
                    doors.Add(door);

                }
                return doors;
            }

            catch (Exception ex)
            {
                return doors;
            }
        }

    }
}
