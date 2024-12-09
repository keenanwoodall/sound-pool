using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Animations;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Beans.Sound
{
    [System.Serializable]
    public class SoundPool
    {
        private ObjectPool<SoundInstance> _pool;
        private List<SoundInstance> _active;
        private List<SoundHandle> _delayed;
        private List<double> _delays;
        private Dictionary<int, double> _debouncePlayTimes;
        private AudioMixerGroup _defaultMixerGroup;
        private double _time;
        private double _deltaTime;

        public SoundPool(int maxCapacity, int defaultCapacity, AudioMixerGroup defaultMixerGroup = null)
        {
            _defaultMixerGroup = defaultMixerGroup;
            _active = new List<SoundInstance>(32);
            _delayed = new List<SoundHandle>(16);
            _delays = new List<double>(16);
            _debouncePlayTimes = new Dictionary<int, double>(32);

            _pool = new ObjectPool<SoundInstance>
            (
                maxSize: maxCapacity,
                defaultCapacity: defaultCapacity,
                createFunc: () =>
                {
                    var obj = new GameObject("Pooled Sound");
                    // obj.hideFlags = HideFlags.HideInHierarchy;
                    
                    return new SoundInstance
                    (
                        obj.AddComponent<AudioSource>(), 
                        obj.AddComponent<ParentConstraint>()
                    );
                },
                actionOnGet: source =>
                {
                    source.AutoRelease = true;

                    source.Audio.gameObject.SetActive(true);
                    source.Audio.playOnAwake = false;

                    source.Audio.enabled = false;
                    source.Audio.clip = null;
                    source.Audio.volume = 1f;
                    source.Audio.pitch = 1f;
                    source.Audio.loop = false;
                    source.Audio.spatialize = false;
                    source.Audio.spatialBlend = 0f;
                    source.Audio.maxDistance = 500f;
                    source.Audio.rolloffMode = AudioRolloffMode.Logarithmic;
                    source.Audio.outputAudioMixerGroup = _defaultMixerGroup;

                    source.Parent.constraintActive = false;
                    source.Parent.enabled = false;
                    for (int i = 0; i < source.Parent.sourceCount; ++i)
                        source.Parent.RemoveSource(0);
                },
                actionOnRelease: source =>
                {
                    source.Generation++;
                    if (source.Audio != null && source.Audio.gameObject != null)
                        source.Audio.gameObject.SetActive(false);
                },
                actionOnDestroy: source =>
                {
                    if (source.Audio != null && source.Audio.gameObject != null)
                        Object.Destroy(source.Audio.gameObject);
                });
        }

        public void SetDefaultMixerGroup(AudioMixerGroup defaultMixerGroup)
        {
            _defaultMixerGroup = defaultMixerGroup;
        }

        public void Dispose()
        {
            while (_active.Count > 0)
                Release(_active[0]);
            _active.Clear();
            _pool.Dispose();
        }

        public SoundHandle Play(AudioClip clip, float delay = 0f)
        {
            var newSound = _pool.Get();
            var instance = new SoundHandle { Generation = newSound.Generation, Sound = newSound };
            instance.PreparePlay(clip);
            _delayed.Add(instance);
            _delays.Add(delay);
            return instance;
        }

        public SoundHandle? Play(AudioClip clip, Debounce debounce, float delay = 0f)
        {
            if (_debouncePlayTimes.TryGetValue(debounce.Id, out var lastPlayTime) && _time - lastPlayTime < debounce.Duration)
                return null;
            _debouncePlayTimes[debounce.Id] = _time;

            return Play(clip, delay);
        }

        public SoundHandle PlayRandom(AudioClip[] clips, float delay = 0f)
        {
            var newSound = _pool.Get();
            var instance = new SoundHandle { Generation = newSound.Generation, Sound = newSound };

            var index = Random.Range(0, clips.Length - 1);
            var clip = clips[index];

            instance.PreparePlay(clip);
            _delayed.Add(instance);
            _delays.Add(delay);

            return instance;
        }

        public SoundHandle? PlayRandom(AudioClip[] clips, Debounce debounce, float delay = 0f)
        {
            if (_debouncePlayTimes.TryGetValue(debounce.Id, out var lastPlayTime) && _time - lastPlayTime < debounce.Duration)
                return null;
            _debouncePlayTimes[debounce.Id] = _time;

            return PlayRandom(clips, delay);
        }

        public SoundHandle PlayRandomNoRepeat(AudioClip[] clips, ref int lastPlayIndex, float delay = 0f)
        {
            var newSound = _pool.Get();
            var instance = new SoundHandle { Generation = newSound.Generation, Sound = newSound };

            var index = 0;
            if (clips.Length > 1)
            {
                index = Random.Range(0, clips.Length - 1);
                while (lastPlayIndex == index)
                    index = Random.Range(0, clips.Length);
            }

            lastPlayIndex = index;
            var clip = clips[index];

            instance.PreparePlay(clip);
            _delayed.Add(instance);
            _delays.Add(delay);

            return instance;
        }

        public SoundHandle? PlayRandomNoRepeat(AudioClip[] clips, ref int lastPlayIndex, Debounce debounce, float delay = 0f)
        {
            if (_debouncePlayTimes.TryGetValue(debounce.Id, out var lastPlayTime) && _time - lastPlayTime < debounce.Duration)
                return null;
            _debouncePlayTimes[debounce.Id] = _time;

            return PlayRandomNoRepeat(clips, ref lastPlayIndex, delay);
        }

        public void Release(SoundInstance sound)
        {
            _active.Remove(sound);
            _pool.Release(sound);
        }

        public void Release(SoundHandle instance)
        {
            if (instance.IsValid())
            {
                if (!_active.Remove(instance.Sound))
                {
                    var removeIndex = _delayed.IndexOf(instance);
                    if (removeIndex >= 0)
                    {
                        _delayed.RemoveAt(removeIndex);
                        _delays.RemoveAt(removeIndex);
                    }
                    else
                        Debug.LogError("Failed to release sound instance.");
                }

                _pool.Release(instance.Sound);
            }
        }

        public void PrepareUpdate(double time)
        {
            _deltaTime = time - _time;
            _time = time;
        }

        public void Update()
        {
            for (int i = 0; i < _delayed.Count; ++i)
            {
                var instance = _delayed[i];
                var delay = _delays[i];

                delay -= _deltaTime;
                if (!instance.IsValid())
                {
                    _delayed.RemoveAt(i);
                    _delays.RemoveAt(i);
                    i--;
                    continue;
                }
                if (delay <= 0f)
                {
                    instance.Sound.Audio.Play();
                    _active.Add(instance.Sound);
                    _delayed.RemoveAt(i);
                    _delays.RemoveAt(i);
                    i--;
                    continue;
                }
                _delayed[i] = instance;
                _delays[i] = delay;
            }

            for (int i = 0; i < _active.Count; ++i)
            {
                var source = _active[i];
                if (source.AutoRelease && !source.Audio.isPlaying && Application.isFocused)
                {
                    Release(source);
                    i--;
                }
            }
        }
    }
}
