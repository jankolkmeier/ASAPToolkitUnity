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

#if !UNITY_EDITOR && UNITY_METRO
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Networking;
using System.Threading.Tasks;
#else
using System.Net;
using System.Net.Sockets;
using System.Threading;
#endif


namespace ASAPToolkit.Unity.Middleware {
    public class UDPMiddleware : MiddlewareBase {

        public string _asapIP = "127.0.0.1";
        public int _sendPort = 6672;
        public int _listenPort = 6673;
        private bool _running = true;

        private bool _listening;

#if !UNITY_EDITOR && UNITY_METRO
    private DatagramSocket udpReceiveSocket;
#else
        private UdpClient udpClient;
        private Thread listenThread;
        private Thread sendThread;
#endif

        public void Start() {
            _listening = false;
#if !UNITY_EDITOR && UNITY_METRO
        Task.Run(() => DataSender());
        Task.Run(() => DataListener());
#else
            listenThread = new Thread(DataListener);
            sendThread = new Thread(DataSender);
            listenThread.Start();
            sendThread.Start();
#endif
        }

        private void OnApplicationQuit() {
            _running = false;
            if (udpClient != null) udpClient.Close();
            listenThread.Join(500);
            sendThread.Join(500);
        }

#if !UNITY_EDITOR && UNITY_METRO
    private async Task DataListener() {
        udpReceiveSocket = new DatagramSocket();
        udpReceiveSocket.MessageReceived += Listener_MessageReceived;
        try {
            await udpReceiveSocket.BindServiceNameAsync(_listenPort+"");
        } catch (Exception e) {
            Debug.Log("DATA LISTENER START EXCEPTION: "+e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
        Debug.Log("UDPMiddleware listening on " + udpReceiveSocket.Information.LocalPort);
        _listening = true;
    }
#else
        private void DataListener() {
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, _listenPort);
            udpClient = new UdpClient(localEndpoint);
            _listening = true;
            while (_running) {

                byte[] buffer = udpClient.Receive(ref remoteEndpoint);
                lock (_receiveQueueLock) {
                    _receiveQueue.Enqueue(new MSG(Encoding.ASCII.GetString(buffer), remoteEndpoint.ToString()));
                }
            }
            udpClient.Close();
        }
#endif

#if !UNITY_EDITOR && UNITY_METRO
    private async void Listener_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args) {
        Debug.Log("DATA!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: ");
        try {
            //Read the message that was received from the UDP echo client.
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            StreamReader reader = new StreamReader(streamIn);
            string message = await reader.ReadToEndAsync();
            lock (_receiveQueueLock) {
                Debug.Log("REC: " + message);
                _receiveQueue.Enqueue(message);
            }
        } catch (Exception e) {
            Debug.Log("DATA LISTENER EXCEPTION: " + e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }
    }
#endif

#if !UNITY_EDITOR && UNITY_METRO
    private async Task DataSender() {
        try {
            DatagramSocket udpSocket = new DatagramSocket();
            await udpSocket.ConnectAsync(new HostName(_asapIP), _sendPort.ToString());
            DataWriter udpWriter = new DataWriter(udpSocket.OutputStream);
            while (!_listening) await Task.Delay(100);
            while (_running) {
                string nextPacket = "";
                lock (_sendQueueLock) {
                    if (_sendQueue.Count > 0) {
                        nextPacket = _sendQueue.Dequeue();
                    }
                }

                if (nextPacket.Length != 0) {
                    udpWriter.WriteString(nextPacket);
                    await udpWriter.StoreAsync();
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
                while (!_listening) Thread.Sleep(100);
                while (_running) {
                    MSG nextPacket = new MSG("");
                    lock (_sendQueueLock) {
                        if (_sendQueue.Count > 0) {
                            nextPacket = _sendQueue.Dequeue();
                        }
                    }

                    if (nextPacket.data.Length > 0) {
                        byte[] sendBytes = Encoding.ASCII.GetBytes(nextPacket.data);
                        udpClient.Send(sendBytes, sendBytes.Length, _asapIP, _sendPort);
                    }
                    Thread.Sleep(1);
                }
            } catch (Exception e) {
                Debug.Log("DATA SENDER EXCEPTION: " + e.ToString());
                return;
            }
        }
#endif

    }
}