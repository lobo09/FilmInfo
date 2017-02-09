using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmInfo.Utility
{
    public class Messenger
    {
        private readonly Dictionary<MessengerKey, object> subscriberDictionary = new Dictionary<MessengerKey, object>();
        private static Messenger messengerInstance;

        private Messenger()
        {
        }

        public static Messenger Default
        {
            get
            {
                if (messengerInstance == null)
                    messengerInstance = new Messenger();

                return messengerInstance;
            }
        }

        public void Register<T>(object subscriber, Action<T> action)
        {
            subscriberDictionary.Add(new MessengerKey(subscriber), action);
        }

        public void Unregister(object subscriber)
        {
            foreach (var item in subscriberDictionary.Where(i => i.Key.Subscriber == subscriber).Select(i => i.Key))
            {
                subscriberDictionary.Remove(item);
            }
        }

        public void Send<T>(T message)
        {
            foreach (var item in subscriberDictionary.Select(i => i.Value).OfType<Action<T>>())
            {
                item(message);
            }
        }
    }

    public class MessengerKey
    {
        public object Subscriber { get; private set; }

        public MessengerKey(object subscriber)
        {
            Subscriber = subscriber;
        }
    }
}
