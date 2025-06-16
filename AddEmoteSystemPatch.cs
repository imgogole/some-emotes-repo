using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SomeEmotesREPO
{
    [HarmonyPatch(typeof(PlayerController))]
    class AddEmoteSystemPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void Start_Postfix(PlayerController __instance)
        {
            var emoteSystem = __instance.GetComponent<EmoteSystem>();
            if (!emoteSystem)
            {
                emoteSystem = __instance.gameObject.AddComponent<EmoteSystem>();
            }

            emoteSystem.SetPlayerController(__instance);

            SomeEmotesREPO.Logger.LogInfo("EmoteSystem has been added to the player. Press [P] to see the Emote Panel.");
        }
    }
}
