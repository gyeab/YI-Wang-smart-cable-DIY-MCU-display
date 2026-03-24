// version: 1.0.0


#include <Arduino.h>
#include <TFT_eSPI.h>
#include <Wire.h>
#include <LinkedList.h>
#include <driver/ledc.h>
#include <string.h>

#include "Adafruit_INA3221.h"
#include <Adafruit_TMP117.h>
#include <Adafruit_Sensor.h>

#include <Preferences.h>

#define DEVICE_NAME "WINGSTUDIO 12V-2X6 Ti V1.0\n"

#define I2C_SDA 11   // 自定义SDA引脚
#define I2C_SCL 10   // 自定义SCL引脚
#define I2C_FREQ 100000

#define INA3221_1_ADDR 0x40
#define INA3221_2_ADDR 0x41

#define CONNECTOR_TMP117_ADDR 0x49
#define CONTROLBOARD_TMP117_ADDR 0x48

#define FAN_CONTROL_PIN 1 //风扇控制引脚
#define SENSE_CONTROL_PIN 8 //sense控制引脚

#define BUZZER_PIN 21 //无源蜂鸣器引脚

#define TMP_TEXT_POSITION_X 367 //温度显示位置X

#define CONFIG_VALUE_COUNT 10 //配置文件数量
#define MAX_CHANNEL_COUNT 6

//配置文件
Preferences Config;

//配置变量
bool ProfileExist = false; //配置文件是否存在

int ScreenRotation = 3; //屏幕旋转方向
int DisplayFontId = 4; //显示字体编号

float ShuntResistance = 2; //检流电阻阻值

float CurrentCorrectionRate = 0.95; //电流校正系数

int FanControlType = 0; //风扇控制类型 0=自动 1=保持关闭 2=保持开启

float FanOnTMP = 45.0; //风扇开启温度 

float AlarmTMP = 90.0; //报警温度

float AlarmCurrent = 12; //报警电流

float AlarmTimeOut = 120; //报警超时时间 -1=永不超时

float ProtectionCurrent = 15; //强制保护电流

float ProtectionTMP = 100.0; //强制保护温度


bool FanIsOn = true; //风扇是否开启

//循环变量
int32_t RefreshInterval = 100; //刷新间隔
bool WarningTest = false; //报警测试标志
bool WarningTriggered = false; //报警触发标志
bool ProtectionTriggered = false; //保护触发标志
int32_t WarningCount = 0; //报警计数器
int32_t WarningTimeOut = 0; //报警超时计数器
bool WarningChangeFlag = false; //报警状态变化标志
bool WarningShown = false; //边框是否绘制标志

bool ProtectionShown = false; //标题栏保护是否绘制标志

//传感器
TFT_eSPI Tft = TFT_eSPI();

uint16_t TitleGray = 0;
uint16_t BackGroundMiddleGray = 0;
uint16_t BackGroundDarkGray = 0;
uint16_t TotalPowerSectionBackgroundColor = 0;
uint16_t TitleLeftTextColor = TFT_GREEN;
uint16_t TitleRightTextColor = TFT_WHITE;
uint16_t ChannelLabelTextColor = TFT_WHITE;
uint16_t VoltageTextColor = TFT_WHITE;
uint16_t VoltageWarningColor = TFT_ORANGE;
uint16_t CurrentTextColor = TFT_WHITE;
uint16_t CurrentWarningColor = TFT_ORANGE;
uint16_t CurrentAlertColor = TFT_RED;
uint16_t CurrentLowColor = 0;
uint16_t TemperatureLabelTextColor = TFT_WHITE;
uint16_t CableLabelTextColor = TFT_WHITE;
uint16_t BoardLabelTextColor = TFT_WHITE;
uint16_t TemperatureUnitTextColor = TFT_WHITE;
uint16_t TemperatureValueColor = TFT_WHITE;
uint16_t TemperatureWarningColor = TFT_ORANGE;
uint16_t TemperatureAlertColor = TFT_RED;
uint16_t TotalPowerTextColor = TFT_WHITE;
uint16_t TotalPowerWarningColor = TFT_ORANGE;
uint16_t TotalPowerAlertColor = TFT_RED;
uint16_t FanOnBackgroundColor = TFT_GREEN;
uint16_t FanOffBackgroundColor = 0;
uint16_t FanTextOnColor = TFT_BLACK;
uint16_t FanTextOffColor = TFT_WHITE;

String DisplayCustomizationPayload = "";
String TitleLeftDisplayText = "WING STUDIO";
String TitleRightDisplayText = "12V-2X6 Ti";
String ProtectionLeftDisplayText = "PROTECTION";
String ProtectionRightDisplayText = "TRIGGERED";
String ChannelLabelTexts[MAX_CHANNEL_COUNT] = { "CH-A", "CH-B", "CH-C", "CH-D", "CH-E", "CH-F" };
String CableLabelText = "Cable";
String BoardLabelText = "Board";
int32_t CableLabelOffsetX = 0;
int32_t CableLabelOffsetY = 0;
int32_t CableUnitOffsetX = 0;
int32_t CableUnitOffsetY = 0;
int32_t CableValueOffsetX = 0;
int32_t CableValueOffsetY = 0;
int32_t BoardLabelOffsetX = 0;
int32_t BoardLabelOffsetY = 0;
int32_t BoardUnitOffsetX = 0;
int32_t BoardUnitOffsetY = 0;
int32_t BoardValueOffsetX = 0;
int32_t BoardValueOffsetY = 0;
String TemperatureUnitText = "TMP (C)";
String TotalPowerLabelText = "Total Power :";
String FanLabelText = "FAN";
String FanOnText = "ON";
String FanOffText = "OFF";

bool FanSwitch = false;

bool Ina3221_Found[2] = {false, false};


bool Cable_Tmp117_Found = false;
bool Board_Tmp117_Found = false;

float ChannelVoltage[] = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0};
float ChannelCurrent[] = {0.0, 0.0, 0.0, 0.0, 0.0, 0.0};
float AverageChannelCurrent = 0.0;
float AverageChannelVoltage = 0.0;
float TotalPower = 0.0;

float Cable_Tmp = 0.0;
float Board_Tmp = 0.0;

uint32_t SensorDataRefreshCount = 0;

LinkedList<String> I2CDeviceList = LinkedList<String>();

LinkedList<Adafruit_INA3221> Ina3221List = LinkedList<Adafruit_INA3221>();

Adafruit_INA3221 Ina3221_1 = Adafruit_INA3221();
Adafruit_INA3221 Ina3221_2 = Adafruit_INA3221();

Adafruit_TMP117 Cable_Tmp117 = Adafruit_TMP117();
Adafruit_TMP117 Board_Tmp117 = Adafruit_TMP117();

void scanI2CDevices();

void setDisplayCustomizationDefaults();

void loadDisplayCustomization();

