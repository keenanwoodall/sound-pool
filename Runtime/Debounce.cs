namespace Beans.Sound
{
    public struct Debounce
    {
        public int Id;
        public float Duration;

        public Debounce WithDuration(float duration) => new Debounce { Id = Id, Duration = duration };
    }
}
