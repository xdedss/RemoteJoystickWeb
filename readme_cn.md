[English](readme.md)

# 用途

用手机控制电脑，可以模拟键盘或摇杆输入

# 依赖

[vJoy](http://vjoystick.sourceforge.net/site/index.php/download-a-install/download) (模拟摇杆)

# 兼容性

IOS不行

安卓 彳亍

# 运行

下载： [Releases](https://github.com/xdedss/RemoteJoystickWeb/releases)

## 1.使用HTTP页面

- 这种情况下大部分浏览器认为http不安全，所以不支持姿态感应（但是神奇的是用微信内置浏览器似乎可以）

```powershell
.\RemoteJoystickWeb.exe example.txt
```

localhost:8000 会运行一个HTTP服务器，上面放着一个网页作为手机端的客户端。

localhost:8001 是socket服务器，接收手机发来的操作数据并且模拟出键盘和摇杆的操作。

之后会弹出一个二维码，内容是HTTP服务器的内网IP链接，顺带socket服务器的内网链接以及UI配置文件会作为get参数加进去。

参数里的example.txt就是手机端的UI配置文件，表示./layout/example.txt

## 2.使用HTTPS页面

- 这种情况下可以使用姿态感应

```powershell
.\RemoteJoystickWeb.exe example.txt https://xdedss.github.io/RemoteJoystickWeb/RemoteJoystickWeb/www/
```

但是socket服务器依然是insecure的，所以需要设置允许https页面中的不安全内容

如果用的是安卓chrome，在 chrome://flags 里面找 "insecure origins treated as secure" 然后把socket服务器的地址加进去。

# 自定义UI

see ./layout/example.txt
