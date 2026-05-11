using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace WanChaoGuiYi.Tests
{
    public sealed class AudioManagerPlayModeTests
    {
        [UnityTest]
        public System.Collections.IEnumerator AudioManagerCreatesLayeredPoolsAndRoutesSpatialCues()
        {
            Type managerType = RuntimeType("AudioManager");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for layered audio control.");

            GameObject root = new GameObject("AudioManager_LayeredPoolTest");
            Component manager = root.AddComponent(managerType);
            yield return null;

            string[] layers = (string[])Invoke(manager, "GetMixerLayerNames");
            CollectionAssert.Contains(layers, "Master");
            CollectionAssert.Contains(layers, "Music");
            CollectionAssert.Contains(layers, "Ambience");
            CollectionAssert.Contains(layers, "SFX");
            CollectionAssert.Contains(layers, "UI");
            CollectionAssert.Contains(layers, "War");
            Assert.GreaterOrEqual((int)Invoke(manager, "GetPoolSize", "War"), 8, "War layer needs enough pooled emitters for dense fronts.");
            Assert.GreaterOrEqual((int)Invoke(manager, "GetPoolSize", "SFX"), 12, "SFX layer should support bursty UI and map feedback.");

            AudioClip clip = CreatePulseClip("war_contact_clip");
            Invoke(manager, "RegisterRuntimeCue", "war_contact", clip, "War", true, 1f, 0.9f);
            AudioSource source = (AudioSource)Invoke(manager, "PlaySpatialCue", "war_contact", new Vector3(2f, 3f, -1f), "hanzhong");
            Assert.IsNotNull(source, "Spatial war cue should allocate an AudioSource.");
            Assert.AreEqual("hanzhong", (string)Invoke(manager, "GetLastCueRegionId"));
            Assert.AreEqual("war_contact", (string)Invoke(manager, "GetLastCueId"));
            Assert.GreaterOrEqual(source.spatialBlend, 0.95f);
            Assert.AreEqual(new Vector3(2f, 3f, -1f), source.transform.position);

            UnityEngine.Object.Destroy(root);
        }

        [UnityTest]
        public System.Collections.IEnumerator DynamicMusicLayersRegionThemesAndSnapshotsReactToMode()
        {
            Type managerType = RuntimeType("AudioManager");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for dynamic music control.");

            GameObject root = new GameObject("AudioManager_DynamicMusicTest");
            Component manager = root.AddComponent(managerType);
            yield return null;

            AudioClip governance = CreatePulseClip("governance_theme");
            AudioClip war = CreatePulseClip("war_theme");
            AudioClip region = CreatePulseClip("guanzhong_theme");
            Invoke(manager, "RegisterMusicLayerCue", "governance_theme", governance, "Music");
            Invoke(manager, "RegisterMusicLayerCue", "war_theme", war, "War");
            Invoke(manager, "RegisterRegionMusic", "guanzhong", "guanzhong_theme", region);

            Invoke(manager, "SetDynamicMusicState", "Governance", "guanzhong", 0.25f);
            Assert.AreEqual("Governance", (string)Invoke(manager, "GetCurrentSnapshotName"));
            Assert.AreEqual("guanzhong_theme", (string)Invoke(manager, "GetActiveRegionMusicCueId"));
            Assert.Greater((float)Invoke(manager, "GetLayerWeight", "Music"), 0.7f);
            Assert.Less((float)Invoke(manager, "GetLayerWeight", "War"), 0.45f);

            Invoke(manager, "SetDynamicMusicState", "War", "guanzhong", 0.9f);
            Assert.AreEqual("War", (string)Invoke(manager, "GetCurrentSnapshotName"));
            Assert.Greater((float)Invoke(manager, "GetLayerWeight", "War"), 0.75f);
            Assert.Less((float)Invoke(manager, "GetLayerWeight", "Ambience"), 0.5f);

            UnityEngine.Object.Destroy(root);
        }

        [UnityTest]
        public System.Collections.IEnumerator DynamicCompositionGeneratesRegionThemesAndUpdatesActiveSourceWeights()
        {
            Type managerType = RuntimeType("AudioManager");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for dynamic composition.");

            GameObject root = new GameObject("AudioManager_DynamicCompositionTest");
            Component manager = root.AddComponent(managerType);
            yield return null;

            Invoke(manager, "RegisterSnapshotProfile", "HighWar", 1f, 0.32f, 0.22f, 1f, 0.9f, 0.95f, 1.25f);
            Invoke(manager, "SetDynamicMusicState", "HighWar", "guanzhong", 0.2f);
            Assert.AreEqual("HighWar", (string)Invoke(manager, "GetCurrentSnapshotName"));
            Assert.AreEqual("region_theme_guanzhong", (string)Invoke(manager, "GetActiveRegionMusicCueId"), "Every region should get a deterministic generated music stem when no authored clip exists yet.");
            Assert.AreEqual(1.25f, (float)Invoke(manager, "GetSnapshotTransitionSeconds", "HighWar"), 0.001f);
            Assert.AreEqual(0.95f, (float)Invoke(manager, "GetLayerWeight", "War"), 0.001f);
            Assert.AreEqual(0.32f, (float)Invoke(manager, "GetLayerWeight", "Music"), 0.001f);

            Invoke(manager, "SetDynamicMusicState", "Governance", "jiangnan", 0.1f);
            Assert.AreEqual("region_theme_jiangnan", (string)Invoke(manager, "GetActiveRegionMusicCueId"), "Generated region music should be unique per region id.");

            AudioClip clip = CreatePulseClip("active_war_loop");
            Invoke(manager, "RegisterRuntimeCue", "active_war_loop", clip, "War", false, 1f, 0f);
            AudioSource source = (AudioSource)Invoke(manager, "PlayCue", "active_war_loop", "front");
            Invoke(manager, "SetDynamicMusicState", "Governance", "guanzhong", 0f);
            float quietWarVolume = source.volume;
            Invoke(manager, "SetDynamicMusicState", "War", "guanzhong", 1f);
            Assert.Greater(source.volume, quietWarVolume + 0.5f, "Active pooled sources should follow dynamic snapshot layer weights instead of keeping stale volume.");

            UnityEngine.Object.Destroy(root);
        }

        [UnityTest]
        public System.Collections.IEnumerator AudioEventBridgeMapsGameEventsToCuesAndMixerSnapshots()
        {
            Type managerType = RuntimeType("AudioManager");
            Type bridgeType = RuntimeType("AudioEventBridge");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for event audio.");
            Assert.IsNotNull(bridgeType, "Runtime should expose AudioEventBridge for scene trigger logic.");

            GameObject root = new GameObject("AudioManager_EventBridgeTest");
            Component manager = root.AddComponent(managerType);
            Component bridge = root.AddComponent(bridgeType);
            EventBus events = new EventBus();
            Invoke(bridge, "Bind", events, manager);
            yield return null;

            events.Publish(new GameEvent(GameEventType.RegionSelected, "guanzhong", null));
            Assert.AreEqual("ui_region_select", (string)Invoke(manager, "GetLastCueId"));
            Assert.AreEqual("guanzhong", (string)Invoke(manager, "GetLastCueRegionId"));

            events.Publish(new GameEvent(GameEventType.ContactDetected, "engagement_1", null));
            Assert.AreEqual("war_contact", (string)Invoke(manager, "GetLastCueId"));
            Assert.AreEqual("War", (string)Invoke(manager, "GetCurrentSnapshotName"));

            events.Publish(new GameEvent(GameEventType.GovernanceImpactApplied, "guanzhong", null));
            Assert.AreEqual("governance_apply", (string)Invoke(manager, "GetLastCueId"));

            UnityEngine.Object.Destroy(root);
        }

        [UnityTest]
        public System.Collections.IEnumerator SpatialEventAudioUsesRealRegionControllerPosition()
        {
            Type managerType = RuntimeType("AudioManager");
            Type bridgeType = RuntimeType("AudioEventBridge");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for spatial debug state.");
            Assert.IsNotNull(bridgeType, "Runtime should expose AudioEventBridge for map-position audio.");

            GameObject root = null;
            GameObject region = null;
            try
            {
                root = new GameObject("AudioManager_RealRegionPositionTest");
                Component manager = root.AddComponent(managerType);
                Component bridge = root.AddComponent(bridgeType);
                EventBus events = new EventBus();
                Invoke(bridge, "Bind", events, manager);

                region = new GameObject("RegionSurface_guanzhong");
                region.transform.position = new Vector3(4.25f, -2.5f, -0.2f);
                RegionController controller = region.AddComponent<RegionController>();
                controller.Bind("guanzhong", null);

                yield return null;

                events.Publish(new GameEvent(GameEventType.ContactDetected, "guanzhong", null));
                Assert.AreEqual("war_contact", (string)Invoke(manager, "GetLastCueId"));
                Assert.AreEqual("guanzhong", (string)Invoke(manager, "GetLastCueRegionId"));
                Assert.AreEqual(region.transform.position, (Vector3)Invoke(manager, "GetLastCueWorldPosition"), "Spatial event audio should use the real 56-region map transform instead of synthetic hash positions.");
            }
            finally
            {
                if (region != null) UnityEngine.Object.Destroy(region);
                if (root != null) UnityEngine.Object.Destroy(root);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator WarLogisticsAndOccupationEventsUseDistinctAudioCues()
        {
            Type managerType = RuntimeType("AudioManager");
            Type bridgeType = RuntimeType("AudioEventBridge");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for event cue inspection.");
            Assert.IsNotNull(bridgeType, "Runtime should expose AudioEventBridge for detailed logistics audio.");

            GameObject root = null;
            try
            {
                root = new GameObject("AudioManager_LogisticsCueTest");
                Component manager = root.AddComponent(managerType);
                Component bridge = root.AddComponent(bridgeType);
                EventBus events = new EventBus();
                Invoke(bridge, "Bind", events, manager);
                yield return null;

                events.Publish(new GameEvent(GameEventType.FrontlinePrepared, "army_player_1", new FrontlinePreparationPayload { targetRegionId = "hanzhong" }));
                Assert.AreEqual("logistics_prepare", (string)Invoke(manager, "GetLastCueId"));
                Assert.AreEqual("hanzhong", (string)Invoke(manager, "GetLastCueRegionId"));

                events.Publish(new GameEvent(GameEventType.FrontlineLogisticsAdvanced, "army_player_1", new FrontlineLogisticsPayload { targetRegionId = "hanzhong", foodDelivered = 60, foodLost = 0 }));
                Assert.AreEqual("logistics_convoy", (string)Invoke(manager, "GetLastCueId"));

                events.Publish(new GameEvent(GameEventType.FrontlineLogisticsCommanded, "army_player_1", new FrontlineLogisticsCommandPayload { targetRegionId = "hanzhong", command = "pause" }));
                Assert.AreEqual("logistics_command", (string)Invoke(manager, "GetLastCueId"));

                events.Publish(new GameEvent(GameEventType.FrontlineLogisticsRaided, "army_player_1", new FrontlineLogisticsRaidPayload { targetRegionId = "hanzhong", foodLost = 30, raidPressure = 80 }));
                Assert.AreEqual("logistics_raided", (string)Invoke(manager, "GetLastCueId"));
                Assert.AreEqual("War", (string)Invoke(manager, "GetCurrentSnapshotName"));

                events.Publish(new GameEvent(GameEventType.OccupationPacificationQueueAdvanced, "hanzhong", new OccupationPacificationQueuePayload { regionId = "hanzhong", turnsRemainingAfter = 2 }));
                Assert.AreEqual("occupation_pacification", (string)Invoke(manager, "GetLastCueId"));

                events.Publish(new GameEvent(GameEventType.DiplomacyWarDeclared, "enemy_faction", null));
                Assert.AreEqual("war_declared", (string)Invoke(manager, "GetLastCueId"));
            }
            finally
            {
                if (root != null) UnityEngine.Object.Destroy(root);
            }
        }

        [UnityTest]
        public System.Collections.IEnumerator AudioManifestBinderRegistersExistingManifestClipsAndSceneMusic()
        {
            Type managerType = RuntimeType("AudioManager");
            Type binderType = RuntimeType("AudioManifestBinder");
            Assert.IsNotNull(managerType, "Runtime should expose AudioManager for manifest registration.");
            Assert.IsNotNull(binderType, "Runtime should expose AudioManifestBinder for existing audio manifests.");

            GameObject root = new GameObject("AudioManager_ManifestBinderTest");
            Component manager = root.AddComponent(managerType);
            Component binder = root.AddComponent(binderType);
            Invoke(binder, "Bind", manager);
            yield return null;

            Assert.IsTrue((bool)Invoke(binder, "HasBoundMixer"), "Manifest binder should attach the generated strategy AudioMixer asset.");
            Assert.GreaterOrEqual((int)Invoke(binder, "GetBoundMixerRouteCount"), 6, "AudioMixer should expose route groups for Master/Music/Ambience/SFX/UI/War.");
            Assert.AreEqual((string)Invoke(binder, "GetBoundMixerName"), (string)Invoke(manager, "GetAudioMixerName"));
            Assert.GreaterOrEqual((int)Invoke(binder, "GetSceneCueCount"), 10, "Scene music manifest should register all scene tracks.");
            Assert.GreaterOrEqual((int)Invoke(binder, "GetSceneClipCount"), 10, "Scene music should bind real MP3 AudioClip assets in the editor.");
            Assert.GreaterOrEqual((int)Invoke(binder, "GetEmperorCueCount"), 13, "Emperor theme manifest should register all emperor cue ids.");
            Assert.GreaterOrEqual((int)Invoke(binder, "GetChronicleCueCount"), 30, "Chronicle event manifest should register the historical event cue set.");
            Assert.GreaterOrEqual((int)Invoke(manager, "GetRegionMusicCueCount"), 56, "Region music manifest should register authored cues for every real region.");
            Assert.AreEqual("region_music_guanzhong", (string)Invoke(manager, "GetRegionMusicCueId", "guanzhong"));
            Assert.IsTrue((bool)Invoke(manager, "HasCue", "scene_governance"));
            Assert.IsTrue((bool)Invoke(manager, "HasCue", "scene_war"));
            Assert.IsTrue((bool)Invoke(manager, "HasCue", "emperor_qin_shi_huang"));
            Assert.IsTrue((bool)Invoke(manager, "HasCue", "event_yellow_river_flood"));
            Assert.IsTrue((bool)Invoke(manager, "HasCue", "region_music_guanzhong"));
            StringAssert.Contains("scene_governance", (string)Invoke(manager, "GetCueClipName", "scene_governance"));
            StringAssert.DoesNotContain("runtime_region_theme", (string)Invoke(manager, "GetCueClipName", "region_music_guanzhong"));

            Invoke(manager, "SetDynamicMusicState", "Governance", "guanzhong", 0f);
            Assert.AreEqual("scene_governance", (string)Invoke(manager, "GetActiveSceneMusicCueId"));
            Assert.AreEqual("region_music_guanzhong", (string)Invoke(manager, "GetActiveRegionMusicCueId"));
            Invoke(manager, "SetDynamicMusicState", "War", null, 1f);
            Assert.AreEqual("scene_war", (string)Invoke(manager, "GetActiveSceneMusicCueId"));
            string audioDebugSummary = (string)Invoke(manager, "GetAudioDebugSummary");
            StringAssert.Contains("音频", audioDebugSummary);
            StringAssert.Contains("状态=", audioDebugSummary);
            StringAssert.Contains("路由=", audioDebugSummary);
            StringAssert.Contains("池=", audioDebugSummary);

            UnityEngine.Object.Destroy(root);
        }

        private static Type RuntimeType(string typeName)
        {
            return Type.GetType("WanChaoGuiYi." + typeName + ", WanChaoGuiYi.Runtime");
        }

        private static object Invoke(object target, string methodName, params object[] args)
        {
            MethodInfo method = ResolveMethod(target.GetType(), methodName, args);
            Assert.IsNotNull(method, target.GetType().Name + " should expose " + methodName + ".");
            return method.Invoke(target, args);
        }

        private static MethodInfo ResolveMethod(Type type, string methodName, object[] args)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name != methodName) continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != args.Length) continue;

                bool matches = true;
                for (int p = 0; p < parameters.Length; p++)
                {
                    if (args[p] != null && !parameters[p].ParameterType.IsInstanceOfType(args[p]))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches) return method;
            }

            return null;
        }

        private static AudioClip CreatePulseClip(string name)
        {
            AudioClip clip = AudioClip.Create(name, 128, 1, 44100, false);
            float[] samples = new float[128];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = i % 2 == 0 ? 0.25f : -0.25f;
            }

            clip.SetData(samples, 0);
            return clip;
        }
    }
}
