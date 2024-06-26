# 空间音频示例
![Model](https://github.com/Pico-Developer/SpatialAudioSample-Unity/blob/main/SpatialAudioSampleMain.jpeg)

PICO 空间音频示例展示了Unity中集成PICO Integration SDK中空间音频的功能，并提供了开发者可以参考的一个综合应用案例。这个示例场景包括一个卡通风格的房屋，场景中布置了多个音频源和区域，展示了多种空间音频效果，包括空间化、全景声、指向性以及声音在不同材料间的传输和反射。开发者可以在Unity编辑器和运行时通过菜单UI调整音频参数，从而探索、学习并优化高质量的3D音频功能，以应用到他们自己的创意项目中。

## 开发环境

- PICO Unity Integration SDK Version: 2.5.5, with the Spatial Audio SDK 1.0.0
- XR Interaction Toolkit Package Version: 2.4.3
- PICO device's system version: 5.9.2
- Unity version: 2022.3.21f1

## 编译和安装

您可以构建Unity项目，并将示例的apk文件安装到您的PICO 4系列设备上进行体验。

使用USB线将您的PICO设备连接到PC，然后打开命令行窗口，使用以下ADB命令将apk文件安装到设备上：

adb install "filepath\filename.apk"


## 使用指南

有关如何使用该示例的详细信息，请参阅[此文档](https://developer-cn.picoxr.com/document/unity/spatial-audio-sample/)。

有关PICO Integration SDK 提供的空间音频功能，请参阅[此文档](https://developer-cn.picoxr.com/document/unity/spatial-audio/)。

## LICENSE
本项目根据[MIT License](LICENSE)进行许可。
