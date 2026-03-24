# 翼王智能电线自定义MCU显示屏

这是一个用于翼王智能电线项目的桌面端 + MCU 固件联动工程。
效果图：
[

![Image](https://github.com/user-attachments/assets/9c3c9e25-6df8-4873-8e4a-820022d9158d)

](url)
项目包含两部分：

1. Windows 桌面端 WPF 工具
2. ESP32-S3 MCU 显示屏固件工程

桌面端可用于：

- 通过串口读取 MCU 传回的数据
- 预览 MCU 显示屏效果
- 自定义显示区块颜色、文字、温度区域位置与字体编号
- 将显示配置写入 MCU

MCU 固件可用于：

- 驱动 TFT 屏幕显示通道电压、电流、温度、总功率、风扇状态
- 保存并应用桌面端下发的显示主题配置
- 支持内建 TFT 字体编号切换

## 仓库内容

- `WingForce.exe` 最新桌面端构建输出位于 `bin/Debug/font-sync-test/`
- `主控固件开发工程/` 为 ESP32-S3 PlatformIO 固件源码
- `主控合并固件V1.0.bin` 为现有合并固件文件
- `主控电脑通讯串口命令.docx` 为串口命令说明文档

## 当前推荐桌面端程序

最新已验证可构建版本：

- `bin/Debug/font-sync-test/WingForce.exe`

## 主要功能

- MCU 显示屏桌面预览
- 显示配色自定义
- 静态文本自定义
- 温度标签和值位置微调
- 串口写入显示配置
- MCU 字体编号设置

## MCU 字体说明

当前 MCU 端使用 TFT_eSPI 的内建字体编号，当前支持：

- `1`
- `2`
- `4`
- `6`
- `7`
- `8`

桌面预览会尽量模拟 MCU 字体效果，但它不是 Windows 真字体直传到 MCU。

如果后续需要真正的 Times New Roman 之类字体，需要额外做字体转换和嵌入式字库集成。

## 桌面端构建环境

- .NET 6
- WPF
- 当前环境使用 `C:\Program Files (x86)\dotnet\dotnet.exe` 构建

构建命令示例：

```powershell
& "C:\Program Files (x86)\dotnet\dotnet.exe" build "WingForce.csproj" -c Debug -o ".\bin\Debug\font-sync-test"
```

## 固件构建环境

- PlatformIO
- Arduino framework
- ESP32-S3 DevKitC-1

构建命令示例：

```powershell
& "C:\Users\yegua\.platformio\penv\Scripts\platformio.exe" run
```

上传命令示例：

```powershell
& "C:\Users\yegua\.platformio\penv\Scripts\platformio.exe" run -t upload
```

当前配置上传串口为：

- `COM5`

## 注意事项

- 如果桌面端程序正在运行，占用了 `COM5`，固件上传会失败。
- 公开仓库前请确认是否允许公开发布 `.exe`、`.bin` 与协议文档。
