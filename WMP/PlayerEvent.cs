using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMP
{
    public enum ActionType
    {
        STOP,
        PLAY,
        PAUSE
    };

    public class PlayerEvent : EventArgs
    {
        public PlayerEvent(ActionType action)
        {
            this.PlayerAction = action;
        }

        public ActionType PlayerAction { get; private set; }
    }
}
