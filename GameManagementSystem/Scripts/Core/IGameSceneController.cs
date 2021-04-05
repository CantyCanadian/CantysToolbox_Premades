using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Canty.GameManagementSystem
{
    public partial interface IGameSceneController
    {
        void OnEnter();
        void OnExit();
    }
}