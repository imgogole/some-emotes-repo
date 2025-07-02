using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SomeEmotesREPO
{
    [HarmonyPatch(typeof(GameDirector))]
    class AddPatchGameDirector
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameDirector.DeathStart))]
        private static void DeathStart_Prefix()
        {
            if (EmoteSystem.Ready && EmoteSystem.Instance.IsEmoting)
            {
                EmoteSystem.Instance.StopEmote();
            }
        }

    }
}
