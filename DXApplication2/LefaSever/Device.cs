using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Device
    {
        public Device() { }

        public Device(string id, string name, string ip)
        {
            ID = id;
            Name = name;
            IP = ip;
        }

        private string id;

        public string ID
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

        private string ip;

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public List<Device> GetDevices()
        {
            List<Device> devices = new List<Device>();
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.DeviceQuery("Q", id, name, ip);
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["deviceId"].ToString();
                    string name = dr["deviceName"].ToString();
                    string ip = dr["deviceIP"].ToString();                   

                    Device device = new Device(id,name,ip);
                    devices.Add(device);
                }
                return devices;
            }
            catch (Exception ex)
            {
                return devices;
            }
        }

        public static Device GetDeviceByDoorId(string doorId)
        {
            Device device = null;
            DataTable dt = null;
            try
            {
                ServiceReference1.WSACUSoapClient client = new ServiceReference1.WSACUSoapClient();
                DataSet ds = client.DoorQuery("D", doorId, "", "");
                dt = ds.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    string id = dr["deviceId"].ToString();
                    string name = dr["deviceName"].ToString();
                    string ip = dr["deviceIP"].ToString();

                    device = new Device(id, name, ip);
                }
                return device;
            }
            catch (Exception ex)
            {
                return device;
            }
        }
    }
}
