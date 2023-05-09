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
        public PromptPosition Position = PromptPosition.Center;
        public float Time = 3f;
        public AudioType Sound = AudioType.None;
        public AudioClip Clip = null;

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

        public Notification Enqueue(string msg, PromptPosition? pos = null, float? time = null, AudioType? sound = null, AudioClip clip = null, Action onDisplay = null, Action onExpire = null)
        {
            return Enqueue(new Notification()
            {
                Message = msg,
                Position = pos ?? Position,
                Duration = time ?? Time,
                Sound = sound ?? Sound,
                Clip = clip ?? Clip,
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
