using HarmonyLib;
using System;

namespace SomeEmotesREPO.Utils
{
    public static class ChatReflection
    {
        private static readonly AccessTools.FieldRef<ChatManager, bool> _chatActiveGetter =
            AccessTools.FieldRefAccess<ChatManager, bool>("chatActive");

        public static bool IsChatActive()
        {
            if (ChatManager.instance == null)
                return false;

            return _chatActiveGetter(ChatManager.instance);
        }
    }
}
