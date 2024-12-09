using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;

namespace Beans.Sound
{
    public struct SoundHandle
    {
        public int Generation;
        public SoundInstance Sound;

        public bool IsValid() => Sound != null && Generation == Sound.Generation;

        public SoundHandle PreparePlay(AudioClip clip)
        {
            Sound.Audio.enabled = true;
            Sound.Audio.clip = clip;
            return this;
        }

        public SoundHandle Play(AudioClip clip)
        {
            Sound.Audio.enabled = true;
            Sound.Audio.clip = clip;
            Sound.Audio.Play();
            return this;
        }

        public SoundHandle SetFromConfig(in SoundConfig config)
        {
            SetVolume(config.Volume);
            SetPitch(config.Pitch);
            VaryPitch(config.PitchVariance);
            return this;
        }

        public SoundHandle SetVolume(float volume)
        {
            Sound.Audio.volume = volume;
            return this;
        }

        public float GetVolume()
        {
            return Sound.Audio.volume;
        }

        public SoundHandle SetPitch(float pitch)
        {
            Sound.Audio.pitch = pitch;
            return this;
        }

        public SoundHandle SetPitchForDuration(float duration)
        {
            Sound.Audio.pitch = Sound.Audio.clip.length / duration;
            return this;
        }

        public float GetPitch()
        {
            return Sound.Audio.pitch;
        }

        public SoundHandle VaryPitch(float variance)
        {
            Sound.Audio.pitch += Mathf.Lerp(-variance, +variance, Random.value);
            return this;
        }

        public SoundHandle SetLoop(bool loop = true)
        {
            Sound.Audio.loop = loop;
            return this;
        }

        public SoundHandle SetSpatial(bool spatial)
        {
            Sound.Audio.spatialize = spatial;
            return this;
        }

        public SoundHandle SetSpatialBlend(float spatialBlend)
        {
            Sound.Audio.spatialBlend = spatialBlend;
            Sound.Audio.spatialize = spatialBlend > 0f;
            return this;
        }

        public SoundHandle SetMaxDistance(float distance)
        {
            Sound.Audio.maxDistance = distance;
            return this;
        }

        public SoundHandle SetRolloffMode(AudioRolloffMode rolloffMode)
        {
            Sound.Audio.rolloffMode = rolloffMode;
            return this;
        }

        public SoundHandle SetMixerGroup(AudioMixerGroup mixerGroup)
        {
            Sound.Audio.outputAudioMixerGroup = mixerGroup;
            return this;
        }

        public SoundHandle SetPosition(Vector3 position)
        {
            Sound.Audio.transform.position = position;
            return this;
        }

        public SoundHandle SetFollowTarget(Transform target)
        {
            var source = new ConstraintSource() { sourceTransform = target, weight = 1f };

            Sound.Parent.AddSource(source);

            var sourceIndex = Sound.Parent.sourceCount - 1;
            Sound.Parent.SetTranslationOffset(sourceIndex, target.InverseTransformPoint(Sound.Audio.transform.position));
            Sound.Parent.SetRotationOffset(sourceIndex, (Quaternion.Inverse(target.rotation) * Sound.Audio.transform.rotation).eulerAngles);

            Sound.Parent.enabled = true;
            Sound.Parent.constraintActive = true;

            return this;
        }

        public SoundHandle SetAutoRelease(bool autoRelease)
        {
            Sound.AutoRelease = autoRelease;
            return this;
        }
    }
}
