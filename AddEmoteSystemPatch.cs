using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SomeEmotesREPO
{
    [HarmonyPatch(typeof(PlayerAvatar))]
    class AddEmoteSystemPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix(PlayerAvatar __instance)
        {
            var emoteSystem = __instance.GetComponent<EmoteSystem>();
            if (!emoteSystem)
            {
                emoteSystem = __instance.gameObject.AddComponent<EmoteSystem>();
            }

            emoteSystem.SetPlayerAvatar(__instance);

            if (__instance.photonView.IsMine)
            {
                SomeEmotesREPO.Logger.LogInfo($"EmoteSystem has been added to the player. Press [{EmoteLoader.PanelKey}] to see the Emote Panel.");
            }
        }
    }
}
