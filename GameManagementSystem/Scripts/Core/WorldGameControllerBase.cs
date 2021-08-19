using System;
using UnityEngine;

namespace Canty.GameManagementSystem
{
    public abstract partial class WorldGameControllerBase<ScenesType> : WorldEventListenerBase
        where ScenesType : Enum
    {
        [SerializeField] private ScenesType m_DefaultScene = default;

        private WorldChangeSceneEvent<ScenesType> m_ChangeSceneEvent = new WorldChangeSceneEvent<ScenesType>("WorldGameController");

        protected override void Awake()
        {
            base.Awake();

            m_ChangeSceneEvent.Reset(m_DefaultScene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            _dispatcher.SendEvent(m_ChangeSceneEvent);
        }
    }
}