
//哇

var deg2rad = Math.PI / 180;

function getQueryVariable(name, defaultValue) {
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if(pair[0] == name){
            return pair[1];
        }
    }
    return(defaultValue);
}

function normalAngle(degrees){
    while (degrees > 180.0){
        degrees -= 360.0;
    }
    while (degrees <= -180.0){
        degrees += 360.0;
    }
    return degrees;
}

function clamp(num, min, max){
    if (min > max) return clamp(num, max, min);
    if (num < min) return min;
    if (num > max) return max;
    return num;
}
// -1~1 to short coding
function float2uint8(f){
//    var arrFloat = new Float32Array(floats);
//    var arrInt = new Uint8Array(arrFloat.buffer);
//    var res = [];
//    arrInt.forEach(i => res.push(i));
//    return res;
    var shortInt = clamp(Math.round(f * 32767), -32767, 32767);
    var arrShort = new Int16Array([shortInt]);
    var arrInt = new Uint8Array(arrShort.buffer);
    var res = [];
    arrInt.forEach(i => res.push(i));
    return res;
}

function bits2uint8(bits){
    var res = 0;
    if (bits instanceof Array){
        bits.forEach((b, i) => {
            if (b){
                res += 1 << i;
            }
        });
    }
    return res;
}

function joinArray(){
    var res = [];
    for (var i = 0; i < arguments.length; i++){
        if (arguments[i] instanceof Array){
            arguments[i].forEach(item => res.push(item));
        }
    }
    return res;
}

function checkAddress(address){
    if (typeof(address) != "string") return 'No address specified.';
    var parts = address.split(':');
    if (parts.length < 2) return 'Invalid syntax.';
    if (parts.length > 2) return 'Invalid syntax.';
    var ip = parts[0].split('.');
    if (ip.length != 4) return 'Invalid IP syntax.';
    for (var i = 0; i < 4; i++){
        ip[i] = parseInt(ip[i]);
        if (isNaN(ip[i])) return  'Invalid IP syntax.';
    }
    var port = parseInt(parts[1]);
    if (isNaN(port) || port < 1 || port > 65535) return 'Invalid port';
    
    if ((ip[0] == 10) || (ip[0] == 172 && ip[1] >= 16 && ip[1] <= 31) || (ip[0] == 192 && ip[1] == 168)) {
        return false;
    }
    else{
        return 'Not a local IP address.';
    }
    
}

//没有合法性检验，假设传入的参数都是合法的
function buildLayout(str){
    var mark = (name, inner, extra) => `<${name}${extra ? (' ' + extra) : ''}>${inner}</${name}>`;
    var joystick = function(width, height, nameX, nameY, jId){
                    console.log(jId);
        var left = -(width - 1.0) / 2.0;
        var top = -(height - 1.0) / 2.0;
        var style = `width:${width*100}%;height:${height*100}%;left:${left*100}%;top:${top*100}%;`;
        return `<div jid="${jId}" namex="${nameX}" namey="${nameY}" class="joystick" style="${style}"><div class="joystick-handle"></div></div>`
    }
    var parseMark = function(str){
        var segments = str.split('-');
        try{
            switch (segments[0]){
                case 'j0':
                    var nameX = segments[1], nameY = segments[2];
                    return `<button id="calibrate" class="joystick" jid="0" namex="${nameX}" namey="${nameY}">calibrate</button>`
                default:
                    switch (segments[0][0]){
                        case 'b':// button
                            var bId = parseInt(segments[0].substr(1)) - 1;
                            var alias = (!segments[1]) ? (bId + 1) : segments[1];
                            return `<button class="${segments[2] == 'h' ? 'remote-btn-hold' : 'remote-btn'}" bid="${bId}">${alias}</button>`
                        case 'k':// keyboard
                            var keyname = segments[1];
                            var alias = (!segments[2]) ? keyname : segments[2];
                            return `<button class="remote-key" keyname="${keyname}">${alias}</button>`
                        case 'j':// joystick
                            var jId = parseInt(segments[0].substr(1));
                            var size = ((segments[3] == null) ? '3x3' : segments[3]).split('x');
                            var sizeX = parseFloat(size[0]), sizeY = parseFloat(size[1]);
                            var nameX = segments[1], nameY = segments[2];
                            return joystick(sizeX, sizeY, nameX, nameY, jId);
                        default:
                            if (str.trim() == '') return '';
                            return 'Invalid string : ' + str;
                    }
            }
        }
        catch(e){
            return 'Error : ' + e.toString();
        }
    }
    var table = function(content){
        var res = '';
        for (var i = 0; i < content.length; i++){
            var resd = '';
            for (var j = 0; j < content[i].length; j++){
                resd += mark('td', content[i][j], `style="width:${100 / content[0].length}%;"`);
            }
            res += mark('tr', resd, `style="height:${100 / content.length}%;"`);
        }
        return mark('table', res);
    }
    
    var rows = str.split('|');
    var sizestr = rows.splice(0, 1)[0];
    var [colNum, rowNum] = sizestr.split('x');
    colNum = parseInt(colNum);
    rowNum = parseInt(rowNum);
    var contentArray = [];
    for (var i = 0; i < rowNum; i++){
        var rowArray = [];
        var cols = rows[i].split(',');
        for (var j = 0; j < colNum; j++){
            rowArray.push(parseMark(cols[j]));
        }
        contentArray.push(rowArray);
    }
    return table(contentArray);
}

