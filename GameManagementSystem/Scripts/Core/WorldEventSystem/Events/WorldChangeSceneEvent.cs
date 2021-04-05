using Canty.EventSystem;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Canty.GameManagementSystem
{
    public class WorldChangeSceneEvent<ScenesType> : WorldEventBase
        where ScenesType : Enum
    {
        public ScenesType Scene { get; private set; } = default;
        public LoadSceneMode Mode { get; private set; } = LoadSceneMode.Additive;

        public void Reset(ScenesType scene, LoadSceneMode mode)
        {
            Scene = scene;
            Mode = mode;
        }

        public override void Copy(EventBase other)
        {
            if (other is WorldChangeSceneEvent<ScenesType>)
            {
                WorldChangeSceneEvent<ScenesType> otherEvent = other as WorldChangeSceneEvent<ScenesType>;

                Scene = otherEvent.Scene;
                Mode = otherEvent.Mode;
            }
        }

        public override string GetDebugData()
        {
            return $"[WorldChangeSceneEvent] sent by [{Origin}] : Scene change requested to [{Scene}] in [{Mode}] mode.";
        }

        public WorldChangeSceneEvent(string origin)
            : base(origin) { }

        public WorldChangeSceneEvent() { }
    }
}