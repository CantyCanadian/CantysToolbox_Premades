using Canty.EventSystem;

namespace Canty.GameManagementSystem
{
    public abstract class WorldEventBase : EventBase
    {
        public WorldEventBase(string origin) 
            : base(origin) { }
    }
}