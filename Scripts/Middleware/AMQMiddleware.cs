using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Apache.NMS;
using Apache.NMS.Util;
using Apache.NMS.Stomp;

namespace ASAPToolkit.Unity.Middleware {
    public class AMQMiddleware : MiddlewareBase {

        public int port;
        public string address;
        public string topicWrite;
        public string topicRead;

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

        void Awake() {
            AMQStart();
        }

        void Start() {

        }

        void Update() {

        }

        void AMQStart() {
            //GlobalAMQSettings global_AMQ_settings = FindObjectOfType<GlobalAMQSettings>();
            //string address = global_AMQ_settings.GetComponent<GlobalAMQSettings>().address;
            //int port = global_AMQ_settings.GetComponent<GlobalAMQSettings>().port;
            try {
                factory = new ConnectionFactory("tcp://" + address + ":" + port.ToString());
                connection = factory.CreateConnection("admin", "admin");
                Debug.Log("AMQ connecting to tcp://" + address + ":" + port.ToString());
                session = connection.CreateSession();
                networkOpen = true;
                connection.Start();
            } catch (System.Exception e) {
                Debug.Log("AMQ Start Exception " + e);
            }

            amqWriterThread = new Thread(new ThreadStart(AMQWriter));
            amqWriterThread.Start();

            amqReaderThread = new Thread(new ThreadStart(AMQReader));
            amqReaderThread.Start();
        }


        void AMQWriter() {
            try {
                IDestination destination_Write = SessionUtil.GetDestination(session, topicWrite);
                IMessageProducer producer = session.CreateProducer(destination_Write);
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                producer.RequestTimeout = System.TimeSpan.FromMilliseconds(250);
                while (networkOpen) {
                    string msg = "";
                    lock (_sendQueueLock) {
                        if (_sendQueue.Count > 0) {
                            msg = _sendQueue.Dequeue();
                        }
                    }

                    if (msg != null && msg.Length > 0) {

                        producer.Send(session.CreateTextMessage(msg));
                    }
                }
            } catch (System.Exception e) {
                Debug.Log("AMQWriter Exception " + e);
            }
        }

        void AMQReader() {
            try {
                ITopic destination_Read = SessionUtil.GetTopic(session, topicRead);
                IMessageConsumer consumer = session.CreateConsumer(destination_Read);
                Debug.Log("AMQ subscribing to " + destination_Read);
                consumer.Listener += new MessageListener(OnAMQMessage);
                while (networkOpen) {
                    semaphore.WaitOne((int)receiveTimeout.TotalMilliseconds, true);
                }
            } catch (System.Exception e) {
                Debug.Log("AMQReader Exception " + e);
            }
        }

        void OnAMQMessage(IMessage receivedMsg) {
            lock (_receiveQueueLock) {
                _receiveQueue.Enqueue((receivedMsg as ITextMessage).Text);
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