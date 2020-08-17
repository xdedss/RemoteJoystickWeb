[中文](readme_cn.md)

# Usage

Use smartphone to control PC. Emulates keyboard or joystick input.

# Requirement

[vJoy]([http://vjoystick.sourceforge.net/site/index.php/download-a-install/download](http://vjoystick.sourceforge.net/site/index.php/download-a-install/download) (Joystick emulator)

# Compatibility

Doesn't work on IOS (I have no idea why)

Works on Android Chrome

# Run

Download from [Releases](https://github.com/xdedss/RemoteJoystickWeb/releases)

There are two options:

- Easy to use, but doesn't support device orientation control.

## 1.http

```powershell
.\RemoteJoystickWeb.exe example.txt
```

This will open two ports, 8000 and 8001. 

localhost:8000 is a HTTP server. It serves a webpage which renders UI on smartphone and sends inputs to PC. 

localhost:8001 is a websocket server. It receives the input sent by the smartphone and emulates keyboard events or joystick input on you PC.

A QR code will be generated with your local address and layout information(in ./layout/example.txt). Scan it with your smartphone and open the link with a browser.  The link will also be printed into the console so you can also manually type it into your browser.

## 2.https

- Supports device orientation control, but need to allow insecure origins in browser settings.

Most browsers will disable deviceorientation event if the page is insecure(http) so if you want to use device orientation as input axes you need a https page.

So I put the webpage on [github pages](https://xdedss.github.io/RemoteJoystickWeb/RemoteJoystickWeb/www/) . Run this to replace the local server with https link:

```powershell
.\RemoteJoystickWeb.exe example.txt https://xdedss.github.io/RemoteJoystickWeb/RemoteJoystickWeb/www/
```

However, our websocket server is still insecure and sending websocket from a https page to an insecure server is not allowed. To fix this, you need to allow insecure origins in browser settings. If you are using android chrome, goto chrome://flags and find "insecure origins treated as secure", add the address of the socket server.

# Edit Layout

see ./layout/example.txt
