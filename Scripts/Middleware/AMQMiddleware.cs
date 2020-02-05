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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Stomp;

namespace ASAPToolkit.Unity.Middleware {
    public class AMQMiddleware : MiddlewareBase {

        public string URI;
        public string defaultWriteTopic;
        public string[] topicRead;

        Thread amqWriterThread;
        Thread amqReaderThread;

        bool networkOpen;
        ISession session;
        IConnectionFactory factory;
        IConnection connection;
        IMessageProducer producer;
        IDestination destination;

        System.TimeSpan receiveTimeout = System.TimeSpan.FromMilliseconds(250);
        AutoResetEvent semaphore = new AutoResetEvent(false);

        void Start() {
            AMQStart();
        }

        void AMQStart() {
            //GlobalAMQSettings global_AMQ_settings = FindObjectOfType<GlobalAMQSettings>();
            //string address = global_AMQ_settings.GetComponent<GlobalAMQSettings>().address;
            //int port = global_AMQ_settings.GetComponent<GlobalAMQSettings>().port;
            try {
                factory = new NMSConnectionFactory(URI);
                //connection = factory.CreateConnection("admin", "admin");
                connection = factory.CreateConnection();
                Debug.Log("AMQ connecting to "+URI);
                session = connection.CreateSession();
                networkOpen = true;
                connection.Start();
            } catch (System.Exception e) {
                Debug.LogWarning("AMQ Start Exception " + e);
            }

            amqWriterThread = new Thread(new ThreadStart(AMQWriter));
            amqWriterThread.Start();

            amqReaderThread = new Thread(new ThreadStart(AMQReader));
            amqReaderThread.Start();
        }


        void AMQWriter() {
            try {
                IDestination destination_Write = SessionUtil.GetDestination(session, defaultWriteTopic);
                IMessageProducer defaultProducer = session.CreateProducer(destination_Write);
                defaultProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                defaultProducer.RequestTimeout = System.TimeSpan.FromMilliseconds(250);
                while (networkOpen) {
                    MSG msg = new MSG("");
                    lock (_sendQueueLock) {
                        if (_sendQueue.Count > 0) {
                            msg = _sendQueue.Dequeue();
                        }
                    }

                    if (msg.data.Length > 0) {
                        if (msg.src.Length == 0) {
                            defaultProducer.Send(session.CreateTextMessage(msg.data));
                        } else {
                            IMessageProducer producer = session.CreateProducer(SessionUtil.GetDestination(session, msg.src));
                            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                            producer.RequestTimeout = receiveTimeout;
                            producer.Send(session.CreateTextMessage(msg.data));
                        }
                    }
                }
            } catch (System.Exception e) {
                Debug.LogWarning("AMQWriter Exception " + e);
            }
        }

        void AMQReader() {
            IMessageConsumer[] consumers = new IMessageConsumer[topicRead.Length];
            try {
                for (int i = 0; i < topicRead.Length; i++) {
                    ITopic destination_Read = SessionUtil.GetTopic(session, topicRead[i]);
                    consumers[i] = session.CreateConsumer(destination_Read);
                    Debug.Log("AMQ subscribing to " + destination_Read);
                    consumers[i].Listener += new MessageListener(OnAMQMessage);
                }
                while (networkOpen) {
                    semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
                }
            } catch (System.Exception e) {
                Debug.Log("AMQReader Exception " + e);
            }
        }

        void OnAMQMessage(IMessage receivedMsg) {
            lock (_receiveQueueLock) {
                _receiveQueue.Enqueue(new MSG((receivedMsg as ITextMessage).Text, receivedMsg.NMSDestination.ToString()));
            }
            semaphore.Set();
        }


        public void OnApplicationQuit() {
            networkOpen = false;
            if (amqWriterThread != null && !amqWriterThread.Join(500)) {
                Debug.LogWarning("Could not close apolloWriterThread");
                amqWriterThread.Abort();
            }

            if (amqReaderThread != null && !amqReaderThread.Join(500)) {
                Debug.LogWarning("Could not close apolloReaderThread");
                amqReaderThread.Abort();
            }

            if (connection != null) connection.Close();
        }

    }
}