controlBuffer = {
    buttons : [],
    buttonsFlip : [],
    keybuttonsQueue : [],
    //joysticks : [{x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}, {x : 0, y : 0}],
    axes : [0, 0, 0, 0, 0, 0, 0, 0],
    axesList : ['x', 'y', 'z', 'rx', 'ry', 'rz', 'sl0', 'sl1'],
    joystickGoto : function(x, y, jId){
        x = clamp(x, -1, 1);
        y = clamp(y, -1, 1);
        var joystick = $(`.joystick[jid=${jId}]`);
        var handle = joystick.children(0);
        var shiftedX = (x + 1) / 2, shiftedY = (y + 1) / 2;
        handle.css('left', (shiftedX * joystick.width() - handle.width() / 2) + 'px').css('top', (shiftedY * joystick.height() - handle.height() / 2) + 'px');
        var indexX = this.axesList.indexOf(joystick.attr('namex'))
        var indexY = this.axesList.indexOf(joystick.attr('namey'));
        if (indexX != -1) this.axes[indexX] = x;
        if (indexY != -1) this.axes[indexY] = y;
        //this.joysticks[jId].x = x;
        //this.joysticks[jId].y = y;
    },
    buttonGoto : function(b, bId){
        b = !!b;
        var button = $(`button[bid=${bId}]`);
        if (b) {
            button.addClass('pressed');
        }
        else {
            button.removeClass('pressed');
        }
        if (this.buttons[bId] ^ b){
            this.buttons[bId] = b;
            this.buttonsFlip[bId]++;
        }
    },
    keyUp : function(name){
        var button = $(`button[keyname=${name}]`);
        button.removeClass('pressed');
        var cmd = 'u ' + name;
        if (this.keybuttonsQueue[this.keybuttonsQueue.length - 1] != cmd){
            this.keybuttonsQueue.push(cmd);
        }
    },
    keyDown : function(name){
        var button = $(`button[keyname=${name}]`);
        button.addClass('pressed');
        var cmd = 'd ' + name;
        if (this.keybuttonsQueue[this.keybuttonsQueue.length - 1] != cmd){
            this.keybuttonsQueue.push(cmd);
        }
    },
    start : function(){
        var that = this;
        $('.remote-btn').on('pointerdown', function(e){
            var bId = parseInt(e.target.getAttribute('bid'));
            that.buttonGoto(true, bId);
        }).on('pointerup pointerleave', function(e){
            var bId = parseInt(e.target.getAttribute('bid'));
            that.buttonGoto(false, bId);
        });
        $('.remote-btn-hold').on('pointerdown', function(e){
            var bId = parseInt(e.target.getAttribute('bid'));
            that.buttonGoto(!that.buttons[bId], bId);
        })
        $('.remote-key').on('pointerdown', function(e){
            var keyname = (e.target.getAttribute('keyname'));
            that.keyDown(keyname);
        }).on('pointerup pointerleave', function(e){
            var keyname = (e.target.getAttribute('keyname'));
            that.keyUp(keyname);
        });
        $('.joystick').on('pointerdown pointermove', function(e){
            var jId = parseInt(e.target.getAttribute('jid'));
            var self = $(e.target);
            var shiftedX = e.offsetX / self.width(), shiftedY = e.offsetY / self.height();
            that.joystickGoto(shiftedX * 2 - 1, shiftedY * 2 - 1, jId);
            
        }).on('pointerup pointerleave', function(e){
            var jId = parseInt(e.target.getAttribute('jid'));
            that.joystickGoto(0, 0, jId);
        });
        for (var i = 0; i < 16; i++){
            this.buttons[i] = false;
            this.buttonsFlip[i] = 0;
            //this.buttonGoto(false, i);
        }
        for (var i = 0; i < 3; i++){
            this.joystickGoto(0, 0, i);
        }
    },
}


