using Canty.EventSystem;
using System;

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

        public override void Copy(EventBase eventObject)
        {
            if (eventObject is WorldSceneChangedEvent<ScenesType> sceneChangeEvent)
            {
                Scene = sceneChangeEvent.Scene;
                GameSceneController = sceneChangeEvent.GameSceneController;
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