void applyDisplayCustomizationPayload(String payload, bool persist);

String buildDisplayCustomizationPayload();

void appendDisplayConfigEntry(String &payload, String key, String value);

String encodeDeviceValue(String value);

String colorToHexString(uint16_t color);

void drawStaticLayout();

void drawNormalHeader();

void drawProtectionHeader();

void drawSensorAvailabilityMessages();

String decodeDeviceValue(String value);

uint16_t colorFromHexString(String value, uint16_t fallback);

int hexCharToInt(char value);

int normalizeDisplayFontId(int value);

void changeFanStatus();

void drawChannelStatus(uint32_t channelIndex, float voltage, float current);

uint16_t colorOfVoltage(float volt);

uint16_t colorOfCurrent(float current);

void refreshSeneorData();

void drawTemperture(float temp, uint32_t centerPositionX, uint32_t positionY);

void warningHandler();

void serialCommandHandler(String input);

void drawBoarder(uint16_t color);

void triggerProtection();

void setup() {

  Tft.init();

  Serial.begin(115200);

  Config.begin("WingStudioCable", false); //打开配置文件

  //读取配置文件
  ScreenRotation = Config.getInt("ScreenRotation", 1); //屏幕旋转方向
  DisplayFontId = normalizeDisplayFontId(Config.getInt("DisplayFontId", 4));
  ShuntResistance = Config.getFloat("ShuntResistance", 2); //检流电阻阻值
  CurrentCorrectionRate = Config.getFloat("CurrCorrectRate", 0.95); //电流校正系数
  FanControlType = Config.getInt("FanControlType", 0); //风扇控制类型 0=自动 1=保持关闭 2=保持开启
  FanOnTMP = Config.getFloat("FanOnTMP", 45.0); //风扇开启温度

  AlarmTMP = Config.getFloat("AlarmTMP", 85.0); //报警温度
  AlarmCurrent = Config.getFloat("AlarmCurrent", 12); //报警电流
  AlarmTimeOut = Config.getFloat("AlarmTimeOut", 120); //报警超时时间 -1=永不超时

  ProtectionCurrent = Config.getFloat("ProtectionCurr", 15); //强制保护电流
  ProtectionTMP = Config.getFloat("ProtectionTMP", 100.0); //强制保护温度
  setDisplayCustomizationDefaults();
  loadDisplayCustomization();
  
  //屏幕初始化
  if (Tft.getRotation() == 0) {
    Serial.println("TFT init OK");
  } else {
    Serial.println("TFT init FAILED");
  }

  Tft.setRotation(ScreenRotation); // 屏幕旋转方向（0-3）
  
  //风扇控制
  pinMode(FAN_CONTROL_PIN, OUTPUT);
  digitalWrite(FAN_CONTROL_PIN, LOW);

  //蜂鸣器
  ledcSetup(0, 0, 8);    // 通道0，初始2.7kHz，8位分辨率
  ledcAttachPin(BUZZER_PIN, 0);
  ledcWrite(0, 0); //0设置PWM占空比为0，关闭PWM输出
  
  //sense控制
  pinMode(SENSE_CONTROL_PIN, OUTPUT_OPEN_DRAIN); //设置为开漏输出
  digitalWrite(SENSE_CONTROL_PIN, HIGH); //设置为高阻态，关闭sense

  drawStaticLayout();

  //风扇状态
  if (FanControlType == 0){

    FanIsOn = true;

  }else if (FanControlType == 1){

    FanIsOn = false;
  
  }else if (FanControlType == 2){

    FanIsOn = true;
  }

  changeFanStatus();

  //i2c初始化
  Wire.begin(I2C_SDA, I2C_SCL, I2C_FREQ);
  scanI2CDevices();

  Serial.println("device initialization");

  //ina 3221初始化
  if (!Ina3221_1.begin(INA3221_1_ADDR)) {  

    Serial.println("INA3221_1(0X40) not found");
    Tft.drawString("INA3221_1(0X40) Not Found", 20, 62, DisplayFontId);

  }else{

    Ina3221_Found[0] = true;
    Ina3221_1.setShuntResistance(0,ShuntResistance/1000);
    Ina3221_1.setShuntResistance(1,ShuntResistance/1000);
    Ina3221_1.setShuntResistance(2,ShuntResistance/1000);
  
    Ina3221_1.setAveragingMode(INA3221_AVG_64_SAMPLES);
    Ina3221_1.setShuntVoltageConvTime(INA3221_CONVTIME_2MS);
    Ina3221_1.setBusVoltageConvTime(INA3221_CONVTIME_2MS);
  }

  if (!Ina3221_2.begin(INA3221_2_ADDR)) {  

    Serial.println("INA3221_2(0X41) not found");
    Tft.drawString("INA3221_2(0X41) Not Found", 20, 158, DisplayFontId);

  } else {
    
    Ina3221_Found[1] = true;
    Ina3221_2.setShuntResistance(0,ShuntResistance/1000);
    Ina3221_2.setShuntResistance(1,ShuntResistance/1000);
    Ina3221_2.setShuntResistance(2,ShuntResistance/1000);
  
    Ina3221_2.setAveragingMode(INA3221_AVG_64_SAMPLES);
    Ina3221_2.setShuntVoltageConvTime(INA3221_CONVTIME_2MS);
    Ina3221_2.setBusVoltageConvTime(INA3221_CONVTIME_2MS);

  }

  Ina3221List.add(Ina3221_1); // 将INA3221_1添加到列表中
  Ina3221List.add(Ina3221_2); // 将INA3221_2添加到列表中

  //TMP117初始化
  //接头
  if (!Cable_Tmp117.begin(CONNECTOR_TMP117_ADDR)) {  
    Serial.println("tmp117 not found");
    
  Tft.drawString("Cable", TMP_TEXT_POSITION_X, 58, DisplayFontId);
  Tft.drawString("TMPSensor", 350, 86, DisplayFontId);
  Tft.drawString("Not Found", 350, 118, DisplayFontId);

  }else{
    Cable_Tmp117_Found = true;
    Cable_Tmp117.setAveragedSampleCount(TMP117_AVERAGE_8X);
  }

  //主控板温度传感器
  if (!Board_Tmp117.begin(CONTROLBOARD_TMP117_ADDR)) {  
    Serial.println("tmp117 not found");
    
    Tft.drawString(BoardLabelText, TMP_TEXT_POSITION_X, 152, DisplayFontId);
    Tft.drawString("TMPSensor", 350, 180, DisplayFontId);
    Tft.drawString("Not Found", 350, 212, DisplayFontId);

  }else{
    Board_Tmp117_Found = true;
    Board_Tmp117.setAveragedSampleCount(TMP117_AVERAGE_8X);
  }

  drawSensorAvailabilityMessages();

  Serial.println("initial finished");

}

