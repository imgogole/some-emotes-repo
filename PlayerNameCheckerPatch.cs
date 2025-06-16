﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SomeEmotesREPO
{
    [HarmonyPatch(typeof(PlayerNameChecker))]
    internal class PlayerNameCheckerPatch
    {
        [HarmonyPrefix, HarmonyPatch(nameof(PlayerNameChecker.Update))]
        public static void Update_Prefix(PlayerNameChecker __instance)
        {
            if (EmoteSystem.Instance && EmoteSystem.Instance.IsEmoting)
            {
                __instance.checkTimer = 0.25f; // Force PlayerNameChecker.Update to never accomplish his task
            }
        }
    }
}
