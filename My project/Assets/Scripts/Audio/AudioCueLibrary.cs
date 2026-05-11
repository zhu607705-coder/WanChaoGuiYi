using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace WanChaoGuiYi
{
    public enum StrategyAudioLayer
    {
        Master,
        Music,
        Ambience,
        SFX,
        UI,
        War
    }

    [Serializable]
    public sealed class AudioCueDefinition
    {
        public string id;
        public AudioClip clip;
        public StrategyAudioLayer layer = StrategyAudioLayer.SFX;
        public bool spatial;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 1f)] public float spatialBlend = 0f;
        public bool loop;
        public int priority = 128;
    }

    [Serializable]
    public sealed class RegionMusicDefinition
    {
        public string regionId;
        public string cueId;
        public AudioClip clip;
    }

    [Serializable]
    public sealed class SceneMusicDefinition
    {
        public string sceneName;
        public string cueId;
        public AudioClip clip;
    }

    [Serializable]
    public sealed class AudioMixerLayerRouteDefinition
    {
        public StrategyAudioLayer layer = StrategyAudioLayer.SFX;
        public AudioMixerGroup mixerGroup;
        public string exposedVolumeParameter;
        [Range(0f, 1f)] public float defaultWeight = 1f;
    }

    [Serializable]
    public sealed class AudioSnapshotLayerWeight
    {
        public StrategyAudioLayer layer = StrategyAudioLayer.Master;
        [Range(0f, 1f)] public float weight = 1f;
    }

    [Serializable]
    public sealed class AudioSnapshotDefinition
    {
        public string name = "Governance";
        public AudioMixerSnapshot mixerSnapshot;
        [Min(0f)] public float transitionSeconds = 0.75f;
        public AudioSnapshotLayerWeight[] weights = new AudioSnapshotLayerWeight[0];
    }

    [CreateAssetMenu(menuName = "WanChaoGuiYi/Audio/Cue Library", fileName = "AudioCueLibrary")]
    public sealed class AudioCueLibrary : ScriptableObject
    {
        [SerializeField] private AudioCueDefinition[] cues = new AudioCueDefinition[0];
        [SerializeField] private SceneMusicDefinition[] sceneMusic = new SceneMusicDefinition[0];
        [SerializeField] private RegionMusicDefinition[] regionMusic = new RegionMusicDefinition[0];
        [SerializeField] private AudioMixerLayerRouteDefinition[] mixerRoutes = new AudioMixerLayerRouteDefinition[0];
        [SerializeField] private AudioSnapshotDefinition[] snapshots = new AudioSnapshotDefinition[0];

        public IEnumerable<AudioCueDefinition> Cues
        {
            get { return cues; }
        }

        public IEnumerable<SceneMusicDefinition> SceneMusic
        {
            get { return sceneMusic; }
        }

        public IEnumerable<RegionMusicDefinition> RegionMusic
        {
            get { return regionMusic; }
        }

        public IEnumerable<AudioMixerLayerRouteDefinition> MixerRoutes
        {
            get { return mixerRoutes; }
        }

        public IEnumerable<AudioSnapshotDefinition> Snapshots
        {
            get { return snapshots; }
        }
    }
}