void loop() {


    //刷新传感器数据
    if (SensorDataRefreshCount >= 20)
    {

      try
      {
        refreshSeneorData();
      }
      catch(const std::exception& e)
      {
        Serial.println("catch exception");
        Serial.println(e.what());
      }

      SensorDataRefreshCount = 0;
    }
    SensorDataRefreshCount ++;


  //输入命令处理
  if (Serial.available() > 0) {
    String input = Serial.readStringUntil('\n');
    input.trim(); // 去除首尾空格

    serialCommandHandler(input); // 处理输入命令
    
    Serial.flush();
  }

  //报警处理
  warningHandler();

  delay(RefreshInterval);
 
}

void setDisplayCustomizationDefaults() {
  TitleGray = Tft.color565(90, 90, 90);
  BackGroundMiddleGray = Tft.color565(32, 32, 32);
  BackGroundDarkGray = Tft.color565(10, 10, 10);
  TotalPowerSectionBackgroundColor = Tft.color565(10, 10, 10);
  TitleLeftTextColor = Tft.color565(92, 255, 92);
  TitleRightTextColor = TFT_WHITE;
  ChannelLabelTextColor = TFT_WHITE;
  VoltageTextColor = TFT_WHITE;
  VoltageWarningColor = TFT_ORANGE;
  CurrentTextColor = TFT_WHITE;
  CurrentWarningColor = TFT_ORANGE;
  CurrentAlertColor = TFT_RED;
  CurrentLowColor = Tft.color565(30, 138, 255);
  TemperatureLabelTextColor = TFT_WHITE;
  CableLabelTextColor = TemperatureLabelTextColor;
  BoardLabelTextColor = TemperatureLabelTextColor;
  TemperatureUnitTextColor = TemperatureLabelTextColor;
  TemperatureValueColor = TFT_WHITE;
  TemperatureWarningColor = TFT_ORANGE;
  TemperatureAlertColor = TFT_RED;
  TotalPowerTextColor = TFT_WHITE;
  TotalPowerWarningColor = TFT_ORANGE;
  TotalPowerAlertColor = TFT_RED;
  FanOnBackgroundColor = Tft.color565(0, 200, 0);
  FanOffBackgroundColor = Tft.color565(32, 32, 32);
  FanTextOnColor = TFT_BLACK;
  FanTextOffColor = TFT_WHITE;
  DisplayFontId = 4;
  DisplayCustomizationPayload = "";
  TitleLeftDisplayText = "WING STUDIO";
  TitleRightDisplayText = "12V-2X6 Ti";
  ProtectionLeftDisplayText = "PROTECTION";
  ProtectionRightDisplayText = "TRIGGERED";
  ChannelLabelTexts[0] = "CH-A";
  ChannelLabelTexts[1] = "CH-B";
  ChannelLabelTexts[2] = "CH-C";
  ChannelLabelTexts[3] = "CH-D";
  ChannelLabelTexts[4] = "CH-E";
  ChannelLabelTexts[5] = "CH-F";
  CableLabelText = "Cable";
  BoardLabelText = "Board";
  CableLabelOffsetX = 0;
  CableLabelOffsetY = 0;
  CableUnitOffsetX = 0;
  CableUnitOffsetY = 0;
  CableValueOffsetX = 0;
  CableValueOffsetY = 0;
  BoardLabelOffsetX = 0;
  BoardLabelOffsetY = 0;
  BoardUnitOffsetX = 0;
  BoardUnitOffsetY = 0;
  BoardValueOffsetX = 0;
  BoardValueOffsetY = 0;
  TemperatureUnitText = "TMP (C)";
  TotalPowerLabelText = "Total Power :";
  FanLabelText = "FAN";
  FanOnText = "ON";
  FanOffText = "OFF";
}

void loadDisplayCustomization() {
  String payload = Config.getString("DisplayCfg", "");
  if (payload.length() > 0) {
    applyDisplayCustomizationPayload(payload, false);
  }
}

void drawNormalHeader() {
  Tft.fillRect(12, 12, 456, 31, TitleGray);

  Tft.setTextColor(TitleLeftTextColor, TitleGray);
  Tft.drawCentreString(TitleLeftDisplayText, 126, 17, DisplayFontId);

  Tft.setTextColor(TitleRightTextColor, TitleGray);
  Tft.drawCentreString(TitleRightDisplayText, 354, 17, DisplayFontId);
}

void drawProtectionHeader() {
  Tft.fillRect(12, 12, 456, 31, TFT_RED);
  Tft.setTextColor(TFT_WHITE, TFT_RED);
  Tft.drawCentreString(ProtectionLeftDisplayText, 126, 17, DisplayFontId);
  Tft.drawCentreString(ProtectionRightDisplayText, 354, 17, DisplayFontId);
}

void drawStaticLayout() {
  Tft.fillScreen(TFT_BLACK);

  if (ProtectionTriggered) {
    drawProtectionHeader();
  } else {
    drawNormalHeader();
  }

  Tft.fillRect(12, 43, 336, 207, BackGroundMiddleGray);
  Tft.fillRect(348, 43, 120, 207, BackGroundDarkGray);
  Tft.fillRect(12, 250, 336, 58, TotalPowerSectionBackgroundColor);

  Tft.setTextColor(CableLabelTextColor, BackGroundDarkGray);
  Tft.drawCentreString(CableLabelText, TMP_TEXT_POSITION_X + 22 + CableLabelOffsetX, 60 + CableLabelOffsetY, DisplayFontId);
  Tft.setTextColor(TemperatureUnitTextColor, BackGroundDarkGray);
  Tft.drawCentreString(TemperatureUnitText, TMP_TEXT_POSITION_X + 22 + CableUnitOffsetX, 88 + CableUnitOffsetY, DisplayFontId);
  Tft.setTextColor(BoardLabelTextColor, BackGroundDarkGray);
  Tft.drawCentreString(BoardLabelText, TMP_TEXT_POSITION_X + 22 + BoardLabelOffsetX, 154 + BoardLabelOffsetY, DisplayFontId);
  Tft.setTextColor(TemperatureUnitTextColor, BackGroundDarkGray);
  Tft.drawCentreString(TemperatureUnitText, TMP_TEXT_POSITION_X + 22 + BoardUnitOffsetX, 182 + BoardUnitOffsetY, DisplayFontId);

  changeFanStatus();
}

