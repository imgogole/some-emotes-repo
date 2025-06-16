using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SomeEmotesREPO
{
    public static class EmoteBundleLoader
    {
        private static AssetBundle emoteBundle;

        public static AssetBundle Load(string path)
        {
            if (emoteBundle == null)
            {
                emoteBundle = AssetBundle.LoadFromFile(path);
                if (emoteBundle == null)
                {
                    Debug.LogError("Échec de chargement de l’AssetBundle !");
                }
            }
            else
            {
                Debug.Log("[EmoteBundleLoader] Bundle déjà chargé, réutilisation.");
            }

            return emoteBundle;
        }

        public static void Unload()
        {
            if (emoteBundle != null)
            {
                emoteBundle.Unload(true);
                emoteBundle = null;
                Debug.Log("[EmoteBundleLoader] Bundle déchargé.");
            }
        }

        public static T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            if (emoteBundle == null)
            {
                return null;
            }

            name = name.ToLower();

            var allPaths = emoteBundle.GetAllAssetNames();
            foreach (var path in allPaths)
            {
                if (path.ToLower().EndsWith(name))
                {
                    return emoteBundle.LoadAsset<T>(path);
                }
            }

            return null;
        }

        public static List<string> GetAllAnimNames()
        {
            List<string> result = new List<string>();
            foreach (string name in emoteBundle.GetAllAssetNames())
            {
                if (name.EndsWith(".anim"))
                {
                    result.Add(name);
                }
            }
            return result;
        }
    }
}
