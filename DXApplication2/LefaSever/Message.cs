using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LefaSever
{
    class Message
    {
        public Message() { }
        public Message(byte[] functionCode, List<byte> data, byte[] respone)
        {
            FunctionCode = functionCode;            
            Data = data;
            Respone = respone;
        }

        public Message(byte[] functionCode, List<byte> data)
        {
            FunctionCode = functionCode;
            Data = data;
        }

        private byte[] functionCode;

        public byte[] FunctionCode
        {
            get { return functionCode; }
            set { functionCode = value; }
        }

        private byte messageLength;

        public byte MessageLength
        {
            get { return messageLength; }
            set { messageLength = value; }
        }

        private List<byte> data;

        public List<byte> Data
        {
            get { return data; }
            set { data = value; }
        }

        private byte[] respone;

        public byte[] Respone
        {
            get { return respone; }
            set { respone = value; }
        }

        public byte[] MakeMassageStructure()
        {
            List<byte> messData = new List<byte>();
            int messLength = data.Count + 4;
         
            messData.Add(functionCode[1]);
            messData.Add(functionCode[0]);

            byte[] mLength = BitConverter.GetBytes((UInt16)messLength);

            messData.Add(mLength[1]);
            messData.Add(mLength[0]);

            messData.AddRange(data);
            return messData.ToArray();
        }

    }
}