void drawSensorAvailabilityMessages() {
  Tft.setTextColor(CableLabelTextColor, BackGroundDarkGray);

  if (!Cable_Tmp117_Found) {
    Tft.drawString(CableLabelText, TMP_TEXT_POSITION_X + CableLabelOffsetX, 58 + CableLabelOffsetY, DisplayFontId);
    Tft.setTextColor(TemperatureUnitTextColor, BackGroundDarkGray);
    Tft.drawString("TMPSensor", 350 + CableUnitOffsetX, 86 + CableUnitOffsetY, DisplayFontId);
    Tft.drawString("Not Found", 350, 118, DisplayFontId);
  }

  if (!Board_Tmp117_Found) {
    Tft.setTextColor(BoardLabelTextColor, BackGroundDarkGray);
    Tft.drawString(BoardLabelText, TMP_TEXT_POSITION_X + BoardLabelOffsetX, 152 + BoardLabelOffsetY, DisplayFontId);
    Tft.setTextColor(TemperatureUnitTextColor, BackGroundDarkGray);
    Tft.drawString("TMPSensor", 350 + BoardUnitOffsetX, 180 + BoardUnitOffsetY, DisplayFontId);
    Tft.drawString("Not Found", 350, 212, DisplayFontId);
  }
}

void applyDisplayCustomizationPayload(String payload, bool persist) {
  DisplayCustomizationPayload = payload;

  int startIndex = 0;
  while (startIndex < payload.length()) {
    int separatorIndex = payload.indexOf(';', startIndex);
    String entry = separatorIndex == -1 ? payload.substring(startIndex) : payload.substring(startIndex, separatorIndex);
    int equalIndex = entry.indexOf('=');

    if (equalIndex > 0) {
      String key = entry.substring(0, equalIndex);
      String value = decodeDeviceValue(entry.substring(equalIndex + 1));

      if (key.equals("TitleBackground")) {
        TitleGray = colorFromHexString(value, TitleGray);
      } else if (key.equals("TitleLeftColor")) {
        TitleLeftTextColor = colorFromHexString(value, TitleLeftTextColor);
      } else if (key.equals("TitleRightColor")) {
        TitleRightTextColor = colorFromHexString(value, TitleRightTextColor);
      } else if (key.equals("ChannelSectionBackground")) {
        BackGroundMiddleGray = colorFromHexString(value, BackGroundMiddleGray);
      } else if (key.equals("ChannelLabelColor")) {
        ChannelLabelTextColor = colorFromHexString(value, ChannelLabelTextColor);
      } else if (key.equals("VoltageTextColor")) {
        VoltageTextColor = colorFromHexString(value, VoltageTextColor);
      } else if (key.equals("VoltageWarningColor")) {
        VoltageWarningColor = colorFromHexString(value, VoltageWarningColor);
      } else if (key.equals("CurrentTextColor")) {
        CurrentTextColor = colorFromHexString(value, CurrentTextColor);
      } else if (key.equals("CurrentWarningColor")) {
        CurrentWarningColor = colorFromHexString(value, CurrentWarningColor);
      } else if (key.equals("CurrentAlertColor")) {
        CurrentAlertColor = colorFromHexString(value, CurrentAlertColor);
      } else if (key.equals("CurrentLowColor")) {
        CurrentLowColor = colorFromHexString(value, CurrentLowColor);
      } else if (key.equals("TemperatureSectionBackground")) {
        BackGroundDarkGray = colorFromHexString(value, BackGroundDarkGray);
      } else if (key.equals("TemperatureLabelColor")) {
        TemperatureLabelTextColor = colorFromHexString(value, TemperatureLabelTextColor);
        CableLabelTextColor = TemperatureLabelTextColor;
        BoardLabelTextColor = TemperatureLabelTextColor;
        TemperatureUnitTextColor = TemperatureLabelTextColor;
      } else if (key.equals("CableLabelColor")) {
        CableLabelTextColor = colorFromHexString(value, CableLabelTextColor);
      } else if (key.equals("BoardLabelColor")) {
        BoardLabelTextColor = colorFromHexString(value, BoardLabelTextColor);
      } else if (key.equals("TemperatureUnitColor")) {
        TemperatureUnitTextColor = colorFromHexString(value, TemperatureUnitTextColor);
      } else if (key.equals("TemperatureValueColor")) {
        TemperatureValueColor = colorFromHexString(value, TemperatureValueColor);
      } else if (key.equals("TemperatureWarningColor")) {
        TemperatureWarningColor = colorFromHexString(value, TemperatureWarningColor);
      } else if (key.equals("TemperatureAlertColor")) {
        TemperatureAlertColor = colorFromHexString(value, TemperatureAlertColor);
      } else if (key.equals("TotalPowerSectionBackground")) {
        TotalPowerSectionBackgroundColor = colorFromHexString(value, TotalPowerSectionBackgroundColor);
      } else if (key.equals("TotalPowerTextColor")) {
        TotalPowerTextColor = colorFromHexString(value, TotalPowerTextColor);
      } else if (key.equals("TotalPowerWarningColor")) {
        TotalPowerWarningColor = colorFromHexString(value, TotalPowerWarningColor);
      } else if (key.equals("TotalPowerAlertColor")) {
        TotalPowerAlertColor = colorFromHexString(value, TotalPowerAlertColor);
      } else if (key.equals("FanOnBackground")) {
        FanOnBackgroundColor = colorFromHexString(value, FanOnBackgroundColor);
      } else if (key.equals("FanOffBackground")) {
        FanOffBackgroundColor = colorFromHexString(value, FanOffBackgroundColor);
      } else if (key.equals("FanTextOnColor")) {
        FanTextOnColor = colorFromHexString(value, FanTextOnColor);
      } else if (key.equals("FanTextOffColor")) {
        FanTextOffColor = colorFromHexString(value, FanTextOffColor);
      } else if (key.equals("DisplayFontId")) {
        DisplayFontId = normalizeDisplayFontId(value.toInt());
      } else if (key.equals("TitleLeftText")) {
        TitleLeftDisplayText = value;
      } else if (key.equals("TitleRightText")) {
        TitleRightDisplayText = value;
      } else if (key.equals("ProtectionLeftText")) {
        ProtectionLeftDisplayText = value;
      } else if (key.equals("ProtectionRightText")) {
        ProtectionRightDisplayText = value;
      } else if (key.equals("ChannelLabels")) {
        int labelStart = 0;
        int labelIndex = 0;
        while (labelStart <= value.length() && labelIndex < MAX_CHANNEL_COUNT) {
          int commaIndex = value.indexOf(',', labelStart);
          if (commaIndex == -1) {
            ChannelLabelTexts[labelIndex] = value.substring(labelStart);
            labelIndex++;
            break;
          }

          ChannelLabelTexts[labelIndex] = value.substring(labelStart, commaIndex);
          labelIndex++;
          labelStart = commaIndex + 1;
        }
      } else if (key.equals("CableLabelText")) {
        CableLabelText = value;
      } else if (key.equals("BoardLabelText")) {
        BoardLabelText = value;
      } else if (key.equals("CableLabelOffsetX")) {
        CableLabelOffsetX = value.toInt();
      } else if (key.equals("CableLabelOffsetY")) {
        CableLabelOffsetY = value.toInt();
      } else if (key.equals("CableUnitOffsetX")) {
        CableUnitOffsetX = value.toInt();
      } else if (key.equals("CableUnitOffsetY")) {
        CableUnitOffsetY = value.toInt();
      } else if (key.equals("CableValueOffsetX")) {
        CableValueOffsetX = value.toInt();
      } else if (key.equals("CableValueOffsetY")) {
        CableValueOffsetY = value.toInt();
      } else if (key.equals("BoardLabelOffsetX")) {
        BoardLabelOffsetX = value.toInt();
      } else if (key.equals("BoardLabelOffsetY")) {
        BoardLabelOffsetY = value.toInt();
      } else if (key.equals("BoardUnitOffsetX")) {
        BoardUnitOffsetX = value.toInt();
      } else if (key.equals("BoardUnitOffsetY")) {
        BoardUnitOffsetY = value.toInt();
      } else if (key.equals("BoardValueOffsetX")) {
        BoardValueOffsetX = value.toInt();
      } else if (key.equals("BoardValueOffsetY")) {
        BoardValueOffsetY = value.toInt();
      } else if (key.equals("TemperatureUnitText")) {
        TemperatureUnitText = value;
      } else if (key.equals("TotalPowerLabelText")) {
        TotalPowerLabelText = value;
      } else if (key.equals("FanLabelText")) {
        FanLabelText = value;
      } else if (key.equals("FanOnText")) {
        FanOnText = value;
      } else if (key.equals("FanOffText")) {
        FanOffText = value;
      }
    }

    if (separatorIndex == -1) {
      break;
    }

    startIndex = separatorIndex + 1;
  }

  if (persist) {
    Config.putInt("DisplayFontId", DisplayFontId);
    Config.putString("DisplayCfg", buildDisplayCustomizationPayload());
  }

  drawStaticLayout();
  drawSensorAvailabilityMessages();

  if (ProtectionTriggered) {
    drawProtectionHeader();
  } else {
    refreshSeneorData();
  }

  if (WarningShown) {
    drawBoarder(TFT_RED);
  }
}

