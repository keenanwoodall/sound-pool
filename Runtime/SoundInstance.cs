using UnityEngine;
using UnityEngine.Animations;

namespace Beans.Sound
{
    public class SoundInstance
    {
        public int Generation;
        public AudioSource Audio;
        public ParentConstraint Parent;
        public bool AutoRelease;

        public SoundInstance(AudioSource audio, ParentConstraint parent)
        {
            Audio = audio;
            Parent = parent;
            AutoRelease = true;
        }
    }
}
