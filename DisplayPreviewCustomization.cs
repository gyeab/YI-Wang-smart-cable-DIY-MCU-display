using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace WingForce
{
    public sealed class DisplayPreviewCustomization
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public string TitleBackground { get; set; } = "#FF5A5A5A";

        public string TitleLeftColor { get; set; } = "#FF5CFF5C";

        public string TitleRightColor { get; set; } = "#FFFFFFFF";

        public string ChannelSectionBackground { get; set; } = "#FF202020";

        public string ChannelLabelColor { get; set; } = "#FFFFFFFF";

        public string VoltageTextColor { get; set; } = "#FFFFFFFF";

        public string VoltageWarningColor { get; set; } = "#FFFFA500";

        public string CurrentTextColor { get; set; } = "#FFFFFFFF";

        public string CurrentWarningColor { get; set; } = "#FFFFA500";

        public string CurrentAlertColor { get; set; } = "#FFFF0000";

        public string CurrentLowColor { get; set; } = "#FF1E8AFF";

        public string TemperatureSectionBackground { get; set; } = "#FF0A0A0A";

        public string TemperatureLabelColor { get; set; } = "#FFFFFFFF";

        public string CableLabelColor { get; set; } = string.Empty;

        public string BoardLabelColor { get; set; } = string.Empty;

        public string TemperatureUnitColor { get; set; } = string.Empty;

        public string TemperatureValueColor { get; set; } = "#FFFFFFFF";

        public string TemperatureWarningColor { get; set; } = "#FFFFA500";

        public string TemperatureAlertColor { get; set; } = "#FFFF0000";

        public string TotalPowerSectionBackground { get; set; } = "#FF0A0A0A";

        public string TotalPowerTextColor { get; set; } = "#FFFFFFFF";

        public string TotalPowerWarningColor { get; set; } = "#FFFFA500";

        public string TotalPowerAlertColor { get; set; } = "#FFFF0000";

        public string FanOnBackground { get; set; } = "#FF00C800";

        public string FanOffBackground { get; set; } = "#FF202020";

        public string FanTextOnColor { get; set; } = "#FF000000";

        public string FanTextOffColor { get; set; } = "#FFFFFFFF";

        public int DisplayFontId { get; set; } = 4;

        public string TitleLeftText { get; set; } = "WING STUDIO";

        public string TitleRightText { get; set; } = "12V-2X6 Ti";

        public string ProtectionLeftText { get; set; } = "PROTECTION";

        public string ProtectionRightText { get; set; } = "TRIGGERED";

        public string ChannelLabels { get; set; } = "CH-A,CH-B,CH-C,CH-D,CH-E,CH-F";

        public string CableLabelText { get; set; } = "Cable";

        public string BoardLabelText { get; set; } = "Board";

        public int CableLabelOffsetX { get; set; }

        public int CableLabelOffsetY { get; set; }

        public int CableUnitOffsetX { get; set; }

        public int CableUnitOffsetY { get; set; }

        public int CableValueOffsetX { get; set; }

        public int CableValueOffsetY { get; set; }

        public int BoardLabelOffsetX { get; set; }

        public int BoardLabelOffsetY { get; set; }

        public int BoardUnitOffsetX { get; set; }

        public int BoardUnitOffsetY { get; set; }

        public int BoardValueOffsetX { get; set; }

        public int BoardValueOffsetY { get; set; }

        public string TemperatureUnitText { get; set; } = "TMP (C)";

        public string TotalPowerLabelText { get; set; } = "Total Power :";

        public string FanLabelText { get; set; } = "FAN";

        public string FanOnText { get; set; } = "ON";

        public string FanOffText { get; set; } = "OFF";

        public DisplayPreviewCustomization Clone()
        {
            return new DisplayPreviewCustomization
            {
                TitleBackground = TitleBackground,
                TitleLeftColor = TitleLeftColor,
                TitleRightColor = TitleRightColor,
                ChannelSectionBackground = ChannelSectionBackground,
                ChannelLabelColor = ChannelLabelColor,
                VoltageTextColor = VoltageTextColor,
                VoltageWarningColor = VoltageWarningColor,
                CurrentTextColor = CurrentTextColor,
                CurrentWarningColor = CurrentWarningColor,
                CurrentAlertColor = CurrentAlertColor,
                CurrentLowColor = CurrentLowColor,
                TemperatureSectionBackground = TemperatureSectionBackground,
                TemperatureLabelColor = TemperatureLabelColor,
                CableLabelColor = CableLabelColor,
                BoardLabelColor = BoardLabelColor,
                TemperatureUnitColor = TemperatureUnitColor,
                TemperatureValueColor = TemperatureValueColor,
                TemperatureWarningColor = TemperatureWarningColor,
                TemperatureAlertColor = TemperatureAlertColor,
                TotalPowerSectionBackground = TotalPowerSectionBackground,
                TotalPowerTextColor = TotalPowerTextColor,
                TotalPowerWarningColor = TotalPowerWarningColor,
                TotalPowerAlertColor = TotalPowerAlertColor,
                FanOnBackground = FanOnBackground,
                FanOffBackground = FanOffBackground,
                FanTextOnColor = FanTextOnColor,
                FanTextOffColor = FanTextOffColor,
                DisplayFontId = DisplayFontId,
                TitleLeftText = TitleLeftText,
                TitleRightText = TitleRightText,
                ProtectionLeftText = ProtectionLeftText,
                ProtectionRightText = ProtectionRightText,
                ChannelLabels = ChannelLabels,
                CableLabelText = CableLabelText,
                BoardLabelText = BoardLabelText,
                CableLabelOffsetX = CableLabelOffsetX,
                CableLabelOffsetY = CableLabelOffsetY,
                CableUnitOffsetX = CableUnitOffsetX,
                CableUnitOffsetY = CableUnitOffsetY,
                CableValueOffsetX = CableValueOffsetX,
                CableValueOffsetY = CableValueOffsetY,
                BoardLabelOffsetX = BoardLabelOffsetX,
                BoardLabelOffsetY = BoardLabelOffsetY,
                BoardUnitOffsetX = BoardUnitOffsetX,
                BoardUnitOffsetY = BoardUnitOffsetY,
                BoardValueOffsetX = BoardValueOffsetX,
                BoardValueOffsetY = BoardValueOffsetY,
                TemperatureUnitText = TemperatureUnitText,
                TotalPowerLabelText = TotalPowerLabelText,
                FanLabelText = FanLabelText,
                FanOnText = FanOnText,
                FanOffText = FanOffText
            };
        }

        public static DisplayPreviewCustomization LoadOrDefault()
        {
            try
            {
                string filePath = GetStoragePath();
                if (!File.Exists(filePath))
                {
                    return new DisplayPreviewCustomization();
                }

                string json = File.ReadAllText(filePath);
                DisplayPreviewCustomization? customization = JsonSerializer.Deserialize<DisplayPreviewCustomization>(json);
                return customization ?? new DisplayPreviewCustomization();
            }
            catch
            {
                return new DisplayPreviewCustomization();
            }
        }

        public void Save()
        {
            string filePath = GetStoragePath();
            string? directoryPath = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, JsonSerializer.Serialize(this, JsonOptions));
        }

        public string ToDevicePayload()
        {
            StringBuilder builder = new StringBuilder();

            foreach (string entry in ToDeviceEntries())
            {
                if (builder.Length > 0)
                {
                    builder.Append(';');
                }

                builder.Append(entry);
            }

            return builder.ToString();
        }

        public IReadOnlyList<string> ToDeviceEntries()
        {
            List<string> entries = new List<string>();
            AddDeviceEntry(entries, "TitleBackground", TitleBackground);
            AddDeviceEntry(entries, "TitleLeftColor", TitleLeftColor);
            AddDeviceEntry(entries, "TitleRightColor", TitleRightColor);
            AddDeviceEntry(entries, "ChannelSectionBackground", ChannelSectionBackground);
            AddDeviceEntry(entries, "ChannelLabelColor", ChannelLabelColor);
            AddDeviceEntry(entries, "VoltageTextColor", VoltageTextColor);
            AddDeviceEntry(entries, "VoltageWarningColor", VoltageWarningColor);
            AddDeviceEntry(entries, "CurrentTextColor", CurrentTextColor);
            AddDeviceEntry(entries, "CurrentWarningColor", CurrentWarningColor);
            AddDeviceEntry(entries, "CurrentAlertColor", CurrentAlertColor);
            AddDeviceEntry(entries, "CurrentLowColor", CurrentLowColor);
            AddDeviceEntry(entries, "TemperatureSectionBackground", TemperatureSectionBackground);
            AddDeviceEntry(entries, "TemperatureLabelColor", TemperatureLabelColor);
            AddDeviceEntry(entries, "CableLabelColor", CableLabelColor);
            AddDeviceEntry(entries, "BoardLabelColor", BoardLabelColor);
            AddDeviceEntry(entries, "TemperatureUnitColor", TemperatureUnitColor);
            AddDeviceEntry(entries, "TemperatureValueColor", TemperatureValueColor);
            AddDeviceEntry(entries, "TemperatureWarningColor", TemperatureWarningColor);
            AddDeviceEntry(entries, "TemperatureAlertColor", TemperatureAlertColor);
            AddDeviceEntry(entries, "TotalPowerSectionBackground", TotalPowerSectionBackground);
            AddDeviceEntry(entries, "TotalPowerTextColor", TotalPowerTextColor);
            AddDeviceEntry(entries, "TotalPowerWarningColor", TotalPowerWarningColor);
            AddDeviceEntry(entries, "TotalPowerAlertColor", TotalPowerAlertColor);
            AddDeviceEntry(entries, "FanOnBackground", FanOnBackground);
            AddDeviceEntry(entries, "FanOffBackground", FanOffBackground);
            AddDeviceEntry(entries, "FanTextOnColor", FanTextOnColor);
            AddDeviceEntry(entries, "FanTextOffColor", FanTextOffColor);
            AddDeviceEntry(entries, "DisplayFontId", DisplayFontId.ToString());
            AddDeviceEntry(entries, "TitleLeftText", TitleLeftText);
            AddDeviceEntry(entries, "TitleRightText", TitleRightText);
            AddDeviceEntry(entries, "ProtectionLeftText", ProtectionLeftText);
            AddDeviceEntry(entries, "ProtectionRightText", ProtectionRightText);
            AddDeviceEntry(entries, "ChannelLabels", ChannelLabels);
            AddDeviceEntry(entries, "CableLabelText", CableLabelText);
            AddDeviceEntry(entries, "BoardLabelText", BoardLabelText);
            AddDeviceEntry(entries, "CableLabelOffsetX", CableLabelOffsetX.ToString());
            AddDeviceEntry(entries, "CableLabelOffsetY", CableLabelOffsetY.ToString());
            AddDeviceEntry(entries, "CableUnitOffsetX", CableUnitOffsetX.ToString());
            AddDeviceEntry(entries, "CableUnitOffsetY", CableUnitOffsetY.ToString());
            AddDeviceEntry(entries, "CableValueOffsetX", CableValueOffsetX.ToString());
            AddDeviceEntry(entries, "CableValueOffsetY", CableValueOffsetY.ToString());
            AddDeviceEntry(entries, "BoardLabelOffsetX", BoardLabelOffsetX.ToString());
            AddDeviceEntry(entries, "BoardLabelOffsetY", BoardLabelOffsetY.ToString());
            AddDeviceEntry(entries, "BoardUnitOffsetX", BoardUnitOffsetX.ToString());
            AddDeviceEntry(entries, "BoardUnitOffsetY", BoardUnitOffsetY.ToString());
            AddDeviceEntry(entries, "BoardValueOffsetX", BoardValueOffsetX.ToString());
            AddDeviceEntry(entries, "BoardValueOffsetY", BoardValueOffsetY.ToString());
            AddDeviceEntry(entries, "TemperatureUnitText", TemperatureUnitText);
            AddDeviceEntry(entries, "TotalPowerLabelText", TotalPowerLabelText);
            AddDeviceEntry(entries, "FanLabelText", FanLabelText);
            AddDeviceEntry(entries, "FanOnText", FanOnText);
            AddDeviceEntry(entries, "FanOffText", FanOffText);
            return entries;
        }

        private static void AppendDeviceValue(StringBuilder builder, string key, string value)
        {
            if (builder.Length > 0)
            {
                builder.Append(';');
            }

            builder.Append(key);
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value ?? string.Empty));
        }

        private static void AddDeviceEntry(List<string> entries, string key, string value)
        {
            StringBuilder builder = new StringBuilder();
            AppendDeviceValue(builder, key, value);
            entries.Add(builder.ToString());
        }

        private static string GetStoragePath()
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(basePath, "WingForce", "display-preview.json");
        }
    }
}