String buildDisplayCustomizationPayload() {
  String payload = "";

  appendDisplayConfigEntry(payload, "TitleBackground", colorToHexString(TitleGray));
  appendDisplayConfigEntry(payload, "TitleLeftColor", colorToHexString(TitleLeftTextColor));
  appendDisplayConfigEntry(payload, "TitleRightColor", colorToHexString(TitleRightTextColor));
  appendDisplayConfigEntry(payload, "ChannelSectionBackground", colorToHexString(BackGroundMiddleGray));
  appendDisplayConfigEntry(payload, "ChannelLabelColor", colorToHexString(ChannelLabelTextColor));
  appendDisplayConfigEntry(payload, "VoltageTextColor", colorToHexString(VoltageTextColor));
  appendDisplayConfigEntry(payload, "VoltageWarningColor", colorToHexString(VoltageWarningColor));
  appendDisplayConfigEntry(payload, "CurrentTextColor", colorToHexString(CurrentTextColor));
  appendDisplayConfigEntry(payload, "CurrentWarningColor", colorToHexString(CurrentWarningColor));
  appendDisplayConfigEntry(payload, "CurrentAlertColor", colorToHexString(CurrentAlertColor));
  appendDisplayConfigEntry(payload, "CurrentLowColor", colorToHexString(CurrentLowColor));
  appendDisplayConfigEntry(payload, "TemperatureSectionBackground", colorToHexString(BackGroundDarkGray));
  appendDisplayConfigEntry(payload, "TemperatureLabelColor", colorToHexString(TemperatureLabelTextColor));
  appendDisplayConfigEntry(payload, "CableLabelColor", colorToHexString(CableLabelTextColor));
  appendDisplayConfigEntry(payload, "BoardLabelColor", colorToHexString(BoardLabelTextColor));
  appendDisplayConfigEntry(payload, "TemperatureUnitColor", colorToHexString(TemperatureUnitTextColor));
  appendDisplayConfigEntry(payload, "TemperatureValueColor", colorToHexString(TemperatureValueColor));
  appendDisplayConfigEntry(payload, "TemperatureWarningColor", colorToHexString(TemperatureWarningColor));
  appendDisplayConfigEntry(payload, "TemperatureAlertColor", colorToHexString(TemperatureAlertColor));
  appendDisplayConfigEntry(payload, "TotalPowerSectionBackground", colorToHexString(TotalPowerSectionBackgroundColor));
  appendDisplayConfigEntry(payload, "TotalPowerTextColor", colorToHexString(TotalPowerTextColor));
  appendDisplayConfigEntry(payload, "TotalPowerWarningColor", colorToHexString(TotalPowerWarningColor));
  appendDisplayConfigEntry(payload, "TotalPowerAlertColor", colorToHexString(TotalPowerAlertColor));
  appendDisplayConfigEntry(payload, "FanOnBackground", colorToHexString(FanOnBackgroundColor));
  appendDisplayConfigEntry(payload, "FanOffBackground", colorToHexString(FanOffBackgroundColor));
  appendDisplayConfigEntry(payload, "FanTextOnColor", colorToHexString(FanTextOnColor));
  appendDisplayConfigEntry(payload, "FanTextOffColor", colorToHexString(FanTextOffColor));
  appendDisplayConfigEntry(payload, "DisplayFontId", String(DisplayFontId));
  appendDisplayConfigEntry(payload, "TitleLeftText", TitleLeftDisplayText);
  appendDisplayConfigEntry(payload, "TitleRightText", TitleRightDisplayText);
  appendDisplayConfigEntry(payload, "ProtectionLeftText", ProtectionLeftDisplayText);
  appendDisplayConfigEntry(payload, "ProtectionRightText", ProtectionRightDisplayText);
  appendDisplayConfigEntry(payload, "ChannelLabels", ChannelLabelTexts[0] + "," + ChannelLabelTexts[1] + "," + ChannelLabelTexts[2] + "," + ChannelLabelTexts[3] + "," + ChannelLabelTexts[4] + "," + ChannelLabelTexts[5]);
  appendDisplayConfigEntry(payload, "CableLabelText", CableLabelText);
  appendDisplayConfigEntry(payload, "BoardLabelText", BoardLabelText);
  appendDisplayConfigEntry(payload, "CableLabelOffsetX", String(CableLabelOffsetX));
  appendDisplayConfigEntry(payload, "CableLabelOffsetY", String(CableLabelOffsetY));
  appendDisplayConfigEntry(payload, "CableUnitOffsetX", String(CableUnitOffsetX));
  appendDisplayConfigEntry(payload, "CableUnitOffsetY", String(CableUnitOffsetY));
  appendDisplayConfigEntry(payload, "CableValueOffsetX", String(CableValueOffsetX));
  appendDisplayConfigEntry(payload, "CableValueOffsetY", String(CableValueOffsetY));
  appendDisplayConfigEntry(payload, "BoardLabelOffsetX", String(BoardLabelOffsetX));
  appendDisplayConfigEntry(payload, "BoardLabelOffsetY", String(BoardLabelOffsetY));
  appendDisplayConfigEntry(payload, "BoardUnitOffsetX", String(BoardUnitOffsetX));
  appendDisplayConfigEntry(payload, "BoardUnitOffsetY", String(BoardUnitOffsetY));
  appendDisplayConfigEntry(payload, "BoardValueOffsetX", String(BoardValueOffsetX));
  appendDisplayConfigEntry(payload, "BoardValueOffsetY", String(BoardValueOffsetY));
  appendDisplayConfigEntry(payload, "TemperatureUnitText", TemperatureUnitText);
  appendDisplayConfigEntry(payload, "TotalPowerLabelText", TotalPowerLabelText);
  appendDisplayConfigEntry(payload, "FanLabelText", FanLabelText);
  appendDisplayConfigEntry(payload, "FanOnText", FanOnText);
  appendDisplayConfigEntry(payload, "FanOffText", FanOffText);

  return payload;
}

