namespace Game.Logic.Mixer
{
    public readonly struct MixerLetter
    {
        public int Index { get; }
        public string Value { get; }

        public MixerLetter(int index, string value)
        {
            Index = index;
            Value = value;
        }
    }
}
