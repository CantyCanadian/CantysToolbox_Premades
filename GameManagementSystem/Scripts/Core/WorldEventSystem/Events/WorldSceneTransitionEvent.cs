using Canty.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Canty.GameManagementSystem
{
    public class WorldSceneTransitionEvent : WorldEventBase
    {
        public override void Copy(EventBase eventObject) { }

        public override string GetDebugData()
        {
            return $"[WorldSceneTransitionEvent] sent by [{Origin}] : Scene is transitioning.";
        }

        public WorldSceneTransitionEvent(string origin)
            : base(origin) { }
    }
}