void appendDisplayConfigEntry(String &payload, String key, String value) {
  if (payload.length() > 0) {
    payload += ";";
  }

  payload += key;
  payload += "=";
  payload += encodeDeviceValue(value);
}

String encodeDeviceValue(String value) {
  String encoded = "";

  for (int index = 0; index < value.length(); index++) {
    char current = value.charAt(index);
    bool unescaped =
      (current >= 'A' && current <= 'Z') ||
      (current >= 'a' && current <= 'z') ||
      (current >= '0' && current <= '9') ||
      current == '-' || current == '_' || current == '.' || current == '~';

    if (unescaped) {
      encoded += current;
    } else {
      char hex[4];
      sprintf(hex, "%%%02X", (uint8_t)current);
      encoded += hex;
    }
  }

  return encoded;
}

String colorToHexString(uint16_t color) {
  uint8_t red = ((color >> 11) & 0x1F) * 255 / 31;
  uint8_t green = ((color >> 5) & 0x3F) * 255 / 63;
  uint8_t blue = (color & 0x1F) * 255 / 31;
  char hex[10];
  sprintf(hex, "#%02X%02X%02X", red, green, blue);
  return String(hex);
}

String decodeDeviceValue(String value) {
  String decoded = "";

  for (int index = 0; index < value.length(); index++) {
    char current = value.charAt(index);
    if (current == '%' && index + 2 < value.length()) {
      int upper = hexCharToInt(value.charAt(index + 1));
      int lower = hexCharToInt(value.charAt(index + 2));
      if (upper > -1 && lower > -1) {
        decoded += char((upper << 4) | lower);
        index += 2;
        continue;
      }
    }

    decoded += current;
  }

  return decoded;
}

uint16_t colorFromHexString(String value, uint16_t fallback) {
  value.replace("#", "");
  if (value.length() == 8) {
    value = value.substring(2);
  }

  if (value.length() != 6) {
    return fallback;
  }

  uint32_t rawColor = strtoul(value.c_str(), nullptr, 16);
  uint8_t red = (rawColor >> 16) & 0xFF;
  uint8_t green = (rawColor >> 8) & 0xFF;
  uint8_t blue = rawColor & 0xFF;
  return Tft.color565(red, green, blue);
}

int hexCharToInt(char value) {
  if (value >= '0' && value <= '9') {
    return value - '0';
  }

  if (value >= 'A' && value <= 'F') {
    return value - 'A' + 10;
  }

  if (value >= 'a' && value <= 'f') {
    return value - 'a' + 10;
  }

  return -1;
}

int normalizeDisplayFontId(int value) {
  switch (value) {
    case 1:
    case 2:
    case 4:
    case 6:
    case 7:
    case 8:
      return value;
    default:
      return 4;
  }
}


void  refreshSeneorData(){

  WarningTriggered = false;

  if (ProtectionTriggered == true) //保护状态下不刷新，保留最终状态
  {
    return;
  }

  AverageChannelVoltage = 0.0;
  AverageChannelCurrent = 0.0;

  TotalPower = 0.0;
  Tft.setTextColor(TFT_WHITE, BackGroundMiddleGray);

  for (int32_t i = 0; i < Ina3221List.size(); i++)
  {
    if (Ina3221_Found[i] == false)
    {
      continue;
    }
    
    Adafruit_INA3221 ina3221 = Ina3221List.get(i);
    
    for (int32_t ch = 0; ch < 3; ch++)
    {
      int32_t channel = ch + i * 3; //通道索引
      if (channel > 5) break; //最多6个通道

      float shuntVoltage = ina3221.getShuntVoltage(ch);
      ChannelVoltage[channel] =  ina3221.getBusVoltage(ch) ;
      ChannelCurrent[channel] =  ina3221.getCurrentAmps(ch) * CurrentCorrectionRate ;


      
      if (ChannelVoltage[channel] > 99.0){
        ChannelVoltage[channel] = 99.0;
      }
      if (ChannelVoltage[channel] < 0.0){
        ChannelVoltage[channel] = 0.0;
      }

      if (ChannelCurrent[channel] > 99.0){
        ChannelCurrent[channel] = 99.0;
      }

      if (ChannelCurrent[channel] < 0.0){
        ChannelCurrent[channel] = 0.0;
      }


      if (ChannelCurrent[channel] > AlarmCurrent) //过流报警
      {
        WarningTriggered = true;
      }

      if (ChannelCurrent[channel] > ProtectionCurrent) //强制保护
      {
        triggerProtection();
      }
      
      AverageChannelVoltage += ChannelVoltage[channel];
      AverageChannelCurrent += ChannelCurrent[channel];

      TotalPower += ChannelVoltage[channel] * ChannelCurrent[channel]; //计算总功率

      delay(20);
    }
  }

  //计算平均电压电流
  AverageChannelVoltage = AverageChannelVoltage / 6;
  AverageChannelCurrent = AverageChannelCurrent / 6;


  //显示通道状态
  for (int32_t i = 0; i < 6; i++)
  {
    drawChannelStatus(i, ChannelVoltage[i], ChannelCurrent[i]);
  }
 
  //显示总功率
  Tft.setTextColor(TotalPowerTextColor, TotalPowerSectionBackgroundColor);

  if (TotalPower >= 600 && TotalPower < 675)
  {
    Tft.setTextColor(TotalPowerWarningColor, TotalPowerSectionBackgroundColor);
  }else if (TotalPower >= 675)
  {
    Tft.setTextColor(TotalPowerAlertColor, TotalPowerSectionBackgroundColor);
  }
  
  char power[50];
  sprintf(power,"%s %.2fW       ",TotalPowerLabelText.c_str(),TotalPower);
  Tft.drawString(power, 50, 267, DisplayFontId);

  //接头温度传感器
  if (Cable_Tmp117_Found == true){

    //tmp117读取温度
    sensors_event_t temp; // create an empty event to be filled
    Cable_Tmp117.getEvent(&temp); //fill the empty event object with the current measurements
    
    drawTemperture(temp.temperature, TMP_TEXT_POSITION_X + CableValueOffsetX, 116 + CableValueOffsetY);

    Cable_Tmp = temp.temperature;//高温报警
    if (Cable_Tmp >= AlarmTMP)
    {
      WarningTriggered = true;
    }
    if (Cable_Tmp >= ProtectionTMP) //高温保护
    {
      triggerProtection();
    }

    //风扇控制 0=自动 1=保持关闭 2=保持开启
    if (FanControlType == 0){

      bool newFanStatus = FanIsOn;

      if (temp.temperature >= FanOnTMP)
      {
        newFanStatus = true;
      }else if (temp.temperature < FanOnTMP - 5)
      {
        newFanStatus = false;
      }

      if (FanIsOn != newFanStatus)
      {
        FanIsOn = newFanStatus;
        changeFanStatus();
      }
    
    }
  }


  //主控板温度传感器
  if (Board_Tmp117_Found == true){

    //tmp117读取温度
    sensors_event_t temp; // create an empty event to be filled
    Board_Tmp117.getEvent(&temp); //fill the empty event object with the current measurements
    
    //temp.temperature = -120.99;
    Board_Tmp = temp.temperature; //高温报警
    if (Board_Tmp >= AlarmTMP)
    {
      WarningTriggered = true;
    }
    
    if (Board_Tmp >= ProtectionTMP) //高温保护
    {
      triggerProtection();
    }
    

    drawTemperture(temp.temperature, TMP_TEXT_POSITION_X + BoardValueOffsetX, 210 + BoardValueOffsetY);
  }

}


