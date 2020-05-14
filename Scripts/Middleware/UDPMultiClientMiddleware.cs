/*
   Copyright 2020 Jan Kolkmeier <jankolkmeier@gmail.com>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;

#if !UNITY_EDITOR && UNITY_METRO
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Networking;
#else
using System.Net;
using System.Net.Sockets;
#endif


namespace ASAPToolkit.Unity.Middleware {
    public class UDPMultiClientMiddleware : MiddlewareBase {
        public string _remoteIP = "127.0.0.1";
        public int _dataPort = 6652;
        private bool _listening = false;
        private bool _running = true;


#if !UNITY_EDITOR && UNITY_METRO
    private Task _sendTask;
    private Task _listenTask;
    private Task _heartbeatTask;
    private DatagramSocket udpClient;
#else
        private Thread _sendTask;
        private Thread _listenTask;
        private Thread _heartbeatTask;
        private UdpClient udpClient;
#endif

        private void Start() {
            _listening = false;
#if !UNITY_EDITOR && UNITY_METRO
        _sendTask = Task.Run(() => DataSender());
        _listenTask = Task.Run(() => DataListener());
        _heartbeatTask = Task.Run(() => HeartBeat());
#else
            _sendTask = new Thread(DataSender);
            _listenTask = new Thread(DataListener);
            _heartbeatTask = new Thread(HeartBeat);
            _sendTask.Start();
            _listenTask.Start();
            _heartbeatTask.Start();
#endif
        }

        public void OnApplicationQuit() {
            _running = false;
            _listening = false;
            send_MRSTE.Set();
            send_MRSTE.Reset();

            _sendTask.Join(500);
            _listenTask.Join(500);
            _heartbeatTask.Join(500);

            try {
                _sendTask.Abort();
            } catch (Exception) {
                Debug.Log("_sendTask.Abort failed");
                //throw;
            }
            try {
                _listenTask.Abort();
            } catch (Exception) {
                Debug.Log("_listenTask.Abort failed");
                //throw;
            }
            try {
                _heartbeatTask.Abort();
            } catch (Exception) {
                Debug.Log("_heartbeatTask.Abort failed");
                //throw;
            }
            try {
                udpClient.Close();
            } catch (Exception) {
                Debug.Log("udpClient.Close failed");
                //throw;
            }

            Debug.Log("exit of UDPMultiClientMiddleware");

#if !UNITY_EDITOR && UNITY_METRO
#else
            //_sendTask.Join(500);
            //_listenTask.Join(500);
            //_heartbeatTask.Join(500);
#endif
        }

#if !UNITY_EDITOR && UNITY_METRO
    private async Task HeartBeat() {
        while (!_listening) { await Task.Delay(100); }
        while (_running) {
            await Task.Delay(3000);
            if (udpClient == null) continue;
            using (var stream = await udpClient.GetOutputStreamAsync(new HostName(_remoteIP), _dataPort.ToString())) {
                using (var writer = new DataWriter(stream)) {
                    writer.WriteBytes(Encoding.UTF8.GetBytes("{}"));
                    await writer.StoreAsync();
                    //Debug.Log("Heartbeat to : " + _remoteIP + ":" + _dataPort.ToString());
                }
            }

        }
    }
#else
        private void HeartBeat() {
            while (!_listening) { Thread.Sleep(100); }
            while (_running) {
                Thread.Sleep(3000);
                if (udpClient == null) continue;
                udpClient.Send(new byte[] { }, 0, _remoteIP, _dataPort);
            }
        }
#endif

#if !UNITY_EDITOR && UNITY_METRO
    private async Task DataListener() {
        udpClient = new DatagramSocket();
        udpClient.MessageReceived += Listener_MessageReceived;
        try {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            HostName IP = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
                       hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                       == icp.NetworkAdapter.NetworkAdapterId);
            Debug.Log("UDPMultiClientMiddleware asking for socket on device: " + IP.ToString());
            await udpClient.BindEndpointAsync(IP, "0");
            _listening = true;
            Debug.Log("UDPMultiClientMiddleware  listening on " + IP.ToString() + ":" + udpClient.Information.LocalPort);
        } catch (Exception e) {
            Debug.Log("DATA LISTENER START EXCEPTION: "+e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
    }

    private async void Listener_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args) {
        try {
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            StreamReader reader = new StreamReader(streamIn);
            string message = await reader.ReadToEndAsync();
            lock (_receiveQueueLock) {
                _receiveQueue.Enqueue(message);
            }
        } catch (Exception e) {
            Debug.Log("DATA LISTENER EXCEPTION: " + e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
    }
#else
        private void DataListener() {
            
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(localEndpoint);
            udpClient.Client.ReceiveBufferSize = 65507 * 32;
            udpClient.Client.SendBufferSize = 65507 * 32;
            IPAddress listenIP = ((IPEndPoint)udpClient.Client.LocalEndPoint).Address;
            int listenPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

            _listening = true;
            Debug.Log("Client listening on " + listenIP.ToString() + ":" + listenPort);

            while (_running)
            {
                try
                {
                    byte[] buffer = udpClient.Receive(ref localEndpoint);//this is the evil bastard that crashes on an ICMP message 
                    String data = Encoding.ASCII.GetString(buffer);
                    MSG msg = new MSG(data);
                    lock (_receiveQueueLock)
                    {
                        _receiveQueue.Enqueue(msg);
                    }
                }
                catch (Exception e)
                {
                    //OK, since this was a bitch to figure out, here is some documentation for what is going on, and why we encounter exceptions when receiving data

                    //Context: The way our multi-client-udp-middleware works: packets are sent from Client A from any available (random) port to the predefined port of Client B. 
                    //Client B then looks at the packet's originating (random) port that A used to send the packet and adds Client A to the list of clients to broadcast outgoing packets to.
                    //When sending packets back from B to A, it will now use the (random) port as its destination.
                    //As a consequence, here in Unity we must send and receive data with the same UdpClient using the same port.

                    //However, on some windows machines sending data to a port where there is no listener results in an ICMP response WSAECONNRESET 
                    //E.g. see here: https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
                    //(This situation can occur if Unity is started before e.g. ASAP is running; in that case there is nobody listening to our packets)
                    //This ICMP response then silently kind of half-closes our UdpClient connection....
                    //Weirdly enough we can still continue sending data, but when we try to call UdpClient.Receive() it throws an exception --- EVEN IF THERE IS DATA WAITING IN THE BUFFER AT THAT VERY MOMENT.
                    //Even weirdly-er, we can just ignore the exception and continue using the udpClient for sending and receiving...!

                    //The current workaround simply catches the exception and ignores it :)
                    //Unfortunately, the incoming packet that caused the exception is lost
                }

            }
            udpClient.Close();
                
        }
#endif

#if !UNITY_EDITOR && UNITY_METRO
    private async Task DataSender() {
        try {
            while (!_listening) { await Task.Delay(100); }
    
            while (_running) {
                send_MRSTE.WaitOne(100);
    
                bool havePackets = true;
                while (havePackets) {
                    if (udpClient == null) continue;
                    string nextPacket = "";
                    
                    lock (_sendQueueLock) {
                        if (_sendQueue.Count > 0) {
                            nextPacket = _sendQueue.Dequeue();
                        } else {
                            havePackets = false;
                            continue;
                        }
                    }

                    if (nextPacket.Length != 0) {
                        using (var stream = await udpClient.GetOutputStreamAsync(new HostName(_remoteIP), _dataPort.ToString())) {
                            using (var writer = new DataWriter(stream)) {
                                writer.WriteBytes(Encoding.UTF8.GetBytes(nextPacket));
                                await writer.StoreAsync();
                                //Debug.Log("SENT: " + nextPacket + " to : " + _remoteIP + ":" + _dataPort.ToString());
                            }
                        }
                    }
                }
                await Task.Delay(1);
            }
        } catch (Exception e) {
            Debug.Log("DATA SENDER EXCEPTION: " + e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
    }
#else
        private void DataSender() {
            try {
                while (!_listening) { Thread.Sleep(100); }

                while (_running) {
                    bool havePackets = true;
                    while (havePackets) {
                        if (udpClient == null) continue;
                        MSG nextPacket = new MSG("");

                        lock (_sendQueueLock) {
                            if (_sendQueue.Count > 0) {
                                nextPacket = _sendQueue.Dequeue();
                            } else {
                                havePackets = false;
                                continue;
                            }
                        }

                        if (nextPacket.data.Length > 0) {
                            byte[] sendBytes = Encoding.ASCII.GetBytes(nextPacket.data);
                            if (nextPacket.src.Length == 0) {
                                udpClient.Send(sendBytes, sendBytes.Length, _remoteIP, _dataPort);
                            } else {
                                string[] elems = nextPacket.src.Split(':');
                                if (elems.Length != 2) {
                                    Debug.LogError("Can't sent to custom dest: " + nextPacket.src);
                                }
                                string _ip = elems[0];
                                int _port = int.Parse(elems[1]);
                                udpClient.Send(sendBytes, sendBytes.Length, _ip, _port);
                            }
                        }
                    }

                    send_MRSTE.WaitOne(100);
                }
            } catch (Exception e) {
                Debug.Log("DATA SENDER EXCEPTION: " + e.ToString());
                return;
            }
        }
#endif

    }
}