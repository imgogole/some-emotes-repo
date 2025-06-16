using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SomeEmotesREPO
{
    public class EmoteSelectionManager : MonoBehaviour
    {
        static EmoteSelectionManager instance;

        bool visible = false;
        int currentPage = 0;
        List<string> lines = new List<string>();
        List<string> emotesToPlay = new List<string>();

        public List<string> Lines => lines;
        public List<string> EmotesToPlay => emotesToPlay;
        public bool Visible => visible;

        public const int emotePerPages = 8;
        public const string delimiter = "----------------";

        public static EmoteSelectionManager Instance
        {
            get { return instance; }
        }

        void Awake()
        {
            instance = this;

            SomeEmotesREPO.Logger.LogInfo("EmoteSelectionManager ready.");
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            if (visible)
            {
                OpenPanel(0);
            }
        }

        public void OpenPanel(int page)
        {
            currentPage = page;

            UpdateLines();
        }

        void Update()
        {
            if (Visible)
            {
                if (currentPage > 0)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0))
                    {
                        OpenPanel(currentPage - 1);
                    }
                }
                if (currentPage < EmoteLoader.Instance.TotalPages)
                {
                    if (Input.GetKeyDown(KeyCode.Comma))
                    {
                        OpenPanel(currentPage + 1);
                    }
                }

                for (int i = 0; i < emotesToPlay.Count; i++)
                {
                    if (Input.GetKeyDown((KeyCode)(49 + i)) || Input.GetKeyDown((KeyCode)(257 + i)))
                    {
                        EmoteSystem.Instance.PlayEmote(emotesToPlay[i]);
                    }
                }
            }
        }

        void UpdateLines()
        {
            lines.Clear();
            emotesToPlay.Clear();
            var l = EmoteLoader.Instance.FetchEmotes(currentPage * emotePerPages, emotePerPages);

            if (currentPage == 0)
            {
                //favorite page
                lines.Add("Favorite emotes");
                lines.Add(delimiter);
                if (l.Count > 0)
                {
                    for (int i = 0; i < l.Count; i++)
                    {
                        lines.Add($"[{i + 1}] {l[i]}");
                        emotesToPlay.Add(l[i]);
                    }
                }
                else
                {
                    lines.Add($"No favorite emote yet");
                }
                lines.Add(delimiter);
                lines.Add("Press [.] for next page");
            }
            else
            {
                lines.Add($"Page {currentPage}/{EmoteLoader.Instance.TotalPages}");
                lines.Add(delimiter);
                for (int i = 0; i < l.Count; i++)
                {
                    lines.Add($"[{i + 1}] {l[i]}");
                    emotesToPlay.Add(l[i]);
                }
                lines.Add(delimiter);
                if (EmoteLoader.Instance.TotalPages > currentPage) lines.Add("Press [.] for next page");
                lines.Add("Press [0] for previous page");
            }

            lines.Add("Press [Num] to emote");
            lines.Add("Press [Ctrl + Num] to add to favourite");
            lines.Add("Press [P] to quit");
        }

        void OnGUI()
        {
            if (!Visible || Lines.Count == 0)
                return;

            const float pad = 8f;
            float lineH = 18f;

            float y = Screen.height - pad - Lines.Count * lineH;

            foreach (var line in Lines)
            {
                GUI.Label(new Rect(pad, y, Screen.width - pad * 2, lineH), line);
                y += lineH;
            }
        }
    }
}