void drawTemperture(float temp, uint32_t positionX, uint32_t positionY){

  Tft.setTextColor(TemperatureValueColor, BackGroundDarkGray);

  if (temp >= AlarmTMP)
  {
    Tft.setTextColor(TemperatureAlertColor, BackGroundDarkGray);

  }else if ( temp >= AlarmTMP - 10)
  {
    Tft.setTextColor(TemperatureWarningColor, BackGroundDarkGray);
  }

  //Tft.setTextColor(TFT_WHITE, TitleGray);

  String tempStr = String(temp, 2);

  if (temp >-100 && temp < 100)
  {
    tempStr = tempStr + "      ";

  }else
  {
    tempStr = tempStr + "   ";
  }
  
  //Serial.println(tempStr);

  Tft.drawString(tempStr, positionX , positionY, DisplayFontId);

}


//串行命令处理函数
void serialCommandHandler(String input){

  String upperInput = input;
  upperInput.toUpperCase(); //转换为大写字母

  if (upperInput.equals("*IDN?")) //查询设备ID
  {
    Serial.println(DEVICE_NAME);

  }else if (upperInput.equals("SENSORDATA?")){ //读取传感器数据

    String sensorData = "";
    for (int ch = 0; ch < 6; ch++)
    {
      sensorData += String(ChannelVoltage[ch], 2) + "," + String(ChannelCurrent[ch], 2) + ",";
    }

    sensorData += String(TotalPower, 2) + "," + String(Cable_Tmp, 2) + "," + String(Board_Tmp, 2) + "\n";

    Serial.print(sensorData);

  }else if (upperInput.equals("RESET")){ //重启设备
    Serial.println("ESP32 Restarting...");
    ESP.restart();
    delay(1000);

  }else if (upperInput.equals("WARNINGON")){ //报警开启(测试)
    WarningTest = true;

  }else if (upperInput.equals("WARNINGOFF")){ //报警关闭(测试)
    WarningTest = false;

  }else if (upperInput.equals("PROTECTIONON")){ //保护开启(测试)
    triggerProtection();

  }else if (upperInput.equals("PROTECTIONOFF")){ //保护关闭(测试)
    ProtectionTriggered = false;
    
    digitalWrite(SENSE_CONTROL_PIN, HIGH); //恢复sense连接

    drawNormalHeader();
    ProtectionShown = false;

  }else if (upperInput.equals("READCONFIG?")){ //读取配置文件
   
    String config = "";
    config += String(ScreenRotation) + ",";
    config += String(ShuntResistance) + ",";
    config += String(CurrentCorrectionRate) + ",";
    config += String(FanControlType) + ",";
    config += String(FanOnTMP) + ",";
    config += String(AlarmTMP) + ",";
    config += String(AlarmCurrent) + ",";
    config += String(AlarmTimeOut) + ",";
    config += String(ProtectionCurrent) + ",";
    config += String(ProtectionTMP) + "\n";
    Serial.print(config);
    
  }else if (upperInput.indexOf("CONFIG:") == 0){ //扫描I2C设备
    //设置配置文件
    String configString = input.substring(7); //去掉前缀"CONFIG:"
    configString.trim(); // 去除首尾空格

    LinkedList<String> configList = LinkedList<String>();

    while (configString.length() > 0) {
      int commaIndex = configString.indexOf(',');
      if (commaIndex == -1) {
        configList.add(configString);
        break;
      } else {
        String value = configString.substring(0, commaIndex);
        configList.add(value);
        configString = configString.substring(commaIndex + 1);
      }
    }
    
    if (configList.size() == CONFIG_VALUE_COUNT)
    {
      ScreenRotation = configList.get(0).toInt(); //屏幕旋转方向
      ShuntResistance = configList.get(1).toFloat(); //检流电阻阻值
      CurrentCorrectionRate = configList.get(2).toFloat(); //电流校正系数
      FanControlType = configList.get(3).toInt(); //风扇控制类型 0=自动 1=保持关闭 2=保持开启
      FanOnTMP = configList.get(4).toFloat(); //风扇开启温度
      AlarmTMP = configList.get(5).toFloat(); //报警温度
      AlarmCurrent = configList.get(6).toFloat(); //报警电流
      AlarmTimeOut = configList.get(7).toFloat(); //报警超时时间 -1=永不超时
      ProtectionCurrent = configList.get(8).toFloat(); //强制保护电流
      ProtectionTMP = configList.get(9).toFloat(); //强制保护温度

      Config.putInt("ScreenRotation", ScreenRotation); //屏幕旋转方向
      Config.putFloat("ShuntResistance", ShuntResistance); //检流电阻阻值
      Config.putFloat("CurrCorrectRate", CurrentCorrectionRate); //电流校正系数
      Config.putInt("FanControlType", FanControlType); //风扇控制类型 0=自动 1=保持关闭 2=保持开启
      Config.putFloat("FanOnTMP", FanOnTMP); //风扇开启温度
      Config.putFloat("AlarmTMP", AlarmTMP); //报警温度
      Config.putFloat("AlarmCurrent", AlarmCurrent); //报警电流
      Config.putFloat("AlarmTimeOut", AlarmTimeOut); //报警超时时间 -1=永不超时
      Config.putFloat("ProtectionCurr", ProtectionCurrent); //强制保护电流
      Config.putFloat("ProtectionTMP", ProtectionTMP); //强制保护温度

      Serial.println("CONFIG OK");
    }

  }else if (upperInput.indexOf("DISPLAYCFG:") == 0){
    String payload = input.substring(11);
    payload.trim();
    if (payload.equalsIgnoreCase("COMMIT")) {
      Config.putString("DisplayCfg", buildDisplayCustomizationPayload());
      Serial.println("DISPLAYCFG OK");
    } else {
      applyDisplayCustomizationPayload(payload, false);
      Serial.println("DISPLAYCFG OK");
    }
  }else if (upperInput.equals("DISPLAYCFG?")) {
    Serial.println(buildDisplayCustomizationPayload());
  }

}

  //报警处理函数
