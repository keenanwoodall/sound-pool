namespace Beans.Sound
{
    public struct Debounces
    {
        public static Debounce FootScuff = new() { Id = 1, Duration = 0.5f };
        public static Debounce ArrowImpact = new() { Id = 2, Duration = 0.1f };
        public static Debounce LandingThump = new() { Id = 3, Duration = 0.3f };
        public static Debounce LandingFootstep = new() { Id = 4, Duration = 0.3f };
    }
}