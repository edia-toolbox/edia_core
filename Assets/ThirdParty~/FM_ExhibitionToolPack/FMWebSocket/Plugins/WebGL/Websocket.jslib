mergeInto(LibraryManager.library, {
  WebSocketAddSocketIO: function (_src) {
    var src = Pointer_stringify(_src);
    var sc = document.createElement("script");
    sc.setAttribute("src", src);
    document.head.appendChild(sc);
    console.log(">>>>>>>>> adding~ script");
  },

  WebSocketAddEventListeners: function (_gameobject) {
    var gameobject = Pointer_stringify(_gameobject);
    window.socketEvents = {};
    window.socketEventListener = function(event, data){
        var socketData = { socketEvent: event, eventData: typeof data === 'undefined' ? '' : JSON.stringify(data) };
        try { gameInstance.SendMessage(gameobject, 'InvokeEventCallback', JSON.stringify(socketData)); } catch(e) {}
        try { unityInstance.SendMessage(gameobject, 'InvokeEventCallback', JSON.stringify(socketData)); } catch(e) {}
    };
  },

  WebSocketConnect: function (_src, _gameobject) {
      var src = Pointer_stringify(_src);
      var gameobject = Pointer_stringify(_gameobject);

      window.socketIO = io.connect(src);
      window.socketIO.on('connect', function(){
          try { gameInstance.SendMessage(gameobject, 'OnOpen'); } catch(e) {}
          try { unityInstance.SendMessage(gameobject, 'OnOpen'); } catch(e) {}
          try { gameInstance.SendMessage(gameobject, 'SetSocketID', window.socketIO.io.engine.id); } catch(e) {}
          try { unityInstance.SendMessage(gameobject, 'SetSocketID', window.socketIO.io.engine.id); } catch(e) {}
      });

      //==========================audio==========================
      console.log("before adding Audio!!!!!! Listener");

      var label_aud = 2001;
      var dataID_aud = 0;
      var dataLength_aud = 0;
      var receivedLength_aud = 0;
      var dataByte_aud = new Uint8Array(100);
      var ReadyToGetFrame_aud = true;
      var SourceSampleRate = 44100;
      var SourceChannels = 1;
      var ABuffer = new Float32Array(0);

      var startTime = 0;
      var audioCtx = new AudioContext();

      window.socketIO.on('OnReceiveData', function (data) {
          var _byteData = new Uint8Array(data.DataByte);
          var _label = ByteToInt32(_byteData, 0);
          if (_label == label_aud){
            var _dataID = ByteToInt32(_byteData, 4);
            if (_dataID != dataID_aud) receivedLength_aud = 0;
            dataID_aud = _dataID;
            dataLength_aud = ByteToInt32(_byteData, 8);
            var _GZipMode = (_byteData[16] == 1) ? true : false;
            if (receivedLength_aud == 0) dataByte_aud = new Uint8Array(0);
            receivedLength_aud += _byteData.length - 17;
            dataByte_aud = CombineInt8Array(dataByte_aud, _byteData.slice(17, _byteData.length));
            if (ReadyToGetFrame_aud){
              if (receivedLength_aud == dataLength_aud) ProcessAudioData(dataByte_aud, _GZipMode);
            }
          }
      });

      window.socketIO.on('message', function(data){
        //Receive Regular raw message, events...
        //example on socket.io server: io.emit('message', 'send sth to allllll');
        try { gameInstance.SendMessage(gameobject, 'RegOnMessage', data); } catch(e) {}
        try { unityInstance.SendMessage(gameobject, 'RegOnMessage', data); } catch(e) {}
      });

      function ProcessAudioData(_byte, _GZipMode) {
          if(_GZipMode) return;
          ReadyToGetFrame_aud = false;
          //read meta data
          SourceSampleRate = ByteToInt32(_byte, 0);
          SourceChannels = ByteToInt32(_byte, 4);
          //conver byte[] to float
          var BufferData = _byte.slice(8, _byte.length);
          AudioFloat = new Float32Array(BufferData.buffer);
          if(AudioFloat.length > 0) StreamAudio(SourceChannels, AudioFloat.length, SourceSampleRate, AudioFloat);
          ReadyToGetFrame_aud = true;
      }

      function StreamAudio(NUM_CHANNELS, NUM_SAMPLES, SAMPLE_RATE, AUDIO_CHUNKS) {
          var audioBuffer = audioCtx.createBuffer(NUM_CHANNELS, (NUM_SAMPLES / NUM_CHANNELS), SAMPLE_RATE);
          for (var channel = 0; channel < NUM_CHANNELS; channel++) {
              // This gives us the actual ArrayBuffer that contains the data
              var nowBuffering = audioBuffer.getChannelData(channel);
              for (var i = 0; i < NUM_SAMPLES; i++) {
                  var order = i * NUM_CHANNELS + channel;
                  nowBuffering[i] = AUDIO_CHUNKS[order];
              }
          }
          var source = audioCtx.createBufferSource();
          source.buffer = audioBuffer;
          source.connect(audioCtx.destination);
          source.start(startTime);
          startTime += audioBuffer.duration;
      }
      function CombineInt8Array(a, b) {
          var c = new Int8Array(a.length + b.length);
          c.set(a);
          c.set(b, a.length);
          return c;
      }
      function ByteToInt32(_byte, _offset) {
          return (_byte[_offset] & 255) + ((_byte[_offset + 1] & 255) << 8) + ((_byte[_offset + 2] & 255) << 16) + ((_byte[_offset + 3] & 255) << 24);
      }
      //==========================audio==========================
      for(var socketEvent in window.socketEvents){
          window.socketIO.on(socketEvent, window.socketEvents[socketEvent]);
      }
  },

  WebSocketClose: function () {
    if(typeof window.socketIO !== 'undefined') window.socketIO.disconnect();
  },

  WebSocketEmitEvent: function (_e) {
    var e = Pointer_stringify(_e);
    if(typeof window.socketIO !== 'undefined') window.socketIO.emit(e);
  },

  WebSocketEmitData: function (_e, _data) {
    var e = Pointer_stringify(_e);
    var data = Pointer_stringify(_data);
    var obj = JSON.parse(data);
    if(typeof window.socketIO !== 'undefined') window.socketIO.emit(e, obj);
  },

  WebSocketEmitEventAction: function (_e, _packetId, _gameobject) {
    var e = Pointer_stringify(_e);
    var packetId = Pointer_stringify(_packetId);
    var gameobject = Pointer_stringify(_gameobject);

    if(typeof window.socketIO !== 'undefined')
    {
        window.socketIO.emit(e, function(data){
            var ackData = {
                packetID: packetId,
                data: typeof data === 'undefined' ? '' : JSON.stringify(data)
            };
        });

        try { gameInstance.SendMessage(gameobject, 'InvokeAck', JSON.stringify(ackData)); } catch(e) {}
        try { unityInstance.SendMessage(gameobject, 'InvokeAck', JSON.stringify(ackData)); } catch(e) {}
    }
  },

  WebSocketEmitDataAction: function (_e, _data, _packetId, _gameobject) {
    var e = Pointer_stringify(_e);
    var data = Pointer_stringify(_data);
    var obj = JSON.parse(data);
    var packetId = Pointer_stringify(_packetId);
    var gameobject = Pointer_stringify(_gameobject);

    if(typeof window.socketIO !== 'undefined')
    {
        window.socketIO.emit(e, obj, function(data){
            var ackData = { packetID: packetId, data: typeof data === 'undefined' ? '' : JSON.stringify(data) };
        });

        try { gameInstance.SendMessage(gameobject, 'InvokeAck', JSON.stringify(ackData)); } catch(e) {}
        try { unityInstance.SendMessage(gameobject, 'InvokeAck', JSON.stringify(ackData)); } catch(e) {}
    }
  },

  WebSocketOn: function (_e) {
    var e = Pointer_stringify(_e);
    if(typeof window.socketEvents[e] === 'undefined')
    {
        window.socketEvents[e] = function(data){ window.socketEventListener(e, data); };
        if(typeof window.socketIO !== 'undefined') window.socketIO.on(e, function(data){ window.socketEventListener(e, data); });
    }
  },

  IsSocketIOConnected: function (_gameobject) {
    //socketio
    if(window.socketIO === null) return;
    var gameobject = Pointer_stringify(_gameobject);
    if(window.socketIO.connected === true){
      try { gameInstance.SendMessage(gameobject, 'RegWebSocketConnected'); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegWebSocketConnected'); } catch(e) {}
    } else{
      try { gameInstance.SendMessage(gameobject, 'RegWebSocketDisconnected'); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegWebSocketDisconnected'); } catch(e) {}
    }
  },

  REG_IsWebSocketConnected: function (_gameobject) {
    //without socketIO
    if(websocket === null) return;
    var gameobject = Pointer_stringify(_gameobject);
    if(websocket.readyState === WebSocket.OPEN || websocket.readyState === WebSocket.CLOSING){
      try { gameInstance.SendMessage(gameobject, 'RegWebSocketConnected'); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegWebSocketConnected'); } catch(e) {}
    } else{
      try { gameInstance.SendMessage(gameobject, 'RegWebSocketDisconnected'); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegWebSocketDisconnected'); } catch(e) {}
    }
  },

  REG_WebSocketAddEventListeners: function (_src, _gameobject) {
    var src = Pointer_stringify(_src);
    var gameobject = Pointer_stringify(_gameobject);

    websocket = new WebSocket(src);
    websocket.onopen = function(evt) { onOpen(evt) };
    websocket.onclose = function(evt) { onClose(evt) };
    websocket.onmessage = function(evt) { onMessage(evt) };
    websocket.onerror = function(evt) { onError(evt) };

    function onOpen(evt){
      if(websocket === null) return;
      if(websocket.readyState === WebSocket.OPEN){
        try { gameInstance.SendMessage(gameobject, 'RegOnOpen'); } catch(e) {}
        try { unityInstance.SendMessage(gameobject, 'RegOnOpen'); } catch(e) {}
      }
    }
    function onClose(evt){
      if(websocket === null) return;
      try { gameInstance.SendMessage(gameobject, 'RegOnClose'); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegOnClose'); } catch(e) {}
      websocket = null;
    }
    function onMessage(evt){
      if(websocket === null) return;
      try { gameInstance.SendMessage(gameobject, 'RegOnMessage', evt.data); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegOnMessage', evt.data); } catch(e) {}
    }
    function onError(evt){
      if(websocket === null) return;
      try { gameInstance.SendMessage(gameobject, 'RegOnError', evt.data); } catch(e) {}
      try { unityInstance.SendMessage(gameobject, 'RegOnError', evt.data); } catch(e) {}
    }
  },

  REG_Close: function (_e, _data) {
    if(websocket === null) return;
    if(websocket.readyState === WebSocket.OPEN) websocket.close();
  },

  REG_Send: function(_e){
    if(websocket === null) return;
    if(websocket.readyState === WebSocket.OPEN){
      var e = Pointer_stringify(_e);
      websocket.send(e);
    }
  },

});