void warningHandler(){

  if (WarningTest == true &&
     ProtectionTriggered != true) //报警测试
  {
    WarningTriggered = true;
  }

  if (WarningTriggered == true) //报警状态
  {
    WarningShown = true;

    if (WarningCount >= 10)
    {
        if (WarningChangeFlag == false)
        {
          WarningChangeFlag = true;
          ledcWriteTone(0, 2731);  // 输出2.7kHz频率
          ledcWrite(0,200);   
          drawBoarder(TFT_RED); //报警边框
          
        }else{
          WarningChangeFlag = false;
          ledcWriteTone(0, 0);  //关闭蜂鸣器
          ledcWrite(0,0);   
          drawBoarder(TFT_BLACK); //黑边框
        }
        
      WarningCount = 0;
    }
    
    WarningCount ++;

    if (AlarmTimeOut > -1 &&
       WarningTimeOut >= AlarmTimeOut * 10) //报警超时处理
    
    {
      if (ProtectionTriggered == false)
      {
        triggerProtection(); //触发保护
      }
    
    }
    
    WarningTimeOut ++;


  }else{ //无报警状态
    WarningTimeOut = 0;
    WarningCount = 0;
    WarningChangeFlag == false;
    
    if (WarningShown == true)
    {
      WarningShown = false;
      drawBoarder(TFT_BLACK); //黑边框
      ledcWriteTone(0, 0);  //关闭蜂鸣器
      ledcWrite(0,0);   
    }

  }
}

void drawBoarder(uint16_t color){
  Tft.fillRect(0, 0, 480, 12, color);
  Tft.fillRect(0, 0, 12, 320, color);
  Tft.fillRect(468, 0, 12, 320, color);
  Tft.fillRect(0, 308, 480, 12, color);
}



void triggerProtection(){
  
  ProtectionTriggered = true;
  
  digitalWrite(SENSE_CONTROL_PIN, LOW); //切断sense控制引脚

  
  drawProtectionHeader();

}

void changeFanStatus(){

  if(FanIsOn == true){

    digitalWrite(FAN_CONTROL_PIN, LOW);

    Tft.fillRect(348, 250, 120, 58, FanOnBackgroundColor);
  
    Tft.setTextColor(FanTextOnColor, FanOnBackgroundColor);
    Tft.drawCentreString(FanLabelText, 408, 256, DisplayFontId);
    Tft.drawCentreString(FanOnText, 408, 282, DisplayFontId);

  }else{

    digitalWrite(FAN_CONTROL_PIN, HIGH);

    Tft.fillRect(348, 250, 120, 58, FanOffBackgroundColor);
  
    Tft.setTextColor(FanTextOffColor, FanOffBackgroundColor);
    Tft.drawCentreString(FanLabelText, 408, 256, DisplayFontId);
    Tft.drawCentreString(FanOffText, 408, 282, DisplayFontId);
  }

}

uint16_t colorOfVoltage(float volt){
  uint16_t color = VoltageTextColor;

  if (volt < 11.2 || volt >12.7)
  {
    color = VoltageWarningColor;
  }

  return color;
}

uint16_t colorOfCurrent(float current){
  uint16_t color = CurrentTextColor;

  if (current <9)
  {
    color = CurrentTextColor;

  }else if (current >= 9 && current < 12){
    color = CurrentWarningColor;

  }else if  (current >= 12){
    color = CurrentAlertColor;
  }

  if (AverageChannelCurrent > 0.1 && current < AverageChannelCurrent * 0.7)
  {
    color = CurrentLowColor;
  }
  
  return color;
}

void drawChannelStatus(uint32_t channelIndex, float voltage, float current){

  if (channelIndex < 3 && Ina3221_Found[0] == false)
  {
    return;
  }

  if (channelIndex >= 3 && Ina3221_Found[1] == false)
  {
    return;
  }
  
  uint32_t statusPositionX = 26;
  uint32_t channelNamePositionX = 52;
  uint32_t voltPositionX = 140;
  uint32_t currentPositionX = 0;

  uint32_t positionY = channelIndex*32 + 55;

  //显示通道名称
  Tft.setTextColor(ChannelLabelTextColor, BackGroundMiddleGray);

  Tft.drawString(ChannelLabelTexts[channelIndex], channelNamePositionX, positionY, DisplayFontId);
  
  Tft.drawString(" : ", channelNamePositionX + 62, positionY, DisplayFontId);

  //显示电压电流
  String voltStr;
  if (voltage < 10)
  {
    voltStr = " 0" + String(voltage, 2) + "V ";
  
  }else{
    voltStr = " " + String(voltage, 2) + "V ";
  }

  String currStr;
  if (current < 10)
  {
    currStr = " 0" + String(current, 2) + "A ";
  }else{
    currStr = " " + String(current, 2) + "A ";
  } 

  Tft.setTextColor(colorOfVoltage(voltage), BackGroundMiddleGray);
  Tft.drawString(voltStr, voltPositionX, positionY, DisplayFontId);
  
  uint16_t currrentColor = colorOfCurrent(current);
  Tft.setTextColor(currrentColor, BackGroundMiddleGray);
  Tft.drawString(currStr, voltPositionX + 100, positionY, DisplayFontId);

  //指示灯
  if (currrentColor == TFT_WHITE)
  {
    currrentColor = TFT_GREEN;
  }
  
  Tft.fillRect(statusPositionX, positionY, 16, 20, currrentColor);

}

void scanI2CDevices() {
  
  Serial.println("Scanning I2C devices...");

  I2CDeviceList.clear();

  int found = 0;
  for (byte addr = 0x08; addr <= 0x77; addr++) {
    Wire.beginTransmission(addr);
    if (Wire.endTransmission() == 0) {
      Serial.printf("Found device at 0x%02X\n", addr);
      found++;

      char i2cDevice[50];

      sprintf(i2cDevice, "Device 0x%02X",addr);

      I2CDeviceList.add(i2cDevice);

    }
    delay(10);
  }
  Serial.printf("Total found: %d devices\n", found);
}
