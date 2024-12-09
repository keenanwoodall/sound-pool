using UnityEngine;

namespace Beans.Sound
{
    [System.Serializable]
    public struct SoundConfig
    {
        public static readonly SoundConfig Default = new()
        {
            Volume = 1f,
            Pitch = 1f,
            PitchVariance = 0.01f
        };

        [Range(0f, 1f)]
        public float Volume;
        public float Pitch;
        public float PitchVariance;
    }
}
