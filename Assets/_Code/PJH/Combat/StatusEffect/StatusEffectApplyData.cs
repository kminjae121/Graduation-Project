namespace Code.Combat.StatusEffect
{
    public readonly struct StatusEffectApplyData
    {
        public int? Duration { get; }
        public int? Value { get; }

        public StatusEffectApplyData(int? duration, int? value)
        {
            Duration = duration;
            Value = value;
        }
    }
}