using System;

namespace Code.Core.Events.Bus
{
    public struct HitStopEvent : IEvent
    {
        public float Duration;
        public float TimeScale;

        public HitStopEvent(float duration, float timeScale = 0f)
        {
            Duration = duration;
            TimeScale = timeScale;
        }
    }
}