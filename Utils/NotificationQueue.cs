using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OuterWildsRPG.Utils
{
    public class NotificationQueue
    {
        public Queue<Notification> Queue = new();
        public int ConcurrentLimit = 1;

        List<Notification> active = new();

        static List<NotificationQueue> allQueues = new();

        public NotificationQueue()
        {
            allQueues.Add(this);
        }

        public Notification Enqueue(Notification notification)
        {
            Queue.Enqueue(notification);
            return notification;
        }

        public Notification Enqueue(string msg, PromptPosition pos = PromptPosition.Center, float time = 5f, AudioType sound = AudioType.None, AudioClip clip = null, Action onDisplay = null, Action onExpire = null)
        {
            return Enqueue(new Notification()
            {
                Message = msg,
                Position = pos,
                Duration = time,
                Sound = sound,
                Clip = clip,
                OnDisplay = onDisplay,
                OnExpire = onExpire,
            });
        }

        public void Update()
        {
            while (Queue.Count > 0 && active.Count < ConcurrentLimit)
            {
                var n = Queue.Dequeue();
                var prompt = new ScreenPrompt(n.Message);
                Locator.GetPromptManager().AddScreenPrompt(prompt, n.Position, true);
                if (n.Sound != AudioType.None)
                    Locator.GetPlayerAudioController().PlayOneShotInternal(n.Sound);
                if (n.Clip != null)
                    Locator.GetPlayerAudioController()._oneShotSource.PlayOneShot(n.Clip);
                active.Add(n);
                n.OnDisplay?.Invoke();
                UnityUtils.RunAfterSeconds(n.Duration, () =>
                {
                    Locator.GetPromptManager().RemoveScreenPrompt(prompt, n.Position);
                    active.Remove(n);
                    n.OnExpire?.Invoke();
                });
            }
        }

        public static void UpdateAll()
        {
            foreach (var queue in allQueues) queue.Update();
        }

        public class Notification
        {
            public string Message;
            public PromptPosition Position;
            public float Duration;
            public AudioType Sound;
            public AudioClip Clip;
            public Action OnDisplay;
            public Action OnExpire;
        }
    }
}
