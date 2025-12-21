using System;
using Core.Config;

namespace Core.Events
{
    public class MessageBoxEvent : WindowEvent
    {
        public GameError Error;
        public Action ExecuteOnClose;
    }
}