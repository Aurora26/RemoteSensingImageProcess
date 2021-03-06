##一、	【程序环境】##
####程序性质：C# 开发的 WPF 桌程序####
####开发平台：Visual Studio 2015、GDAL库、.Net Framework 4.5####
####运行环境：Windows 8.1 以上####

##二、	【程序目的】
####`GDAL` 库是一个开源库，它能够实现读取任意格式的图像文件，包括遥感图像，本程序利用 `GDAL` 库来读取遥感图像，使用 `C#` 实现一些核心图像的功能，包括：

1.	遥感图像及普通图像读取：实现读取 `.img` 遥感图像及选择波段进行处理，实现读取大部分常见图像格式

2.	图像基本操作：实现图像平移、缩放等常见操作 

3.	遥感图像增强处理：实现灰度拉伸、`HIS`变换、图像平滑、图像锐化、边缘增强、反相等增强处理

##三、	【设计思路】##

###1)	程序使用`C#`来开发`WPF`桌面应用程序，界面使用`XAML`编写，实现界面与业务逻辑分离###
###2)	程序包括几个主要模块###

1.	遥感图像读取模块：使用`GDAL`读取遥感图像，这一步的结果是`DataSet`数据集

2.	图像内部存储模块： 包括将读取的`DataSet`处理成`C#`能够处理的`BitMap`数据集及波段信息的提取，另外为了将图像显示在`WPF`的`Image`控件上，还需将`BitMap`数据集转换为`BitMapImage`数据集

3.	界面事件模块： 这一层实质上是实现了界面事件与业务逻辑的交互

4.	图像处理模块： 业务逻辑层，编写了灰度拉伸、`HIS`变换、图像平滑、图像锐化、边缘增强、反相等图像增强处理的函数

###3)	程序使用事件机制驱动###

##四、	【实现过程】##
###1)	读取模块###

1.	使用`GDAL`库的`Gdal.Open`方法打开遥感文件，将之存储为`datase`数据集；

2.	利用`System.Drawing.Rectangle`获取图像容器的宽高；

3.	利用`dataSet.GetMetadata`获取图像的波段信息，将波段信息作为波段选择窗口的构造函数实例化一个新的波段选择窗口，用户可自由选择显示波段；

4.	定义一个`int`类型的数组存储用户选择的波段信息，使用遥感图像读取模块的主要类`ImageOperate`中的`ImageOperate.GetImage`方法来将`DataSet`数据集转化为`Bitmap`数据集，该方法需要传入三个参数，`DataSet`数据集，选择的三个波段序号，以及由图像容器的宽高创建的矩形；

5.	在`ImageOperate.GetImage`函数内部，按照波段信息将图像像素点存入内存，利用`C#`的`BitMap`类转化为`BitMap`数据集；

6.	调用`BitmapToBitmapSource`方法将`BitMap`数据集转化为`BitMapImage`数据集；需要注意的是，如果读取的图像没有波段信息，或者是普通图像，那么读取的直接是`BitMap`数据集而不是`DataSet`数据集，那么就可以直调用`BitmapToBitmapSource`方法；

7.	将转换的`BitMapImage`数据集作为`WPF`图像控件`Image`的`Resource`，实现图像在界面上的加载；

8.	这一模块的主要流程是： 菜单点击事件（打开文件）-> `OpenFileDialog`选择文件 -> `Gdal.Open`获取`DataSet`数据集 -> `System.Drawing.Rectangle` 由图像容器的宽高创建的矩形 -> `dataSet.GetMetadata` 获取图像波段信息 -> 实例化波段选择窗口，用户选择显示波段 -> 根据以上信息使用 `ImageOperate.GetImage` 生成`BitMap`数据集 -> 转化`BitMap`数据集为`BitMapImage`数据集 -> 界面图像控件加载`BitMapImage`数据集；

###2)	界面事件模块###

1.	主要是一些菜单事件，每个事件调取相应的图像处理函数；

2.	这一模块的主要流程是： 菜单点击相应事件 -> 事件驱动相应图像处理函数；

###3)	图像处理模块###

1.	实现灰度拉伸、`HIS`变换、图像平滑、图像锐化、边缘增强、反相等图像增强处理的函数；

2.	灰度拉伸函数接受一个`BitMap`参数，首先使用`pointBitmap`类的`pointBitMap.LockBits`方法对像素点进行锁定，并使用`BitMap`复制一个新的数据集，对复制的数据集进行操作，防止破坏原图像；

3.	对图像的所有像素点灰度值进行遍历，找出灰度值的最大值与最小值；

4.	实例化一个灰度值拉伸窗口，将图像灰度值的范围作为构造参数传入，灰度值拉伸窗口有两个滑动条用来定义新的灰度值范围；

5.	根据新的灰度值范围重新计算图像灰度值并匹配更新；

6.	同理，其他图像处理函数与灰度拉伸函数函数处理流程基本相似，具体代码详见程序文件；

##五、	【测试结果】##

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/1.png)
图1. 软件主界面

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/2.png)
图2. 软件菜单栏展示

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/3.png)
图3. 波段选择

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/4.png)
图4. 加载测试图像1、4、7波段

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/5.png)
图5. 灰度拉伸窗口

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/6.png)
图6. 调整灰度范围

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/7.png)
图7. 缩小灰度范围结果

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/8.png)
图8. HIV变换窗口

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/9.png)
图9. HIV变换窗口

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/10.png)
图10. 边缘提取

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/11.png)
图11. 图像模糊

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/12.png)
图12. 色彩反相

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/13.png)
图13. 自由放缩移动图像

![image](https://github.com/intMinor/RemoteSensingImageProcess/blob/master/Screenshots/14.png)
图14. 项目文件结构


