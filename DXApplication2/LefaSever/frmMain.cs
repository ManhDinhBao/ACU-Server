using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.CheckedListBox;

namespace LefaSever
{
    public partial class frmMain : DevExpress.XtraEditors.XtraForm
    {
        const int MAX_CLIENTS = 1000;
        const int PORT = 16707;

        public AsyncCallback pfnWorkerCallBack;
        private Socket m_mainSocket;
        private Socket[] m_workerSocket = new Socket[10];
        private int m_clientCount = 0;
        private List<Device> listDeviceConneted;
        private List<Device> listDeviceInfo;
        List<GroupUser> groupUsers;
        List<User> users;
        List<Schedule> schedules;
        List<Door> doorsConnect;

        DataTable dtEvent;

        public frmMain()
        {
            InitializeComponent();           
            UpdateControls(false);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //UpdateAppConfig("10.173.24.199:8088");
            dtpDate.EditValue = DateTime.Now;
            radioClient.Checked = true;
        }

        private static void UpdateAppConfig(String Name)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                XmlNodeList endpoints = doc.GetElementsByTagName("endpoint");
                foreach (XmlNode item in endpoints)
                {
                    var addressAttribute = item.Attributes["address"];
                    if (!ReferenceEquals(null, addressAttribute))
                    {
                        addressAttribute.Value = "http://" + Name + "/WSACU.ASMX";

                    }
                }
                doc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #region Common

