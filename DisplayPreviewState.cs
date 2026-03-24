namespace WingForce
{
    public sealed class DisplayPreviewState
    {
        public int ScreenOrientationIndex { get; set; }

        public double AlarmTemperature { get; set; }

        public double AlarmCurrent { get; set; }

        public double FanOnTemperature { get; set; }

        public int FanControlType { get; set; }

        public double TotalPower { get; set; }

        public double CableTemperature { get; set; }

        public double BoardTemperature { get; set; }

        public bool FanIsOn { get; set; }

        public bool ProtectionActive { get; set; }

        public double[] ChannelVoltages { get; set; } = new double[6];

        public double[] ChannelCurrents { get; set; } = new double[6];
    }
}