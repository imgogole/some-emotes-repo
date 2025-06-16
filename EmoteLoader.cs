using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SomeEmotesREPO
{
    public class EmoteLoader : MonoBehaviour
    {
        private AssetBundle assetBundle;

        private static EmoteLoader instance;
        public static EmoteLoader Instance {  get { return instance;  } }

        private List<string> emotesName = new List<string>();

        public int TotalPages
        {
            get
            {
                return Mathf.CeilToInt(emotesName.Count / EmoteSelectionManager.emotePerPages);
            }
        }

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            var path = Path.Combine(Paths.PluginPath, "ImGogole-SomeEmotesREPO", "Emotes", "emotes.bundle");
            assetBundle = EmoteBundleLoader.Load(path);
        }

        public List<string> FetchEmotes(int from, int offset)
        {
            var result = new List<string>();

            if (from < 0) from = 0;
            if (offset <= 0 || from >= emotesName.Count)
                return result;

            int length = Mathf.Min(offset, emotesName.Count - from);

            return emotesName.GetRange(from, length);
        }

        public EmoteLauncher LoadEmote(PlayerController player)
        {
            if (assetBundle == null)
            {
                SomeEmotesREPO.Logger.LogError("Unable to load emotes.bundle");
                return null;
            }

            emotesName.Clear();

            //first we get the prefab, it contains the repo model with an animator
            GameObject emotePrefab = EmoteBundleLoader.LoadAsset<GameObject>("emote.prefab");

            if (emotePrefab == null)
            {
                SomeEmotesREPO.Logger.LogError("Prefab emote not found");
                return null;
            }

            GameObject emoteInstance = Instantiate(emotePrefab, Vector3.zero, Quaternion.identity);

            //then for this object, we add him the EmoteLauncher component. This component allows himself to play the
            //asked emote
            EmoteLauncher emoteLauncher = emoteInstance.AddComponent<EmoteLauncher>();

            //we search every anim in the assetbundle, check them then add them into out emote launcher
            List<string> animNames = EmoteBundleLoader.GetAllAnimNames();
            foreach (string name in animNames)
            {
                emoteLauncher.AddEntry(ExtractElement(name), EmoteBundleLoader.LoadAsset<AnimationClip>(name));
            }

            //init the launcher
            emoteLauncher.Init(player.transform); //dumbass method

            //set emote names
            if (player.playerAvatarScript.playerAvatarVisuals.playerAvatar.photonView.IsMine)//????
                emotesName = emoteLauncher.emoteNames;

            // set the textures from the character
            emoteLauncher.InitTexturesFrom(player.playerAvatar.transform.parent.gameObject);

            SomeEmotesREPO.Logger.LogInfo($"Emote created of {player.playerAvatarScript.playerAvatarVisuals.playerAvatar.photonView.Owner.NickName}");

            return emoteLauncher;
        }

        public static string ExtractElement(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string lastSegment = path.Trim();
            int lastSlash = lastSegment.LastIndexOf('/');
            if (lastSlash >= 0)
                lastSegment = lastSegment.Substring(lastSlash + 1);

            if (lastSegment.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - ".anim".Length);

            return lastSegment;
        }
    }
}
