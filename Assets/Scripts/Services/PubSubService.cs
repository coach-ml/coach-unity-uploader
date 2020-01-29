using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

namespace ReactUnity.Services
{
    public interface IPubSubService : IService
    {
        Subscription Subscribe(string topic, Subscription.EventReceivedDelegate listener);
        Subscription Subscribe(string topic);
    }

    class PubSubService : IPubSubService
    {
        public static readonly string SERVER_URL = "localhost:52964";
        private Dictionary<string, TopicHandler> _groups;

        PubSubService()
        {
            _groups = new Dictionary<string, TopicHandler>();
        }

        public Subscription Subscribe(string topic, Subscription.EventReceivedDelegate listener)
        {
            var sub = Subscribe(topic);
            sub.OnEventRecieved += listener;
            return sub;
        }

        public Subscription Subscribe(string topic)
        {
            if (!_groups.ContainsKey(topic))
            {
                _groups[topic] = new TopicHandler(topic);
            }
            return _groups[topic].AddSubscription();
        }
    }

    public class TopicHandler
    {
        private List<Subscription> _subscriptions;
        private string _topic;
        private WebSocket _webSocket;
        private int reconnectAttempts;

        public TopicHandler(string topic)
        {
            _subscriptions = new List<Subscription>();
            _topic = topic;
        }

        public Subscription AddSubscription()
        {
            if (_webSocket == null)
            {
                Connect();
            }
            var sub = new Subscription(this);
            lock (_subscriptions)
            {
                _subscriptions.Add(sub);
            }
            return sub;
        }

        public void RemoveSubscription(Subscription sub)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(sub);
            }

            if (_subscriptions.Count == 0)
            {
                Disconnect();
            }
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            if(e.IsText)
            {
                ForEachSubscription(s => s.Event(_topic, e.Data));
            }
        }

        private void Connect()
        {
            _webSocket = new WebSocket("ws://" + PubSubService.SERVER_URL + "/PubSub/Subscribe?topic=" + _topic);
            _webSocket.OnMessage += Socket_OnMessage;
            _webSocket.OnClose += Socket_OnClose;
            _webSocket.OnError += _webSocket_OnError;
            _webSocket.OnOpen += _webSocket_OnOpen;

            _webSocket.Connect();
        }

        private void _webSocket_OnOpen(object sender, EventArgs e)
        {
            reconnectAttempts = 0;
        }

        private void _webSocket_OnError(object sender, ErrorEventArgs e)
        {
            Debug.Log("Websocket Failed");
            ForEachSubscription(s => s.Error());
            Reconnect();
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log("Websocket Closed");
            ForEachSubscription(s => s.Error());
            Reconnect();
        }

        private void Reconnect()
        {
            Disconnect();
            reconnectAttempts++;
            if (reconnectAttempts > 5)
            {
                Debug.Log("Connection lost");
                ForEachSubscription(s => s.Lost());
            }
            else
            {
                Debug.Log("Reconnection attempt " + reconnectAttempts);
                Connect();
            }
        }

        private void Disconnect()
        {
            if(_webSocket != null)
            {
                if(_webSocket.ReadyState != WebSocketState.Closed && _webSocket.ReadyState != WebSocketState.Closing)
                {
                    _webSocket.Close();
                }
                _webSocket = null;
            }
        }

        private void ForEachSubscription(Action<Subscription> action)
        {
            List<Subscription> subCopy;
            lock (_subscriptions)
            {
                subCopy = new List<Subscription>(_subscriptions);
            }
            foreach (var s in subCopy)
            {
                action(s);
            }
        }
    }

    public class Subscription
    {
        public delegate void EventReceivedDelegate(string topic, string value);
        public delegate void SubscriptionErrorDelegate();
        public delegate void SubscriptionLostDelegate();

        public event EventReceivedDelegate OnEventRecieved;
        public event SubscriptionErrorDelegate OnSubscriptionError;
        public event SubscriptionLostDelegate OnSubscriptionLost;

        private TopicHandler _group;

        public Subscription(TopicHandler group)
        {
            _group = group;
        }

        public void Error()
        {
            OnSubscriptionError?.Invoke();
        }

        public void Lost()
        {
            OnSubscriptionLost?.Invoke();
        }

        public void Event(string topic, string value)
        {
            OnEventRecieved?.Invoke(topic, value);
        }

        public void Unsubscribe()
        {
            _group.RemoveSubscription(this);
        }
    }
}
