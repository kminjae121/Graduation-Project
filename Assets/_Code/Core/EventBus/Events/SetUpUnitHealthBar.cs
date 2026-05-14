using UnityEngine;

namespace Code.Core.Events.Bus
{
    public struct SetUpUnitHealthBar : IEvent
    {
        public Sprite unitImage;
        public int unitCount;

        public float currentValue;
        
        public float maxValue;

        public float finalValue;
        
        public SetUpUnitHealthBar(int unitCount, float currentValue, float maxValue, Sprite unitImage)
        {
            this.unitCount = unitCount;
            this.currentValue = currentValue;
            this.maxValue = maxValue;
            this.unitImage = unitImage;

            if (maxValue <= 0f)
            {
                finalValue = 0f; 
                return;
            }
            
            float clampedCurrent = Mathf.Clamp(currentValue, 0f, maxValue);
            finalValue = clampedCurrent / maxValue; 
        }
    }
}