socket = {
    hasConnection : false,
    start : function(serverAddress){
        if (this.hasConnection) return;
        var that = this;
        var ws = new WebSocket('ws://' + serverAddress);
        ws.onopen = function(){
            that.hasConnection = true;
        };
        ws.onmessage = function(e) {
            console.log("message received : " + e.data);
        };
        ws.onerror = function(e) {
            console.log('socket error : ', e);
            that.hasConnection = false;
        };
        this.ws = ws;
    },
    startInterval : function(serverAddress){
        var that = this;
        setInterval(function(){
            if (!that.hasConnection){
                try{
                    that.start(serverAddress);
                }
                catch(e){
                    console.log('restart error : ', e);
                }
            }
        }, 1000);
    },
    send : function(sth){
        if (this.hasConnection){
            this.ws.send(sth);
        }
    }
}

gyro = {
    q : new Quaternion(),
    qCal : new Quaternion(),
    getDiff : function(){
        return this.qCal.inverse().mul(this.q);
    },
    calibrate : function(){
        this.qCal = this.q;
    },
    start : function(onupdate){
        var that = this;
        function handleOrientation(event) {
            //gamma = event.gamma; beta = event.beta;
            
            var q = Quaternion.fromEuler(event.alpha * deg2rad, event.beta * deg2rad, event.gamma * deg2rad, 'ZXY');
            that.q = q;
            
            var diff = that.getDiff();
            var dir = diff.rotateVector([1, 0, 0]);
            var roll = Math.atan2(dir[1], dir[0]);
            var pitch = Math.atan(dir[2] / Math.sqrt(dir[0]**2 + dir[1]**2));
            
            var res = "alpha: " + event.alpha + "\n";
            res += "beta : " + event.beta + "\n";
            res += "gamma: " + event.gamma + "\n";
            res += "roll : " + (roll / deg2rad) + "°\n";
            res += "pitch: " + (pitch / deg2rad) + "°\n";
            $('#info').html(res);
            $('#warning').html('');
            onupdate(pitch, roll);
        }
        window.addEventListener('deviceorientation', handleOrientation);
    }
}


var serverAddress = getQueryVariable('s', null);
var problem = checkAddress(serverAddress);
if (!problem){
    if (!confirm('Please confirm your PC\'s local ip address: ' + serverAddress)){
        serverAddress = null;
    }
}
while((problem = checkAddress(serverAddress))){
    serverAddress = prompt(problem + ' Please enter a valid address here:', '');
    if (serverAddress == null){
        break;
    }
}
var layout = getQueryVariable('l', null);
if (layout === null) { 
    layout = '5x8|j0-x-y,,,,b1|,k-a,k-q,,b2|,k-s,k-w,,b3|,k-d,k-e,,b4|,,,,b5|,,,,b6|,,j1-ry-rx,,b7|,,,,b8';
}
else { 
    layout = decodeURIComponent(layout); 
}
$('#debug').html('server: ' + serverAddress);
try{
    $('#frame').append(buildLayout(layout));
}
catch(e){
    $('#warning').html('Can not parse layout');
}

$(document).on('click', '#calibrate', function(e){
    gyro.calibrate();
});





controlBuffer.start();
gyro.start((pitch, roll) => {
    rightAxis = clamp(-roll / (90.0 * deg2rad), -1.0, 1.0);
    pushAxis = clamp(pitch / (70.0 * deg2rad), -1.0, 1.0);
    controlBuffer.joystickGoto(rightAxis, pushAxis, 0);
});
socket.start(serverAddress);

setInterval(function(){
    var buttonsFlipped = [];
    for (var i = 0; i < controlBuffer.buttons.length; i++){
        if (controlBuffer.buttonsFlip[i] > 0){
            controlBuffer.buttonsFlip[i]--;
        }
        buttonsFlipped.push(controlBuffer.buttons[i] ^ (controlBuffer.buttonsFlip[i] % 2));
    }
    var data = joinArray(
        float2uint8(controlBuffer.axes[0]), float2uint8(controlBuffer.axes[1]), 
        float2uint8(controlBuffer.axes[2]), float2uint8(controlBuffer.axes[3]),
        float2uint8(controlBuffer.axes[4]), float2uint8(controlBuffer.axes[5]),
        float2uint8(controlBuffer.axes[6]), float2uint8(controlBuffer.axes[7]),
        [bits2uint8(buttonsFlipped.slice(0, 8)), bits2uint8(buttonsFlipped.slice(8, 16))],
    );
    try{
        socket.send(new Uint8Array(data));
        if (controlBuffer.keybuttonsQueue.length > 0){
            socket.send(controlBuffer.keybuttonsQueue.shift());
        }
    }
    catch(e){
        console.log('error when sending socket');
        console.log(e);
    }
}, 20);

