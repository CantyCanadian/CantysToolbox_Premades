using Canty.EventSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Canty.GameManagementSystem
{
    public class WorldSceneChangedEvent<ScenesType> : WorldEventBase
        where ScenesType : Enum
    {
        public ScenesType Scene { get; private set; } = default;
        public IGameSceneController GameSceneController { get; private set; } = default;

        public void Reset(ScenesType scene, IGameSceneController gameSceneController)
        {
            Scene = scene;
            GameSceneController = gameSceneController;
        }

        public override void Copy(EventBase other)
        {
            if (other is WorldSceneChangedEvent<ScenesType>)
            {
                WorldSceneChangedEvent<ScenesType> otherEvent = other as WorldSceneChangedEvent<ScenesType>;

                Scene = otherEvent.Scene;
                GameSceneController = otherEvent.GameSceneController;
            }
        }

        public override string GetDebugData()
        {
            return $"[WorldSceneChangedEvent] sent by [{Origin}] : Scene changed to [{Scene}].";
        }

        public WorldSceneChangedEvent(string origin)
            : base(origin) { }
    }
}