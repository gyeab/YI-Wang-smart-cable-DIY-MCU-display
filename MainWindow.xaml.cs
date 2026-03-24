using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WingForce;

namespace WingForce
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private const string DefaultPortName = "COM5";


        WSVirtualInstrument? WS12V2X6Ti = null;
        PreviewWindow? PreviewWindowInstance = null;
    DisplayPreviewCustomization PreviewCustomization = DisplayPreviewCustomization.LoadOrDefault();

        private ArrayList VIList = new ArrayList();

        bool UpdatingSensorData = false;

        bool WarningTesting = false;
        bool ProtectionTesting = false;

        readonly double[] PreviewChannelVoltages = new double[6];
        readonly double[] PreviewChannelCurrents = new double[6];
        double PreviewTotalPower = 0;
        double PreviewCableTemp = 0;
        double PreviewBoardTemp = 0;

        ArrayList ChannelDataList = new ArrayList();

        System.Timers.Timer UpdateTimer = new System.Timers.Timer();

        public MainWindow()
        {

            InitializeComponent();
            RefreshAvailablePorts();

            ScreenOrientSelectBox.SelectionChanged += PreviewInputChanged;
            FanControlTypeBox.SelectionChanged += PreviewInputChanged;
            ShuntResistanceBox.TextChanged += PreviewInputChanged;
            CurrentCorrectionBox.TextChanged += PreviewInputChanged;
            FanOnTempBox.TextChanged += PreviewInputChanged;
            WarningTempBox.TextChanged += PreviewInputChanged;
            WarningCurrentBox.TextChanged += PreviewInputChanged;
            WarningTimeOutBox.TextChanged += PreviewInputChanged;
            ProtectCurrentBox.TextChanged += PreviewInputChanged;
            ProtectTempBox.TextChanged += PreviewInputChanged;


            UpdateTimer.Interval = 3500;
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateTimerElapsed);
            UpdateTimer.AutoReset = true;

           // Thread autoSearchThread = new Thread(AutoSearchDevice);
           // autoSearchThread.Start();
        }



        private void AutoSearchDevice()
        {
            System.Threading.Thread.Sleep(200);
            this.Dispatcher.Invoke(new Action(delegate
            {
                RoutedEventArgs e = new RoutedEventArgs();
                this.ConnectionButton_Click(this, e);
            }
            ));
        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string? selectedPort = PortSelectBox.SelectedItem as string;
            bool attemptedSelectedPort = false;

            if (string.IsNullOrWhiteSpace(selectedPort) == false)
            {
                attemptedSelectedPort = true;

                string? errorMessage = TryConnectToPort(selectedPort, true);
                if (VIList.Count == 0 && string.IsNullOrWhiteSpace(errorMessage) == false)
                {
                    MessageBox.Show(errorMessage);
                }
            }

            if (VIList.Count == 0 && attemptedSelectedPort == false)
            {
                foreach (String portName in SerialPort.GetPortNames())
                {
                    TryConnectToPort(portName, false);

                    if (VIList.Count > 0)
                    {
                        break;
                    }
                }
            }

            //连接成功初始化
            if (VIList.Count > 0)
            {
                WS12V2X6Ti = (WSVirtualInstrument)VIList[0];

                ConnectionButton.IsEnabled = false;
                PortSelectBox.IsEnabled = false;
                RefreshPortsButton.IsEnabled = false;

                UpdatingSensorData = false;

                string[] channelIndex = { "A", "B", "C", "D", "E", "F" }; 

                for (int i = 0; i < 6; i++) {
                    WFChannelData channel = new WFChannelData();
                    channel.Id = i;
                    channel.ChannelName = "CH-" + channelIndex[i];
                    
                    ChannelDataList.Add(channel);
                }

                ChannelDataListView.ItemsSource = ChannelDataList;

                this.ReadConfigButton_Click(this, e);

                UpdateTimer.Start();
            }
            else
            {
                if (attemptedSelectedPort)
                {
                    return;
                }

                MessageBox.Show("Device not found. Select the MCU COM port and try again.");
            }
        }

        private string? TryConnectToPort(string portName, bool showDetailedError)
        {
            bool initialSuccessed = false;
            WSVirtualInstrument tmpVI = new WSVirtualInstrument();

            try
            {
                initialSuccessed = tmpVI.InitialRS232Device(portName, VIList, 115200);
            }
            catch (Exception ex)
            {
                Debug.Write("RS232连接错误：" + ex.Message);
                if (showDetailedError)
                {
                    return "RS232 connection error on " + portName + ": " + ex.Message;
                }

                return ex.Message;
            }

            if (initialSuccessed == false)
            {
                if (showDetailedError)
                {
                    string detail = string.IsNullOrWhiteSpace(tmpVI.ID) ? "Unknown error" : tmpVI.ID;
                    return "COM port open failed on " + portName + ": " + detail;
                }

                Debug.WriteLine("COM设备连接错误:" + tmpVI.ID);
            }

            return null;
        }

        private void RefreshPortsButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshAvailablePorts();
        }

        private void RefreshAvailablePorts()
        {
            string? previousSelection = PortSelectBox.SelectedItem as string;
            PortSelectBox.ItemsSource = null;

            string[] portNames = SerialPort.GetPortNames();
            Array.Sort(portNames, StringComparer.OrdinalIgnoreCase);

            PortSelectBox.ItemsSource = portNames;

            if (portNames.Length == 0)
            {
                PortSelectBox.Text = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(previousSelection) == false)
            {
                foreach (string portName in portNames)
                {
                    if (string.Equals(portName, previousSelection, StringComparison.OrdinalIgnoreCase))
                    {
                        PortSelectBox.SelectedItem = portName;
                        return;
                    }
                }
            }

            foreach (string portName in portNames)
            {
                if (string.Equals(portName, DefaultPortName, StringComparison.OrdinalIgnoreCase))
                {
                    PortSelectBox.SelectedItem = portName;
                    return;
                }
            }

            PortSelectBox.SelectedIndex = 0;
        }

        private void PreviewInputChanged(object sender, EventArgs e)
        {
            UpdatePreviewWindow();
        }

        private void PreviewDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            if (PreviewWindowInstance == null || PreviewWindowInstance.IsLoaded == false)
            {
                PreviewWindowInstance = new PreviewWindow(PreviewCustomization);
                PreviewWindowInstance.Owner = this;
                PreviewWindowInstance.CustomizationChanged += PreviewWindowCustomizationChanged;
                PreviewWindowInstance.Closed += (_, _) => PreviewWindowInstance = null;
            }

            PreviewWindowInstance.Show();
            PreviewWindowInstance.Activate();
            UpdatePreviewWindow();
        }

        private void UpdatePreviewWindow()
        {
            if (PreviewWindowInstance == null || PreviewWindowInstance.IsLoaded == false)
            {
                return;
            }

            PreviewWindowInstance.UpdatePreview(BuildPreviewState());
        }

        private void PreviewWindowCustomizationChanged(DisplayPreviewCustomization customization)
        {
            PreviewCustomization = customization.Clone();
            PreviewCustomization.Save();
        }

        private DisplayPreviewState BuildPreviewState()
        {
            int fanControlType = FanControlTypeBox.SelectedIndex < 0 ? 0 : FanControlTypeBox.SelectedIndex;
            double fanOnTemp = ParseDoubleOrDefault(FanOnTempBox.Text, 45);
            double alarmTemperature = ParseDoubleOrDefault(WarningTempBox.Text, 90);
            double alarmCurrent = ParseDoubleOrDefault(WarningCurrentBox.Text, 12);

            return new DisplayPreviewState
            {
                ScreenOrientationIndex = ScreenOrientSelectBox.SelectedIndex < 0 ? 0 : ScreenOrientSelectBox.SelectedIndex,
                AlarmTemperature = alarmTemperature,
                AlarmCurrent = alarmCurrent,
                FanOnTemperature = fanOnTemp,
                FanControlType = fanControlType,
                TotalPower = PreviewTotalPower,
                CableTemperature = PreviewCableTemp,
                BoardTemperature = PreviewBoardTemp,
                FanIsOn = ComputePreviewFanState(fanControlType, fanOnTemp, PreviewCableTemp),
                ProtectionActive = ProtectionTesting,
                ChannelVoltages = (double[])PreviewChannelVoltages.Clone(),
                ChannelCurrents = (double[])PreviewChannelCurrents.Clone()
            };
        }

        private static double ParseDoubleOrDefault(string? text, double fallback)
        {
            double value;

            if (double.TryParse(text, out value))
            {
                return value;
            }

            return fallback;
        }

        private static bool ComputePreviewFanState(int fanControlType, double fanOnTemp, double cableTemp)
        {
            if (fanControlType == 1)
            {
                return false;
            }

            if (fanControlType == 2)
            {
                return true;
            }

            return cableTemp >= fanOnTemp;
        }

        //读取配置
        private void ReadConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if(WS12V2X6Ti != null)
            {
                StopTimerAndWaitSensorUpdateDone();

                WS12V2X6Ti.Write("readconfig?");

                string configString = WS12V2X6Ti.ReadString();

                Debug.WriteLine(configString);

                string[] values = configString.Split(',');


                if (values != null && values.Length == 10) {

                    //屏幕朝向
                    int orient = 0;
                    
                    int.TryParse(values[0], out orient);

                    if (orient == 1) { 
                        orient = 0;
                    }else if (orient == 3) {
                        orient = 1;
                    }

                    ScreenOrientSelectBox.SelectedIndex = orient;

                    //检流电阻阻值
                    double resistance = 1;
                    double.TryParse(values[1], out resistance);

                    if (resistance > 0) {
                        ShuntResistanceBox.Text = resistance.ToString();
                    }

                    //电流校正系数
                    double currentCorrection = 0.95;
                    double.TryParse(values[2], out currentCorrection);

                    if (currentCorrection > 0)
                    {
                        CurrentCorrectionBox.Text = currentCorrection.ToString();
                    }

                    //风扇控制模式
                    int fanControlMode = 0;
                    int.TryParse(values[3], out fanControlMode);

                    if (fanControlMode >= 0 &&
                        fanControlMode <3) {

                        FanControlTypeBox.SelectedIndex = fanControlMode;

                    }

                    //风扇启动温度
                    double fanOnTemp = 45;
                    double.TryParse(values[4],out fanOnTemp);

                    if (fanOnTemp > 0) { 
                        FanOnTempBox.Text = fanOnTemp.ToString();
                    }

                    //报警温度
                    double alarmTemp = 90;
                    double.TryParse(values[5], out alarmTemp);

                    if (alarmTemp > 0) { 
                        WarningTempBox.Text = alarmTemp.ToString();
                    }

                    //报警电流
                    double alarmCurrent = 12;
                    double.TryParse(values[6], out alarmCurrent);

                    if (alarmCurrent > 0) { 
                        WarningCurrentBox.Text = alarmCurrent.ToString();
                    }

                    //报警超时
                    double alarmTimeOut = 120;
                    double.TryParse(values[7],out alarmTimeOut);

                    if (alarmTimeOut > 0) {
                        WarningTimeOutBox.Text = alarmTimeOut.ToString();
                    }

                    //强制保护电流
                    double protectCurr = 15;
                    double.TryParse(values[8], out protectCurr);

                    if (protectCurr > 0)
                    {
                        ProtectCurrentBox.Text = protectCurr.ToString();
                    }

                    //强制保护温度
                    double protectTemp = 100;
                    double.TryParse(values[9], out protectTemp);

                    if (protectTemp > 0)
                    {
                        ProtectTempBox.Text = protectTemp.ToString();
                    }


                }

                UpdatePreviewWindow();

                UpdateTimer.Start();
            }
        }


        private void WriteConfigButton1_Click(object sender, RoutedEventArgs e)
        {
            if (WS12V2X6Ti != null)
            {
                StopTimerAndWaitSensorUpdateDone();


                string configString = "config:";

                //屏幕朝向
                int orient = ScreenOrientSelectBox.SelectedIndex;

                if (orient == 0)
                {
                    orient = 1;
                }
                else if (orient == 1) {

                    orient = 3;
                }

                configString += orient.ToString() + ",";

                //检流电阻阻值
                double resistance = 1;
                double.TryParse(ShuntResistanceBox.Text, out resistance);
                if (resistance <= 0.5) {
                    resistance = 0.5;
                }

                configString += resistance.ToString() + ",";

                //电流校正系数
                double currentCorrection = 0.95;
                double.TryParse(CurrentCorrectionBox.Text, out currentCorrection);
                if (currentCorrection <= 0)
                {
                    currentCorrection = 0.95;
                }

                configString += currentCorrection + ",";

                //风扇控制模式
                configString += FanControlTypeBox.SelectedIndex.ToString() + ",";

                //风扇启动温度
                double fanOnTemp = 45;
                double.TryParse(FanOnTempBox.Text, out fanOnTemp);
                configString += fanOnTemp.ToString()  + ",";

                //报警温度

                double alarmTemp = 90;
                double.TryParse(WarningTempBox.Text, out alarmTemp);
                configString += alarmTemp.ToString() + ",";


                //报警电流
                double alarmCurr = 12;
                double.TryParse(WarningCurrentBox.Text, out alarmCurr);
                configString += alarmCurr.ToString() + ",";


                //报警超时
                double alarmTimeOut = 120;
                double.TryParse(WarningTimeOutBox.Text, out alarmTimeOut);
                configString += alarmTimeOut.ToString() + ",";

                //强制保护电流
                double protectCurr = 15;
                double.TryParse(ProtectCurrentBox.Text, out protectCurr);
                configString += protectCurr.ToString() + ",";

                //强制保护温度
                double protectTemp = 100;
                double.TryParse(ProtectTempBox.Text,out protectTemp);
                configString += protectTemp.ToString();


                Debug.WriteLine(configString);

                WS12V2X6Ti.Write(configString);

                string configAck = WS12V2X6Ti.ReadString();
                if (configAck.Contains("CONFIG OK") == false)
                {
                    MessageBox.Show("MCU did not confirm config write. Response: " + configAck);
                    UpdateTimer.Start();
                    return;
                }

                System.Threading.Thread.Sleep(200);

                if (WriteDisplayCustomizationToDevice(showSuccessMessage: false) == false)
                {
                    UpdateTimer.Start();
                    return;
                }

                System.Threading.Thread.Sleep(500);

                WS12V2X6Ti.Write("reset");

                System.Threading.Thread.Sleep(2000);

                UpdateTimer.Start();
            }
        }

        private void WriteThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WS12V2X6Ti == null)
            {
                MessageBox.Show("Device not connected.");
                return;
            }

            StopTimerAndWaitSensorUpdateDone();
            if (WriteDisplayCustomizationToDevice(showSuccessMessage: true) == false)
            {
                UpdateTimer.Start();
                return;
            }

            System.Threading.Thread.Sleep(200);
            UpdateTimer.Start();
        }

        private bool WriteDisplayCustomizationToDevice(bool showSuccessMessage)
        {
            if (WS12V2X6Ti == null)
            {
                return false;
            }

            foreach (string displayEntry in PreviewCustomization.ToDeviceEntries())
            {
                WS12V2X6Ti.Write("displaycfg:" + displayEntry);
                string response = WS12V2X6Ti.ReadString();
                if (response.Contains("DISPLAYCFG OK") == false)
                {
                    MessageBox.Show("MCU did not accept display setting. Response: " + response + "\nThis usually means the MCU is still running old firmware.");
                    return false;
                }

                System.Threading.Thread.Sleep(20);
            }

            WS12V2X6Ti.Write("displaycfg:commit");
            string commitResponse = WS12V2X6Ti.ReadString();
            if (commitResponse.Contains("DISPLAYCFG OK") == false)
            {
                MessageBox.Show("MCU did not confirm display theme commit. Response: " + commitResponse);
                return false;
            }

            if (showSuccessMessage)
            {
                MessageBox.Show("Display theme written to MCU.");
            }

            return true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void WriteConfigString() {

            if (WS12V2X6Ti != null)
            {
            }
        }

        private void StopTimerAndWaitSensorUpdateDone() {

            UpdateTimer.Stop();

            while (UpdatingSensorData) {

                System.Threading.Thread.Sleep(10);
            }
        }

        //计时器刷新数据
        private void UpdateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (WS12V2X6Ti != null)
            {
                UpdatingSensorData = true;

                WS12V2X6Ti.Write("SENSORDATA?");

                string dataString = WS12V2X6Ti.ReadString();

                Debug.WriteLine(dataString);

                dataString = dataString.Replace( "\n", "");
                dataString = dataString.Replace("\r", "");

                string[] values = dataString.Split(',');

                if (values != null && values.Length == 15 && ChannelDataList.Count == 6) {


                    foreach(WFChannelData channelData in ChannelDataList) { 
                        int index = ChannelDataList.IndexOf(channelData);


                        double voltage = 0;
                        double.TryParse(values[index *2], out voltage);

                        channelData.voltage = voltage;
                        PreviewChannelVoltages[index] = voltage;

                        double current = 0;
                        double.TryParse(values[index * 2 + 1], out current);

                        channelData.current = current;
                        PreviewChannelCurrents[index] = current;

                        Debug.WriteLine(voltage + " , " + current);
                    }

                    double.TryParse(values[12], out PreviewTotalPower);
                    double.TryParse(values[13], out PreviewCableTemp);
                    double.TryParse(values[14], out PreviewBoardTemp);
                    

                    this.Dispatcher.Invoke(new Action(delegate
                    {

                        TotalPowerLabel.Content = values[12] + " W";
                        CableTempLabel.Content = values[13] + " ℃";
                        BoardTempLabel.Content = values[14] + " ℃";
                        UpdatePreviewWindow();
                    }
                    ));
                }


                UpdatingSensorData = false;
            }
        }

        private void ResetDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (WS12V2X6Ti != null)
            {
                StopTimerAndWaitSensorUpdateDone();

                WS12V2X6Ti.Write("reset");

                System.Threading.Thread.Sleep(2000);

                UpdateTimer.Start();
            }
        }




        //限制输入框
        private void tb_PreviewTextInput(object sender, TextCompositionEventArgs e)

        {

            Regex re = new Regex("[^0-9.]+");

            e.Handled = re.IsMatch(e.Text);

        }

        private void WarningTestButton_Click(object sender, RoutedEventArgs e)
        {

            if (WS12V2X6Ti != null)
            {
                StopTimerAndWaitSensorUpdateDone();

                if (WarningTesting == false)
                {
                    WS12V2X6Ti.Write("warningon");
                    WarningTesting = true;
                    WarningTestButton.Content = "Warning Test Off";
                }
                else 
                {
                    WS12V2X6Ti.Write("warningoff");
                    WarningTesting = false;
                    WarningTestButton.Content = "Warning Test On";
                }

                UpdatePreviewWindow();

                UpdateTimer.Start();
            }
        }

        private void ProtectionTestButton_Click(object sender, RoutedEventArgs e)
        {

            if (WS12V2X6Ti != null)
            {
                StopTimerAndWaitSensorUpdateDone();

                if (ProtectionTesting == false)
                {
                    WS12V2X6Ti.Write("protectionon");
                    ProtectionTesting = true;
                    ProtectionTestButton.Content = "Protection Test Off";
                }
                else 
                {
                    WS12V2X6Ti.Write("protectionoff");
                    ProtectionTesting = false;
                    ProtectionTestButton.Content = "Protection Test On";
                }

                UpdatePreviewWindow();
                
                UpdateTimer.Start();
            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            StopTimerAndWaitSensorUpdateDone();

            if (PreviewWindowInstance != null)
            {
                PreviewWindowInstance.Close();
            }
        }
    }

}