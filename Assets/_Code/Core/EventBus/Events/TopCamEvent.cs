using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct TopCamEvent : IEvent
    {
        public GameObject cam;

        public TopCamEvent(GameObject cam)
        {
            this.cam = cam;
        }
    }
}