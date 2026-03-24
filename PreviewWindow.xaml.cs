using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WingForce
{
    public partial class PreviewWindow : Window
    {
        private readonly Border[] _channelIndicators;
        private readonly TextBlock[] _channelLabels;
        private readonly TextBlock[] _channelVoltages;
        private readonly TextBlock[] _channelCurrents;
        private const double TemperatureTextBaseLeft = 19;
        private const double TemperatureLabelWidth = 68;
        private const double TemperatureValueWidth = 84;
        private const double CableLabelBaseTop = 17;
        private const double CableUnitBaseTop = 45;
        private const double CableValueBaseTop = 73;
        private const double BoardLabelBaseTop = 111;
        private const double BoardUnitBaseTop = 139;
        private const double BoardValueBaseTop = 167;
        private DisplayPreviewState _currentState = new DisplayPreviewState();
        private DisplayPreviewCustomization _customization;

        private static readonly Brush AccentGreenBrush = CreateBrush(0, 200, 0);
        private static readonly Brush AccentBlueBrush = CreateBrush(30, 138, 255);
        private static readonly Brush AccentOrangeBrush = new SolidColorBrush(Colors.Orange);

        public event Action<DisplayPreviewCustomization>? CustomizationChanged;

        public PreviewWindow(DisplayPreviewCustomization customization)
        {
            InitializeComponent();

            _customization = customization.Clone();

            _channelIndicators = new[]
            {
                ChannelIndicator0,
                ChannelIndicator1,
                ChannelIndicator2,
                ChannelIndicator3,
                ChannelIndicator4,
                ChannelIndicator5
            };

            _channelLabels = new[]
            {
                ChannelLabel0,
                ChannelLabel1,
                ChannelLabel2,
                ChannelLabel3,
                ChannelLabel4,
                ChannelLabel5
            };

            _channelVoltages = new[]
            {
                ChannelVoltage0,
                ChannelVoltage1,
                ChannelVoltage2,
                ChannelVoltage3,
                ChannelVoltage4,
                ChannelVoltage5
            };

            _channelCurrents = new[]
            {
                ChannelCurrent0,
                ChannelCurrent1,
                ChannelCurrent2,
                ChannelCurrent3,
                ChannelCurrent4,
                ChannelCurrent5
            };

            LoadCustomizationInputs();
            RenderPreview();
        }

        public void UpdatePreview(DisplayPreviewState state)
        {
            _currentState = state;
            RenderPreview();
        }

        private void RenderPreview()
        {
            DisplayRotationTransform.Angle = _currentState.ScreenOrientationIndex == 0 ? 0 : 180;

            TitleBar.Background = ParseBrush(_customization.TitleBackground, Brushes.DimGray);
            ChannelSection.Background = ParseBrush(_customization.ChannelSectionBackground, CreateBrush(32, 32, 32));
            TemperatureSection.Background = ParseBrush(_customization.TemperatureSectionBackground, CreateBrush(10, 10, 10));
            TotalPowerSection.Background = ParseBrush(_customization.TotalPowerSectionBackground, CreateBrush(10, 10, 10));

            Brush titleLeftBrush = ParseBrush(_customization.TitleLeftColor, Brushes.LightGreen);
            Brush titleRightBrush = ParseBrush(_customization.TitleRightColor, Brushes.White);
            Brush channelLabelBrush = ParseBrush(_customization.ChannelLabelColor, Brushes.White);
            Brush temperatureLabelBrush = ParseBrush(_customization.TemperatureLabelColor, Brushes.White);
            Brush cableLabelBrush = ParseOptionalBrush(_customization.CableLabelColor, temperatureLabelBrush);
            Brush boardLabelBrush = ParseOptionalBrush(_customization.BoardLabelColor, temperatureLabelBrush);
            Brush temperatureUnitBrush = ParseOptionalBrush(_customization.TemperatureUnitColor, temperatureLabelBrush);
            Brush defaultVoltageBrush = ParseBrush(_customization.VoltageTextColor, Brushes.White);
            Brush voltageWarningBrush = ParseBrush(_customization.VoltageWarningColor, AccentOrangeBrush);
            Brush defaultCurrentBrush = ParseBrush(_customization.CurrentTextColor, Brushes.White);
            Brush currentWarningBrush = ParseBrush(_customization.CurrentWarningColor, AccentOrangeBrush);
            Brush currentAlertBrush = ParseBrush(_customization.CurrentAlertColor, Brushes.Red);
            Brush currentLowBrush = ParseBrush(_customization.CurrentLowColor, AccentBlueBrush);
            Brush defaultTemperatureValueBrush = ParseBrush(_customization.TemperatureValueColor, Brushes.White);
            Brush temperatureWarningBrush = ParseBrush(_customization.TemperatureWarningColor, AccentOrangeBrush);
            Brush temperatureAlertBrush = ParseBrush(_customization.TemperatureAlertColor, Brushes.Red);
            Brush defaultTotalPowerBrush = ParseBrush(_customization.TotalPowerTextColor, Brushes.White);
            Brush totalPowerWarningBrush = ParseBrush(_customization.TotalPowerWarningColor, AccentOrangeBrush);
            Brush totalPowerAlertBrush = ParseBrush(_customization.TotalPowerAlertColor, Brushes.Red);
            Brush fanOnBackground = ParseBrush(_customization.FanOnBackground, AccentGreenBrush);
            Brush fanOffBackground = ParseBrush(_customization.FanOffBackground, CreateBrush(32, 32, 32));
            Brush fanOnTextBrush = ParseBrush(_customization.FanTextOnColor, Brushes.Black);
            Brush fanOffTextBrush = ParseBrush(_customization.FanTextOffColor, Brushes.White);
            string[] channelLabels = ParseChannelLabels(_customization.ChannelLabels);
            int displayFontId = NormalizeDisplayFontId(_customization.DisplayFontId);
            FontFamily previewFontFamily = GetPreviewFontFamily(displayFontId);

            ApplyDisplayFont(previewFontFamily);

            if (_currentState.ProtectionActive)
            {
                TitleBar.Background = Brushes.Red;
                TitleLeftText.Foreground = Brushes.White;
                TitleLeftText.Text = _customization.ProtectionLeftText;
                TitleRightText.Text = _customization.ProtectionRightText;
                TitleRightText.Foreground = Brushes.White;
            }
            else
            {
                TitleLeftText.Foreground = titleLeftBrush;
                TitleLeftText.Text = _customization.TitleLeftText;
                TitleRightText.Text = _customization.TitleRightText;
                TitleRightText.Foreground = titleRightBrush;
            }

            CableLabelText.Text = _customization.CableLabelText;
            BoardLabelText.Text = _customization.BoardLabelText;
            CableTempUnitText.Text = _customization.TemperatureUnitText;
            BoardTempUnitText.Text = _customization.TemperatureUnitText;
            CableLabelText.Foreground = cableLabelBrush;
            BoardLabelText.Foreground = boardLabelBrush;
            CableTempUnitText.Foreground = temperatureUnitBrush;
            BoardTempUnitText.Foreground = temperatureUnitBrush;
            SetTemperatureTextPosition(CableLabelText, _customization.CableLabelOffsetX, _customization.CableLabelOffsetY, CableLabelBaseTop, TemperatureLabelWidth);
            SetTemperatureTextPosition(CableTempUnitText, _customization.CableUnitOffsetX, _customization.CableUnitOffsetY, CableUnitBaseTop, TemperatureLabelWidth);
            SetTemperatureTextPosition(CableTempValue, _customization.CableValueOffsetX, _customization.CableValueOffsetY, CableValueBaseTop, TemperatureValueWidth);
            SetTemperatureTextPosition(BoardLabelText, _customization.BoardLabelOffsetX, _customization.BoardLabelOffsetY, BoardLabelBaseTop, TemperatureLabelWidth);
            SetTemperatureTextPosition(BoardTempUnitText, _customization.BoardUnitOffsetX, _customization.BoardUnitOffsetY, BoardUnitBaseTop, TemperatureLabelWidth);
            SetTemperatureTextPosition(BoardTempValue, _customization.BoardValueOffsetX, _customization.BoardValueOffsetY, BoardValueBaseTop, TemperatureValueWidth);

            FanLabelText.Text = _customization.FanLabelText;

            double averageCurrent = _currentState.ChannelCurrents.Length == 0 ? 0 : _currentState.ChannelCurrents.Average();

            for (int index = 0; index < 6; index++)
            {
                double voltage = index < _currentState.ChannelVoltages.Length ? _currentState.ChannelVoltages[index] : 0;
                double current = index < _currentState.ChannelCurrents.Length ? _currentState.ChannelCurrents[index] : 0;
                CurrentDisplayState currentState = GetCurrentDisplayState(current, averageCurrent, _currentState.AlarmCurrent);
                Brush currentBrush = GetCurrentBrush(currentState, defaultCurrentBrush, currentWarningBrush, currentAlertBrush, currentLowBrush);

                _channelLabels[index].Text = channelLabels[index] + " :";
                _channelLabels[index].Foreground = channelLabelBrush;

                _channelVoltages[index].Text = FormatValue(voltage, "V");
                _channelVoltages[index].Foreground = GetVoltageBrush(voltage, defaultVoltageBrush, voltageWarningBrush);

                _channelCurrents[index].Text = FormatValue(current, "A");
                _channelCurrents[index].Foreground = currentBrush;

                _channelIndicators[index].Background = currentState == CurrentDisplayState.Normal ? AccentGreenBrush : currentBrush;
            }

            TotalPowerValue.Text = $"{_customization.TotalPowerLabelText} {_currentState.TotalPower:F2}W";
            TotalPowerValue.Foreground = GetPowerBrush(_currentState.TotalPower, defaultTotalPowerBrush, totalPowerWarningBrush, totalPowerAlertBrush);

            CableTempValue.Text = _currentState.CableTemperature.ToString("F2");
            CableTempValue.Foreground = GetTemperatureBrush(_currentState.CableTemperature, _currentState.AlarmTemperature, defaultTemperatureValueBrush, temperatureWarningBrush, temperatureAlertBrush);

            BoardTempValue.Text = _currentState.BoardTemperature.ToString("F2");
            BoardTempValue.Foreground = GetTemperatureBrush(_currentState.BoardTemperature, _currentState.AlarmTemperature, defaultTemperatureValueBrush, temperatureWarningBrush, temperatureAlertBrush);

            FanStatusPanel.Background = _currentState.FanIsOn ? fanOnBackground : fanOffBackground;
            FanLabelText.Foreground = _currentState.FanIsOn ? fanOnTextBrush : fanOffTextBrush;
            FanStatusText.Text = _currentState.FanIsOn ? _customization.FanOnText : _customization.FanOffText;
            FanStatusText.Foreground = _currentState.FanIsOn ? fanOnTextBrush : fanOffTextBrush;
        }

        private void ApplyCustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _customization = new DisplayPreviewCustomization
                {
                    TitleBackground = TitleBackgroundBox.Text,
                    TitleLeftColor = TitleLeftColorBox.Text,
                    TitleRightColor = TitleRightColorBox.Text,
                    ChannelSectionBackground = ChannelBackgroundBox.Text,
                    ChannelLabelColor = ChannelLabelColorBox.Text,
                    VoltageTextColor = VoltageTextColorBox.Text,
                    VoltageWarningColor = VoltageWarningColorBox.Text,
                    CurrentTextColor = CurrentTextColorBox.Text,
                    CurrentWarningColor = CurrentWarningColorBox.Text,
                    CurrentAlertColor = CurrentAlertColorBox.Text,
                    CurrentLowColor = CurrentLowColorBox.Text,
                    TemperatureSectionBackground = TemperatureBackgroundBox.Text,
                    TemperatureLabelColor = TemperatureLabelColorBox.Text,
                    CableLabelColor = CableLabelColorBox.Text,
                    BoardLabelColor = BoardLabelColorBox.Text,
                    TemperatureUnitColor = TemperatureUnitColorBox.Text,
                    TemperatureValueColor = TemperatureValueColorBox.Text,
                    TemperatureWarningColor = TemperatureWarningColorBox.Text,
                    TemperatureAlertColor = TemperatureAlertColorBox.Text,
                    TotalPowerSectionBackground = TotalPowerBackgroundBox.Text,
                    TotalPowerTextColor = TotalPowerTextColorBox.Text,
                    TotalPowerWarningColor = TotalPowerWarningColorBox.Text,
                    TotalPowerAlertColor = TotalPowerAlertColorBox.Text,
                    FanOnBackground = FanOnBackgroundBox.Text,
                    FanOffBackground = FanOffBackgroundBox.Text,
                    FanTextOnColor = FanOnTextColorBox.Text,
                    FanTextOffColor = FanOffTextColorBox.Text,
                    DisplayFontId = ParseDisplayFontId(DisplayFontIdBox.Text),
                    TitleLeftText = TitleLeftTextBox.Text,
                    TitleRightText = TitleRightTextBox.Text,
                    ProtectionLeftText = ProtectionLeftTextBox.Text,
                    ProtectionRightText = ProtectionRightTextBox.Text,
                    ChannelLabels = ChannelLabelsBox.Text,
                    CableLabelText = CableLabelBox.Text,
                    BoardLabelText = BoardLabelBox.Text,
                    CableLabelOffsetX = ParseOffset(CableLabelOffsetXBox.Text, "Cable label offset X"),
                    CableLabelOffsetY = ParseOffset(CableLabelOffsetYBox.Text, "Cable label offset Y"),
                    CableUnitOffsetX = ParseOffset(CableUnitOffsetXBox.Text, "Cable TMP offset X"),
                    CableUnitOffsetY = ParseOffset(CableUnitOffsetYBox.Text, "Cable TMP offset Y"),
                    CableValueOffsetX = ParseOffset(CableValueOffsetXBox.Text, "Cable value offset X"),
                    CableValueOffsetY = ParseOffset(CableValueOffsetYBox.Text, "Cable value offset Y"),
                    BoardLabelOffsetX = ParseOffset(BoardLabelOffsetXBox.Text, "Board label offset X"),
                    BoardLabelOffsetY = ParseOffset(BoardLabelOffsetYBox.Text, "Board label offset Y"),
                    BoardUnitOffsetX = ParseOffset(BoardUnitOffsetXBox.Text, "Board TMP offset X"),
                    BoardUnitOffsetY = ParseOffset(BoardUnitOffsetYBox.Text, "Board TMP offset Y"),
                    BoardValueOffsetX = ParseOffset(BoardValueOffsetXBox.Text, "Board value offset X"),
                    BoardValueOffsetY = ParseOffset(BoardValueOffsetYBox.Text, "Board value offset Y"),
                    TemperatureUnitText = TemperatureUnitBox.Text,
                    TotalPowerLabelText = TotalPowerLabelBox.Text,
                    FanLabelText = FanLabelBox.Text,
                    FanOnText = FanOnTextBox.Text,
                    FanOffText = FanOffTextBox.Text
                };

                ValidateCustomization(_customization);
                CustomizationMessageText.Visibility = Visibility.Collapsed;
                RenderPreview();
                CustomizationChanged?.Invoke(_customization.Clone());
            }
            catch (Exception ex)
            {
                CustomizationMessageText.Text = ex.Message;
                CustomizationMessageText.Visibility = Visibility.Visible;
            }
        }

        private void ResetCustomizationButton_Click(object sender, RoutedEventArgs e)
        {
            _customization = new DisplayPreviewCustomization();
            LoadCustomizationInputs();
            CustomizationMessageText.Visibility = Visibility.Collapsed;
            RenderPreview();
            CustomizationChanged?.Invoke(_customization.Clone());
        }

        private void LoadCustomizationInputs()
        {
            TitleBackgroundBox.Text = _customization.TitleBackground;
            TitleLeftColorBox.Text = _customization.TitleLeftColor;
            TitleRightColorBox.Text = _customization.TitleRightColor;
            ChannelBackgroundBox.Text = _customization.ChannelSectionBackground;
            ChannelLabelColorBox.Text = _customization.ChannelLabelColor;
            VoltageTextColorBox.Text = _customization.VoltageTextColor;
            VoltageWarningColorBox.Text = _customization.VoltageWarningColor;
            CurrentTextColorBox.Text = _customization.CurrentTextColor;
            CurrentWarningColorBox.Text = _customization.CurrentWarningColor;
            CurrentAlertColorBox.Text = _customization.CurrentAlertColor;
            CurrentLowColorBox.Text = _customization.CurrentLowColor;
            TemperatureBackgroundBox.Text = _customization.TemperatureSectionBackground;
            TemperatureLabelColorBox.Text = _customization.TemperatureLabelColor;
            CableLabelColorBox.Text = _customization.CableLabelColor;
            BoardLabelColorBox.Text = _customization.BoardLabelColor;
            TemperatureUnitColorBox.Text = _customization.TemperatureUnitColor;
            TemperatureValueColorBox.Text = _customization.TemperatureValueColor;
            TemperatureWarningColorBox.Text = _customization.TemperatureWarningColor;
            TemperatureAlertColorBox.Text = _customization.TemperatureAlertColor;
            TotalPowerBackgroundBox.Text = _customization.TotalPowerSectionBackground;
            TotalPowerTextColorBox.Text = _customization.TotalPowerTextColor;
            TotalPowerWarningColorBox.Text = _customization.TotalPowerWarningColor;
            TotalPowerAlertColorBox.Text = _customization.TotalPowerAlertColor;
            FanOnBackgroundBox.Text = _customization.FanOnBackground;
            FanOffBackgroundBox.Text = _customization.FanOffBackground;
            FanOnTextColorBox.Text = _customization.FanTextOnColor;
            FanOffTextColorBox.Text = _customization.FanTextOffColor;
            DisplayFontIdBox.Text = _customization.DisplayFontId.ToString();
            TitleLeftTextBox.Text = _customization.TitleLeftText;
            TitleRightTextBox.Text = _customization.TitleRightText;
            ProtectionLeftTextBox.Text = _customization.ProtectionLeftText;
            ProtectionRightTextBox.Text = _customization.ProtectionRightText;
            ChannelLabelsBox.Text = _customization.ChannelLabels;
            CableLabelBox.Text = _customization.CableLabelText;
            BoardLabelBox.Text = _customization.BoardLabelText;
            CableLabelOffsetXBox.Text = _customization.CableLabelOffsetX.ToString();
            CableLabelOffsetYBox.Text = _customization.CableLabelOffsetY.ToString();
            CableUnitOffsetXBox.Text = _customization.CableUnitOffsetX.ToString();
            CableUnitOffsetYBox.Text = _customization.CableUnitOffsetY.ToString();
            CableValueOffsetXBox.Text = _customization.CableValueOffsetX.ToString();
            CableValueOffsetYBox.Text = _customization.CableValueOffsetY.ToString();
            BoardLabelOffsetXBox.Text = _customization.BoardLabelOffsetX.ToString();
            BoardLabelOffsetYBox.Text = _customization.BoardLabelOffsetY.ToString();
            BoardUnitOffsetXBox.Text = _customization.BoardUnitOffsetX.ToString();
            BoardUnitOffsetYBox.Text = _customization.BoardUnitOffsetY.ToString();
            BoardValueOffsetXBox.Text = _customization.BoardValueOffsetX.ToString();
            BoardValueOffsetYBox.Text = _customization.BoardValueOffsetY.ToString();
            TemperatureUnitBox.Text = _customization.TemperatureUnitText;
            TotalPowerLabelBox.Text = _customization.TotalPowerLabelText;
            FanLabelBox.Text = _customization.FanLabelText;
            FanOnTextBox.Text = _customization.FanOnText;
            FanOffTextBox.Text = _customization.FanOffText;
        }

        private static void ValidateCustomization(DisplayPreviewCustomization customization)
        {
            ParseBrush(customization.TitleBackground, Brushes.DimGray);
            ParseBrush(customization.TitleLeftColor, Brushes.LightGreen);
            ParseBrush(customization.TitleRightColor, Brushes.White);
            ParseBrush(customization.ChannelSectionBackground, CreateBrush(32, 32, 32));
            ParseBrush(customization.ChannelLabelColor, Brushes.White);
            ParseBrush(customization.VoltageTextColor, Brushes.White);
            ParseBrush(customization.VoltageWarningColor, AccentOrangeBrush);
            ParseBrush(customization.CurrentTextColor, Brushes.White);
            ParseBrush(customization.CurrentWarningColor, AccentOrangeBrush);
            ParseBrush(customization.CurrentAlertColor, Brushes.Red);
            ParseBrush(customization.CurrentLowColor, AccentBlueBrush);
            ParseBrush(customization.TemperatureSectionBackground, CreateBrush(10, 10, 10));
            ParseBrush(customization.TemperatureLabelColor, Brushes.White);
            ParseOptionalBrush(customization.CableLabelColor, Brushes.White);
            ParseOptionalBrush(customization.BoardLabelColor, Brushes.White);
            ParseOptionalBrush(customization.TemperatureUnitColor, Brushes.White);
            ParseBrush(customization.TemperatureValueColor, Brushes.White);
            ParseBrush(customization.TemperatureWarningColor, AccentOrangeBrush);
            ParseBrush(customization.TemperatureAlertColor, Brushes.Red);
            ParseBrush(customization.TotalPowerSectionBackground, CreateBrush(10, 10, 10));
            ParseBrush(customization.TotalPowerTextColor, Brushes.White);
            ParseBrush(customization.TotalPowerWarningColor, AccentOrangeBrush);
            ParseBrush(customization.TotalPowerAlertColor, Brushes.Red);
            ParseBrush(customization.FanOnBackground, AccentGreenBrush);
            ParseBrush(customization.FanOffBackground, CreateBrush(32, 32, 32));
            ParseBrush(customization.FanTextOnColor, Brushes.Black);
            ParseBrush(customization.FanTextOffColor, Brushes.White);
            NormalizeDisplayFontId(customization.DisplayFontId);

            if (SplitChannelLabels(customization.ChannelLabels).Length != 6)
            {
                throw new InvalidOperationException("Channel labels must contain 6 comma-separated items.");
            }
        }

        private enum CurrentDisplayState
        {
            Normal,
            Warning,
            Alert,
            Low
        }

        private static Brush GetVoltageBrush(double voltage, Brush normalBrush, Brush warningBrush)
        {
            if (voltage < 11.2 || voltage > 12.7)
            {
                return warningBrush;
            }

            return normalBrush;
        }

        private static CurrentDisplayState GetCurrentDisplayState(double current, double averageCurrent, double alarmCurrent)
        {
            if (averageCurrent > 0.1 && current < averageCurrent * 0.7)
            {
                return CurrentDisplayState.Low;
            }

            if (current >= alarmCurrent)
            {
                return CurrentDisplayState.Alert;
            }

            if (current >= Math.Max(alarmCurrent - 3, 9))
            {
                return CurrentDisplayState.Warning;
            }

            return CurrentDisplayState.Normal;
        }

        private static Brush GetCurrentBrush(CurrentDisplayState state, Brush normalBrush, Brush warningBrush, Brush alertBrush, Brush lowBrush)
        {
            return state switch
            {
                CurrentDisplayState.Warning => warningBrush,
                CurrentDisplayState.Alert => alertBrush,
                CurrentDisplayState.Low => lowBrush,
                _ => normalBrush
            };
        }

        private static Brush GetPowerBrush(double totalPower, Brush normalBrush, Brush warningBrush, Brush alertBrush)
        {
            if (totalPower >= 675)
            {
                return alertBrush;
            }

            if (totalPower >= 600)
            {
                return warningBrush;
            }

            return normalBrush;
        }

        private static Brush GetTemperatureBrush(double temperature, double alarmTemperature, Brush normalBrush, Brush warningBrush, Brush alertBrush)
        {
            if (temperature >= alarmTemperature)
            {
                return alertBrush;
            }

            if (temperature >= alarmTemperature - 10)
            {
                return warningBrush;
            }

            return normalBrush;
        }

        private static string FormatValue(double value, string unit)
        {
            return $" {value,5:F2}{unit}";
        }

        private static int ParseOffset(string value, string fieldName)
        {
            if (int.TryParse(value, out int offset))
            {
                return offset;
            }

            throw new InvalidOperationException($"{fieldName} must be a whole number.");
        }

        private static int ParseDisplayFontId(string value)
        {
            if (int.TryParse(value, out int fontId))
            {
                return NormalizeDisplayFontId(fontId);
            }

            throw new InvalidOperationException("MCU font ID must be one of: 1, 2, 4, 6, 7, 8.");
        }

        private static int NormalizeDisplayFontId(int fontId)
        {
            return fontId switch
            {
                1 => 1,
                2 => 2,
                4 => 4,
                6 => 6,
                7 => 7,
                8 => 8,
                _ => throw new InvalidOperationException("MCU font ID must be one of: 1, 2, 4, 6, 7, 8.")
            };
        }

        private void ApplyDisplayFont(FontFamily fontFamily)
        {
            TitleLeftText.FontFamily = fontFamily;
            TitleLeftText.FontWeight = FontWeights.Normal;
            TitleRightText.FontFamily = fontFamily;
            TitleRightText.FontWeight = FontWeights.Normal;
            CableLabelText.FontFamily = fontFamily;
            CableTempUnitText.FontFamily = fontFamily;
            BoardLabelText.FontFamily = fontFamily;
            BoardTempUnitText.FontFamily = fontFamily;
            TotalPowerValue.FontFamily = fontFamily;
            FanLabelText.FontFamily = fontFamily;
            FanLabelText.FontWeight = FontWeights.Normal;
            FanStatusText.FontFamily = fontFamily;
            FanStatusText.FontWeight = FontWeights.Normal;

            foreach (TextBlock label in _channelLabels)
            {
                label.FontFamily = fontFamily;
            }

            foreach (TextBlock voltage in _channelVoltages)
            {
                voltage.FontFamily = fontFamily;
            }

            foreach (TextBlock current in _channelCurrents)
            {
                current.FontFamily = fontFamily;
            }

            CableTempValue.FontFamily = fontFamily;
            BoardTempValue.FontFamily = fontFamily;
        }

        private static FontFamily GetPreviewFontFamily(int fontId)
        {
            return fontId switch
            {
                7 => new FontFamily("Consolas"),
                8 => new FontFamily("Times New Roman"),
                6 => new FontFamily("Georgia"),
                _ => new FontFamily("Segoe UI")
            };
        }

        private static string[] ParseChannelLabels(string value)
        {
            string[] labels = SplitChannelLabels(value);

            if (labels.Length == 6)
            {
                return labels;
            }

            return new[] { "CH-A", "CH-B", "CH-C", "CH-D", "CH-E", "CH-F" };
        }

        private static string[] SplitChannelLabels(string value)
        {
            return value
                .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => item.Length > 0)
                .ToArray();
        }

        private static Brush ParseBrush(string value, Brush fallback)
        {
            try
            {
                object? converted = ColorConverter.ConvertFromString(value);
                if (converted is Color color)
                {
                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Freeze();
                    return brush;
                }
            }
            catch
            {
                throw new InvalidOperationException($"Invalid color value: {value}");
            }

            return fallback;
        }

        private static Brush ParseOptionalBrush(string value, Brush fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return ParseBrush(value, fallback);
        }

        private static void SetTemperatureTextPosition(FrameworkElement element, int offsetX, int offsetY, double baseTop, double width)
        {
            element.Width = width;
            Canvas.SetLeft(element, TemperatureTextBaseLeft + offsetX);
            Canvas.SetTop(element, baseTop + offsetY);
        }

        private static SolidColorBrush CreateBrush(byte red, byte green, byte blue)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}