        /// <summary>
        /// Get all schedule from DB and add to checkedCombobox
        /// </summary>
        private void GetAllSchedule()
        {
            try
            {
                //Get all schedule
                schedules = new List<Schedule>();
                Schedule schedule = new Schedule("", "");
                schedules = schedule.GetSchedule();

                if (schedules != null)
                {
                    //Add item into checkedCombobox Schedule - Tab card
                    chkcboSchedule.Properties.DataSource = schedules;
                    chkcboSchedule.Properties.ValueMember = "ScheduleId";
                    chkcboSchedule.Properties.DisplayMember = "ScheduleName";
                    chkcboSchedule.Properties.NullText = "Choose Schedule";

                    //Add item into checkedCombobox Schedule - Tab right
                    chkcboRight.Properties.DataSource = schedules;
                    chkcboRight.Properties.ValueMember = "ScheduleId";
                    chkcboRight.Properties.DisplayMember = "ScheduleName";
                    chkcboRight.Properties.NullText = "Choose Schedule";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Get all user group from DB and add to checkedCombobox
        /// </summary>
        private void GetAllGroupUser()
        {
            try
            {
                //Get all user group
                groupUsers = new List<GroupUser>();
                GroupUser groupUser = new GroupUser("", "");
                groupUsers = groupUser.GetUserGroup();

                if (groupUsers != null)
                {
                    //Add item into checkedCombobox group user - Tab card
                    chkcboGUser.Properties.DataSource = groupUsers;
                    chkcboGUser.Properties.ValueMember = "Id";
                    chkcboGUser.Properties.DisplayMember = "Name";
                    chkcboGUser.Properties.NullText = "Choose group user";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Get all user from DB
        /// </summary>
        private void GetAllUser()
        {
            try
            {
                //Get all user group
                users = new List<User>();
                User u = new User("", "");
                users = u.GetUsers();

                //Add item into checkedCombobox user - Tab card
                chkcboUsers.Properties.DataSource = users;
                chkcboUsers.Properties.ValueMember = "Id";
                chkcboUsers.Properties.DisplayMember = "Name";
                chkcboUsers.Properties.NullText = "Choose user";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Init data table structure for store event data
        /// </summary>
        private void InitDataTableEvent()
        {
            dtEvent = new DataTable();
            dtEvent.Columns.Add("time", typeof(DateTime));
            dtEvent.Columns.Add("device", typeof(string));
            dtEvent.Columns.Add("function", typeof(string));
            dtEvent.Columns.Add("data", typeof(string));
            gridEvent.DataSource = dtEvent;
        }

        /// <summary>
        /// Send message to selected device and show result in memoedit
        /// </summary>
        /// <param name="messages">List of messages to send</param>
        /// <param name="memoEdit">memoedit for show result</param>
        /// <param name="mode">RIGHT: send right; CARD: send card</param>
        private void SendMessageToDevice(List<byte[]> messages, MemoEdit memoEdit, string mode)
        {
            try
            {
                List<Device> listDeviceSend = new List<Device>();
                //Get selected device
                Device device = null; ;
                string id = lookupDevice.Text;
                if (id == "Choose device...")
                {
                    MessageBox.Show("Please select device!");
                    return;
                }
                foreach (Device d in listDeviceConneted)
                {
                    if (id == d.ID)
                    {
                        device = d;
                    }
                }

                for (int i = 0; i < m_clientCount; i++)
                {
                    IPEndPoint remoteIpEndPoint = m_workerSocket[i].RemoteEndPoint as IPEndPoint;

                    if (device.IP == remoteIpEndPoint.Address.ToString())
                    {
                        if (m_workerSocket[i] != null)
                        {
                            if (m_workerSocket[i].Connected)
                            {
                                memoEdit.Text += string.Format("\r\nTransering data to device {0}...", device.Name);
                                foreach (byte[] b in messages)
                                {
                                    try
                                    {
                                        m_workerSocket[i].Send(b);
                                    }
                                    catch (SocketException se)
                                    {
                                        MessageBox.Show(se.Message);
                                    }
                                }
                                if (mode == "RIGHT")
                                {
                                    timer1.Interval = 1000;
                                    timer1.Start();
                                }
                                else
                                    if (mode == "CARD")
                                {
                                    timer2.Interval = 1000;
                                    timer2.Start();
                                }
                                else
                                    if (mode == "DATE")
                                {
                                    timer2.Interval = 1000;
                                    timer2.Start();
                                }
                                else
                                    if (mode == "IP")
                                {
                                    timer2.Interval = 1000;
                                    timer2.Start();
                                }
                            }
                            else
                            {
                                listIP.Text += string.Format("\r\n Device {0} disconnect", device.IP);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
        private void SendMessageToDevice(List<byte[]> messages, string IP)
        {
            try
            {
                for (int i = 0; i < m_clientCount; i++)
                {
                    IPEndPoint remoteIpEndPoint = m_workerSocket[i].RemoteEndPoint as IPEndPoint;

                    if (IP == remoteIpEndPoint.Address.ToString())
                    {
                        if (m_workerSocket[i] != null)
                        {
                            if (m_workerSocket[i].Connected)
                            {
                                foreach (byte[] b in messages)
                                {
                                    try
                                    {
                                        m_workerSocket[i].Send(b);
                                    }
                                    catch (SocketException se)
                                    {
                                        MessageBox.Show(se.Message);
                                    }
                                }
                            }
                            else
                            {
                                listIP.Text += string.Format("\r\n Device {0} disconnect", IP);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
        
        /// <summary>
        /// Update button enable status
        /// </summary>
        /// <param name="listening">Start: true; Stop: false</param>
        private void UpdateControls(bool listening)
        {
            btnConnect.Enabled = !listening;
            btnDisconnect.Enabled = listening;
            //panelControl.Enabled = listening;
            btnTransfer.Enabled = listening;
            btnSendRight.Enabled = listening;
            xtraTabControl1.Enabled = listening;
        }

        String GetIP()
        {
            String strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

            // Grab the first IP addresses
            String IPStr = "";
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                IPStr = ipaddress.ToString();
                return IPStr;
            }
            return IPStr;
        }

        /// <summary>
        /// This is the call back function, which will be invoked when a client is connected
        /// </summary>
        /// <param name="asyn"></param>
        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                // Here we complete/end the BeginAccept() asynchronous call
                // by calling EndAccept() - which returns the reference to
                // a new Socket object
                m_workerSocket[m_clientCount] = m_mainSocket.EndAccept(asyn);
                // Let the worker Socket do the further processing for the 
                // just connected client
                WaitForData(m_workerSocket[m_clientCount]);
                //Get IP of client	
                IPEndPoint remoteIpEndPoint = m_workerSocket[m_clientCount].RemoteEndPoint as IPEndPoint;

                //Add device to combobox
                foreach (Device device in listDeviceInfo)
                {
                    if (device.IP == remoteIpEndPoint.Address.ToString())
                    {
                        listDeviceConneted.Add(device);
                        if (this.lookupDevice.InvokeRequired)
                        {
                            this.lookupDevice.BeginInvoke((MethodInvoker)delegate ()
                            {
                                this.lookupDevice.Properties.DataSource = listDeviceConneted;
                                this.lookupDevice.Properties.ValueMember = "IP";
                                this.lookupDevice.Properties.DisplayMember = "ID";
                            });
                        }
                        else
                        {
                            this.lookupDevice.Properties.DataSource = listDeviceConneted;
                        }

                    }
                }

                // Display this client connection as a status message on the GUI
                String str = String.Format("Device {0} connected", remoteIpEndPoint.Address);

                // Now increment the client count
                ++m_clientCount;
                if (this.listIP.InvokeRequired)
                {
                    this.listIP.BeginInvoke((MethodInvoker)delegate () { this.listIP.Text += ("\r\n" + str); });
                }
                else
                {
                    this.listIP.Text = str;
                }

                //lblStatus.Text = str;                

                // Since the main Socket is now free, it can go back and wait for
                // other clients who are attempting to connect
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }

        // Start waiting for data from the client
        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = soc;
                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.dataBuffer, 0,
                                   theSocPkt.dataBuffer.Length,
                                   SocketFlags.None,
                                   pfnWorkerCallBack,
                                   theSocPkt);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }
        public class SocketPacket
        {
            public System.Net.Sockets.Socket m_currentSocket;

            public byte[] dataBuffer = new byte[100];
        }
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                int iRx = 0;
                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                iRx = socketData.m_currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.dataBuffer,
                                         0, iRx, chars, 0);
                System.String szData = new System.String(chars);

                //Get cilent IP
                IPEndPoint remoteIpEndPoint = socketData.m_currentSocket.RemoteEndPoint as IPEndPoint;
                string IP = remoteIpEndPoint.Address.ToString();

                //Get string message 
                string message = BitConverter.ToString(socketData.dataBuffer);

                //Get function code
                string strCode = ArrayToStringNonInt(socketData, 0, 1);
                string code = string.Format("0x{0}", strCode);
                string functionName = Common.GetEnumName(code);

                //Get messlength
                string strLength = ArrayToStringNonInt(socketData, 2, 3);
                int intMesslength = int.Parse(strLength, System.Globalization.NumberStyles.HexNumber);

                //Get message data
                string strData = ArrayToStringNonInt(socketData, 4, intMesslength);

                //Add to table event
                DataRow row = dtEvent.NewRow();
                row["time"] = DateTime.Now;
                row["device"] = IP;
                row["function"] = functionName;
                row["data"] = strData;

                if (this.gridEvent.InvokeRequired)
                {
                    this.gridEvent.BeginInvoke((MethodInvoker)delegate ()
                    {
                        dtEvent.Rows.InsertAt(row, 0);
                        gridEvent.DataSource = dtEvent;
                    });
                }
                else
                {
                    dtEvent.Rows.InsertAt(row, 0);
                    gridEvent.DataSource = dtEvent;
                }

                //If event is SEND_EVENT_RSP, save to DB
                if (functionName == "SEND_EVENT_RSP")
                {
                    int length = 5;
                    AddEventToDB(socketData, IP, functionName);

                    byte[] a = BitConverter.GetBytes((UInt16)Common.FunctionCode.SEND_EVENT_ACK);
                    List<byte> list = new List<byte>();
                    list.Add(a[1]);
                    list.Add(a[0]);

                    byte[] mlength = BitConverter.GetBytes((UInt16)length);
                    byte messId = socketData.dataBuffer[4];
                    List<byte> data = new List<byte>();
                    data.AddRange(list.ToArray());
                    data.Add(mlength[1]);
                    data.Add(mlength[0]);
                    data.Add(messId);
                    List<byte[]> send = new List<byte[]>();
                    send.Add(data.ToArray());
                    SendMessageToDevice(send, IP);
                }

                // Continue the waiting for data on the Socket
                WaitForData(socketData.m_currentSocket);
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void AddEventToDB(SocketPacket p, string deviceIP, string functionName)
        {
            try
            {
                string eventId = ArrayToString(p, 4, 4);
                string cardNo = ArrayToString(p, 5, 8);

                string strHour = ArrayToString(p, 9, 9);
                string strMin = ArrayToString(p, 10, 10);
                string strSec = ArrayToString(p, 11, 11);
                string strDay = ArrayToString(p, 12, 12);
                string strMonth = ArrayToString(p, 13, 13);
                string strYear = (Convert.ToInt16(ArrayToString(p, 14, 14)) + 1900).ToString();
                string myDate = string.Format("{0}/{1}/{2} {3}:{4}:{5}", strYear, strMonth, strDay, strHour, strMin, strSec);
                //DateTime eventDate = DateTime.ParseExact(myDate, "dd/MM/yyyy HH:mm:ss",System.Globalization.CultureInfo.InvariantCulture);
                DateTime eventDate = Convert.ToDateTime(myDate);
                string doorId = ArrayToString(p, 15, 15);

                string strStatus = ArrayToString(p, 16, 16);
                string eventStatus = "";
                if (strStatus == "0")
                {
                    eventStatus = "GRANTED";
                }
                else
                    if (strStatus == "1")
                {
                    eventStatus = "DENY";
                }
                else
                    if (strStatus == "2")
                {
                    eventStatus = "NOT_DEFINED";
                }

                Event e = new Event(eventId, eventDate, deviceIP, functionName, cardNo, doorId, eventStatus);
                e.AddEvent();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private string ArrayToString(SocketPacket p, int startPos, int endPos)
        {
            string result = null;
            try
            {
                int byteCount = endPos - startPos + 1;                
                List<byte> byteArr = new List<byte>();
                for (int i = startPos; i <= endPos; i++)
                {
                    byteArr.Add(p.dataBuffer[i]);
                }
                result = BitConverter.ToString(byteArr.ToArray());
                if (byteCount > 1)
                {
                    for (int i = 1; i < byteCount; i++)
                    {
                        result = result.Remove(i * 2, 1);
                    }
                }
                result = int.Parse(result, System.Globalization.NumberStyles.HexNumber).ToString();
                return result;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }

        private string ArrayToStringNonInt(SocketPacket p, int startPos, int endPos)
        {
            string result = null;
            try
            {
                int byteCount = endPos - startPos + 1;
                List<byte> byteArr = new List<byte>();
                for (int i = startPos; i <= endPos; i++)
                {
                    byteArr.Add(p.dataBuffer[i]);
                }
                result = BitConverter.ToString(byteArr.ToArray());
                if (byteCount > 1)
                {
                    for (int i = 1; i < byteCount; i++)
                    {
                        result = result.Remove(i * 2, 1);
                    }
                }
                return result;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }

        void CloseSockets()
        {
            if (m_mainSocket != null)
            {
                m_mainSocket.Close();
            }
            for (int i = 0; i < m_clientCount; i++)
            {
                if (m_workerSocket[i] != null)
                {
                    m_workerSocket[i].Close();
                    m_workerSocket[i] = null;
                }
            }
        }

        static string GetIntBinaryString(int n)
        {  
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                    b[pos] = '1';
                else
                    b[pos] = '0';
                pos--;
                i++;
            }
            return new string(b);   

        }

        static int OneBit(int value, int position)
        {
            value |= (1 << position);
            return value;
        }

        #endregion

        #region Event
        private void listIP_MouseUp(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                popupConnection.ShowPopup(Control.MousePosition);
            }
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //Clear memoIPlist
            listIP.Text = "";
        }

        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //Clear memo card
            memoHisCard.Text = "";
        }

        private void barButtonItem6_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            //Clear memo right
            memoHisRight.Text = "";
        }

        private void memoHisRight_MouseUp(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                popupRight.ShowPopup(Control.MousePosition);
            }
        }

        private void memoHisCard_MouseUp(object sender, MouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                popupCard.ShowPopup(Control.MousePosition);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                timer1.Stop();
                memoHisRight.Text += string.Format("\r\nCompleted");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                timer2.Stop();
                memoHisCard.Text += string.Format("\r\nCompleted");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                listDeviceConneted = new List<Device>();
                //InitData
                GetAllUser();
                GetAllGroupUser();
                GetAllSchedule();
                InitDataTableEvent();
                listIP.Text = "Start listen!";

                // Create the listening socket...
                m_mainSocket = new Socket(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, PORT);
                // Bind to local IP Address...
                m_mainSocket.Bind(ipLocal);
                // Start listening...
                m_mainSocket.Listen(100);
                // Create the call back for any client connections...
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);

                UpdateControls(true);

                //Get device info from server
                Device device = new Device("", "", "");
                listDeviceInfo = device.GetDevices();

            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            CloseSockets();
            UpdateControls(false);
            listDeviceConneted.Clear();
            lookupDevice.Properties.DataSource = null;
            memoHisCard.Text = "";
            memoHisRight.Text = "";
            listIP.Text = "";
        }

        #endregion        

        #region TabCard
        private void btnTransfer_Click(object sender, EventArgs e)
        {
            try
            {
                List<Card> cards = GetCardSend();
                if (cards.Count <= 0)
                {
                    MessageBox.Show("Please select user or group!");
                    return;
                }


                int j = 0;
                foreach (CheckedListBoxItem item in chkcboSchedule.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        j += 1;
                    }

                }

                if (j == 0)
                {
                    MessageBox.Show("Please select schedule!");
                    return;
                }

                int i = 0;
                foreach (CheckedListBoxItem item in chkcboDoor.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        i += 1;
                    }

                }

                if (i == 0)
                {
                    MessageBox.Show("Please select door!");
                    return;
                }

                //Send cards
                List<byte[]> dataSend = CardDataSend();
                SendMessageToDevice(dataSend, memoHisCard,"CARD");

            }

            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void lookupDevice_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                //Load Door by Device
                string id = lookupDevice.Text;
                List<Door> doors = Door.GetDoorsByDevice(id);
                chkcboDoor.Properties.DataSource = doors;
                chkcboDoor.Properties.ValueMember = "readerId";
                chkcboDoor.Properties.DisplayMember = "DoorName";
                chkcboDoor.Properties.NullText = "Choose Door";

                doorsConnect = new List<Door>();
                doorsConnect = doors;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private List<byte> GenDoorRight()
        {
            List<byte> right = new List<byte>();
            try
            {              
                List<byte> d = new List<byte>();
                byte[] zero = BitConverter.GetBytes((UInt32)0);

                List<int> listPos = new List<int>();
                List<int> listPosRight = new List<int>();
                List<byte> d1 = new List<byte>();
                List<byte> d2 = new List<byte>();
                List<byte> d3 = new List<byte>();
                List<byte> d4 = new List<byte>();

                //Get door selected position
                foreach (CheckedListBoxItem item in chkcboDoor.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        int pos = chkcboDoor.Properties.Items.IndexOf(item);
                        listPos.Add(pos + 1);
                    }

                }

                foreach (CheckedListBoxItem item in chkcboSchedule.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        int pos = chkcboSchedule.Properties.Items.IndexOf(item);
                        listPosRight.Add(pos + 1);
                    }

                }

                //Make right data
                d = MakeRightData(listPosRight);
                d1.AddRange(zero);
                d2.AddRange(zero);
                d3.AddRange(zero);
                d4.AddRange(zero);

                //Fill right data in door position
                foreach (int i in listPos)
                {
                    switch (i)
                    {
                        case 1:
                            d1.Clear();
                            d1.AddRange(d);
                            break;
                        case 2:
                            d2.Clear();
                            d2.AddRange(d);
                            break;
                        case 3:
                            d3.Clear();
                            d3.AddRange(d);
                            break;
                        case 4:
                            d4.Clear();
                            d4.AddRange(d);
                            break;
                    }
                }

                right.AddRange(d1);
                right.AddRange(d2);
                right.AddRange(d3);
                right.AddRange(d4);

                return right;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }

        private List<byte> MakeRightData(List<int> listPos)
        {
            List<byte> right = new List<byte>();
            try
            {                
                byte[] zero = BitConverter.GetBytes((UInt32)0);
                int value = 0;

                foreach (int i in listPos)
                {
                    int posreal = i - 1;
                    //int pos = 31 - posreal * 2 + posreal;
                    value = OneBit(value, posreal);

                }
                //string k = GetIntBinaryString(value);
                //string hex = value.ToString("X8");

                byte[] h = BitConverter.GetBytes((UInt32)value);
                right.Add(h[3]);
                right.Add(h[2]);
                right.Add(h[1]);
                right.Add(h[0]);

                return right;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }

        private List<Card> GetCardSend()
        { 
            List<Card> listCardSend = new List<Card>();
            try
            {
                List<User> listUserSend = new List<User>();

                //Get selected users
                foreach (CheckedListBoxItem item in chkcboUsers.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        int pos = chkcboUsers.Properties.Items.IndexOf(item);
                        User u = users[pos];
                        listUserSend.Add(u);
                    }

                }

                //Get user in group user selected
                foreach (CheckedListBoxItem item in chkcboGUser.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        int pos = chkcboGUser.Properties.Items.IndexOf(item);
                        GroupUser group = groupUsers[pos];
                        foreach (User u in group.Users)
                        {
                            if (listUserSend.Contains(u) == false)
                            {
                                listUserSend.Add(u);
                            }
                        }
                    }

                }

                // Ger list card
                foreach (User u in listUserSend)
                {
                    foreach (Card c in u.Cards)
                    {
                        listCardSend.Add(c);
                    }
                }

                return listCardSend;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }

        private List<byte[]> CardDataSend()
        {
            List<byte[]> sendData = new List<byte[]>();
            try
            {
                List<byte> data = new List<byte>();
                List<Card> cards = GetCardSend();

                //Set card qty
                int cardQty = cards.Count;
                string hexQty = cardQty.ToString("X");
                data.Add(Convert.ToByte(hexQty));

                //Add card data
                foreach (Card c in cards)
                {
                    int cardNo = Convert.ToInt32(c.CardNo);
                    byte[] hexCardNo = BitConverter.GetBytes((UInt32)cardNo);
                    data.Add(hexCardNo[3]);
                    data.Add(hexCardNo[2]);
                    data.Add(hexCardNo[1]);
                    data.Add(hexCardNo[0]);
                    List<byte> t = GenDoorRight();
                    data.AddRange(GenDoorRight());
                }

                byte[] functionCode = BitConverter.GetBytes((UInt16)Common.FunctionCode.ADD_CARD);
                Message message = new Message(functionCode, data);
                sendData.Add(message.MakeMassageStructure());

                return sendData;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private List<byte[]> DateTimeDataSend(DateTime dateTime)
        {
            List<byte[]> sendData = new List<byte[]>();
            try
            {
                List<byte> data = new List<byte>();                

                //Set date time
                string hour = dateTime.Hour.ToString("X");
                data.Add(Convert.ToByte(hour));

                string min = dateTime.Minute.ToString("X");
                data.Add(Convert.ToByte(min));

                string second = dateTime.Second.ToString("X");
                data.Add(Convert.ToByte(second));

                string weekday = ((int)dateTime.DayOfWeek).ToString("X");
                data.Add(Convert.ToByte(weekday));

                string day = dateTime.Day.ToString("X");
                data.Add(Convert.ToByte(day));

                string month = dateTime.Month.ToString("X");
                data.Add(Convert.ToByte(month));

                string year = (dateTime.Year-1900).ToString("X");
                data.Add(Convert.ToByte(year));                

                byte[] functionCode = BitConverter.GetBytes((UInt16)Common.FunctionCode.SET_RTC);
                Message message = new Message(functionCode, data);
                sendData.Add(message.MakeMassageStructure());

                return sendData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private List<byte[]> IPSend()
        {
            List<byte[]> sendData = new List<byte[]>();
            try
            {
                List<byte> data = new List<byte>();

                string ip = txtIP.Text;
                string subnet = txtSubnet.Text;
                string gateway = txtGateway.Text;
                string host = txtHost.Text;

                data.AddRange(IPtoByte(ip));
                data.AddRange(IPtoByte(subnet));
                data.AddRange(IPtoByte(gateway));
                data.AddRange(IPtoByte(host));                

                byte[] functionCode = BitConverter.GetBytes((UInt16)Common.FunctionCode.SET_STATIC_IP);
                Message message = new Message(functionCode, data);
                sendData.Add(message.MakeMassageStructure());

                return sendData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private List<byte[]> IPServerSend()
        {
            List<byte[]> sendData = new List<byte[]>();
            try
            {
                List<byte> data = new List<byte>();

                string ip = txtIP.Text;

                data.AddRange(IPtoByte(ip));

                byte[] functionCode = BitConverter.GetBytes((UInt16)Common.FunctionCode.SET_NTP_SERVER);
                Message message = new Message(functionCode, data);
                sendData.Add(message.MakeMassageStructure());

                return sendData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }
        }

        private byte[] IPtoByte(string IP)
        {
            byte[] arrIP = null;
            string[] arr = IP.Split('.');
            for(int i=0;i<arr.Count();i++)
            {
                arrIP[i] = Convert.ToByte(Convert.ToInt16(arr[i]).ToString("X"));
            }
            return arrIP;
        }

        #endregion

        #region TabRight
        private void btnSendRight_Click(object sender, EventArgs e)
        {
            try
            {
                int i = 0;
                foreach (CheckedListBoxItem item in chkcboRight.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        i += 1;
                    }

                }

                if (i == 0)
                {
                    MessageBox.Show("Please select schedule!");
                    return;
                }

                //Send right
                List<byte[]> dataSend = RightDataSend();
                SendMessageToDevice(dataSend,memoHisRight,"RIGHT");              

            }

            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private List<byte[]> RightDataSend()
        {
            List<byte[]> sendData = new List<byte[]>();
            try
            {
                List<Message> messages = new List<Message>();
                List<Period> listPeriodSend = new List<Period>();

                //Get list message to send
                foreach (CheckedListBoxItem item in chkcboRight.Properties.Items)
                {
                    if (item.CheckState == CheckState.Checked)
                    {
                        int pos = chkcboRight.Properties.Items.IndexOf(item);
                        Schedule s = schedules[pos];

                        //Add Id
                        List<byte> data = new List<byte>();
                        byte id = Convert.ToByte(Convert.ToInt32(s.ScheduleId.Substring(5, 5)).ToString("X2"));
                        data.Add(id);

                        //Add period qty
                        byte pQty = Convert.ToByte(s.ListPeriod.Count.ToString("X2"));
                        data.Add(pQty);

                        //Add period detail
                        foreach (Period p in s.ListPeriod)
                        {
                            string dayInWeek = Common.DayInWeekToInt(p.DayInWeek).ToString("X2");
                            string hexSHour = p.StartTime.Hour.ToString("X2");
                            string hexSMin = p.StartTime.Minute.ToString("X2");
                            string hexEHour = p.EndTime.Hour.ToString("X2");
                            string hexEMin = p.EndTime.Minute.ToString("X2");

                            data.Add(Convert.ToByte(dayInWeek, 16));
                            data.Add(Convert.ToByte(hexSHour, 16));
                            data.Add(Convert.ToByte(hexSMin, 16));
                            data.Add(Convert.ToByte(hexEHour, 16));
                            data.Add(Convert.ToByte(hexEMin, 16));
                        }

                        byte[] functionCode = BitConverter.GetBytes((UInt16)Common.FunctionCode.SET_PERMISSION);
                        Message message = new Message(functionCode, data);
                        sendData.Add(message.MakeMassageStructure());

                    }
                }

                return sendData;
            }
            catch(Exception ex)
            {
                return null;
                MessageBox.Show(ex.ToString());
            }
        }


        #endregion

        #region TabSetting
        private void btnSetTime_Click(object sender, EventArgs e)
        {
            try
            {
                int year = Convert.ToDateTime(dtpDate.EditValue).Year;
                int month = Convert.ToDateTime(dtpDate.EditValue).Month;
                int day = Convert.ToDateTime(dtpDate.EditValue).Day;
                int hour = Convert.ToDateTime(dtpTime.EditValue).Hour;
                int min = Convert.ToDateTime(dtpTime.EditValue).Minute;
                int sec = Convert.ToDateTime(dtpTime.EditValue).Second;

                DateTime dateTime = new DateTime(year, month, day, hour, min, sec);

                //Send Time
                List<byte[]> dataSend = DateTimeDataSend(dateTime);
                SendMessageToDevice(dataSend, memoSetting, "DATE");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnUpdateTime_Click(object sender, EventArgs e)
        {
            try
            {
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                int day = DateTime.Now.Day;
                int hour = DateTime.Now.Hour;
                int min = DateTime.Now.Minute;
                int sec = DateTime.Now.Second;

                DateTime dateTime = new DateTime(year, month, day, hour, min, sec);

                //Send Time
                List<byte[]> dataSend = DateTimeDataSend(dateTime);
                SendMessageToDevice(dataSend, memoSetting, "DATE");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSetIP_Click(object sender, EventArgs e)
        {
            try
            {
                if (radioServer.Checked)
                {
                    //Set IP server
                    List<byte[]> dataSend = IPServerSend();
                    SendMessageToDevice(dataSend, memoSetting, "IP");
                }
                else
                if (radioClient.Checked)
                {
                    //Set IP
                    List<byte[]> dataSend = IPSend();
                    SendMessageToDevice(dataSend, memoSetting, "IP");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void radioServer_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioServer.Checked)
            {
                txtGateway.Enabled = true;
                txtHost.Enabled = true;
                txtSubnet.Enabled = true;
            }
            else
            {
                txtGateway.Enabled = false;
                txtHost.Enabled = false;
                txtSubnet.Enabled = false;
            }
        }
        #endregion     
       
    }
}
