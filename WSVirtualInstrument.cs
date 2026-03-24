using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Ports;
using System.Diagnostics;

namespace WingForce
{

    class WSVirtualInstrument : Object

    {
        //连接类型
        public int ConnectionType = -1;

        //GPIB设备
        public ArrayList HoldCommandList = new ArrayList();

        //RS232设备
        public SerialPort RS232Port;
        public bool RS232DataReady = false;
        public byte[] rs232recvRawData = new byte[0]; 



        //设备ID
        public string ID = "";

        //设备编号
        public int index = -1;

        //仪器类型
        public int InstrumentType = -1;

        //收取的数据
        public string ReceivedString { get; set; }

        public bool IsRefreshingData = false;

        public WSVirtualInstrument(){
      
            }

        //写入命令
        public void Write(String WriteContent)
        {

            string writeCommand = WriteContent;

            if (this.RS232Port == null)
            {
                return;
            }

            try
            {
                if (this.RS232Port.IsOpen == true)
                {
                    char[] data = (writeCommand + "\r\n").ToCharArray();
                    RS232Port.Write(data, 0, data.Length);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("通讯写入错误" + writeCommand + "，" + ex.Message);
            }

        }


        public byte[] ReadRS232RawData()
        {
            byte[] data = this.rs232recvRawData;

            this.rs232recvRawData = new byte[0];

            return data;
        }

        //读取数据
        public string ReadString()
        {
            string dataString = null;
            dataString = this.ExtractRS232String();

            return dataString;
        }

        public string GPIBReadString()
        {
            string newData = null;
            try
            {

            }
            catch (Exception ex)
            {
                Debug.WriteLine("GPIB读取错误：" + ex.Message + this.ID);
            }
            return newData;

        }

        public void ExecuteHoldCommandList()
        {
            foreach (string command in HoldCommandList)
            {
                Write(command);
            }
            HoldCommandList.Clear();
        }


        public void SetRS232Recevier()
        {
            RS232Port.DataReceived += new SerialDataReceivedEventHandler(ReceiveRS232Data);
        }



        public void ReceiveRS232Data(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                System.Threading.Thread.Sleep(30);

                SerialPort _SerialPort = (SerialPort)sender;

                int _bytesToRead = _SerialPort.BytesToRead;

                byte[] newData = new byte[_bytesToRead];

                _SerialPort.Read(newData, 0, _bytesToRead);

                rs232recvRawData = rs232recvRawData.Concat(newData).ToArray();

                string newString = System.Text.Encoding.ASCII.GetString(rs232recvRawData);

                ReceivedString = newString;

                RS232DataReady = true;



            }
            catch (Exception ex)
            {
                Debug.WriteLine("RS232读取错误" + ex.Message);

            }
        }

        public string ExtractRS232String()
        {
            //增加超时检测
            int OverTimeCounter = 0;

            while( (ReceivedString == null || RS232DataReady == false) && OverTimeCounter < 300)
            {
                System.Threading.Thread.Sleep(1);
                OverTimeCounter++;
            }

            //Debug.WriteLine("读取耗时:" + OverTimeCounter*10);
            string tmpData = ReceivedString;
            if(tmpData == null)
            {
                tmpData = 0.ToString();
            }

            //清空BUFFER及FLAG
            ReceivedString = null;
            rs232recvRawData = new byte[0];
            RS232DataReady = false;

            return tmpData;
        }

        public bool InitialRS232Device(String portName, ArrayList VIList, int baudRate)
        {

            bool initialSucceeded = false;

            try
            {
                SerialPort port = new SerialPort();
                //串口名称
                port.PortName = portName;
                //波特率
                port.BaudRate = baudRate;
                //数据位
                port.DataBits = 8;
                //停止位
                port.StopBits = StopBits.One;
                //校验位
                port.Parity = Parity.None;

                //握手
                //port.DtrEnable = true;
                //port.RtsEnable = true;

                //port.ReceivedBytesThreshold = 1;

                //打开串口
                port.Open();

                this.RS232Port = port;
                this.SetRS232Recevier();

                System.Threading.Thread.Sleep(100);

                Debug.WriteLine(this.ReadString());

                System.Threading.Thread.Sleep(100);


                //获取ID
                if (this.RS232Port != null && this.RS232Port.IsOpen)
                {
                    this.Write("*IDN?");
                    System.Threading.Thread.Sleep(200);
                }

                Debug.WriteLine("RS232--" + portName + "，ID：" + this.ReceivedString + "\r\n");
                this.ID = this.ReceivedString;


                Debug.WriteLine(this.ID);

                this.ReceivedString = null;
                this.rs232recvRawData = new byte[0];

                //初始化仪器
                initialSucceeded = this.InitialInstrument(VIList);
            }
            catch (Exception ex)
            {
                Debug.Write("RS232连接错误：" + ex.Message);
                this.ID = ex.Message;
                initialSucceeded = false;
            }

            if (initialSucceeded == false)
            {
                this.DisconnectDevice();
            }
            return initialSucceeded;
        }


        public bool InitialInstrument(ArrayList VIList)
        {
            bool initialSuccessed = false;

            if (this.ID == null)
            {
                return false;
            }

            if (this.ID.Contains("WINGSTUDIO 12V-2X6 Ti"))
            {
                this.InstrumentType = 1;

                
            }

            if (this.InstrumentType > -1)
            {
                this.index = VIList.Count;
                VIList.Add(this);
                initialSuccessed = true;

                Debug.WriteLine("COM设备连接成功: " + this.ID);
            }
            else
            {
                this.ID = null;
                initialSuccessed = false;
            }
            return initialSuccessed;
        }

        public void DisconnectDevice()
        {
            try
            {

                if (this.RS232Port != null)
                {
                    this.RS232Port.Close();
                    this.RS232Port.Dispose();
                    this.RS232Port = null;
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show("设备断开错误：" + ex.Message);
            }


        }

        public void ClearCommandList()
        {
            HoldCommandList.Clear();
            HoldCommandList.Add("abort");
        }
    }
}
