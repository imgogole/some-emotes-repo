﻿using SomeEmotesREPO.Utils;
using System.Collections.Generic;
using UnityEngine;

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
        public const int LineHeigth = 30;
        public const string delimiter = "-----------------------------";

        public GUIStyle style;

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
            if (ChatManager.instance.chatActive) return;

            if (Visible)
            {
                if (currentPage > 0 && Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    OpenPanel(currentPage - 1);
                }

                if (currentPage < EmoteLoader.Instance.TotalPages && Input.GetKeyDown(KeyCode.RightArrow))
                {
                    OpenPanel(currentPage + 1);
                }

                for (int i = 0; i < emotesToPlay.Count; i++)
                {
                    KeyCode key1 = (KeyCode)(49 + i);
                    KeyCode key2 = (KeyCode)(257 + i);

                    bool pressed = Input.GetKeyDown(key1) || Input.GetKeyDown(key2);
                    bool ctrl = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

                    if (pressed)
                    {
                        if (ctrl)
                        {
                            EmoteSystem.Instance.SetFavorite(emotesToPlay[i]);
                            UpdateLines();
                        }
                        else
                        {
                            EmoteSystem.Instance.PlayEmote(emotesToPlay[i]);
                        }
                    }
                }
            }
        }


        void UpdateLines()
        {
            lines.Clear();
            emotesToPlay.Clear();
            var l = EmoteSystem.Instance.FetchEmotes(currentPage * emotePerPages, emotePerPages);

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
                    lines.Add($"No emote yet");
                }
                lines.Add(delimiter);
                lines.Add("Press [->] for next page");
            }
            else
            {
                lines.Add($"Page {currentPage + 1}/{EmoteLoader.Instance.TotalPages + 1}");
                lines.Add(delimiter);
                for (int i = 0; i < l.Count; i++)
                {
                    lines.Add($"[{i + 1}] {l[i]}");
                    emotesToPlay.Add(l[i]);
                }
                lines.Add(delimiter);
                if (EmoteLoader.Instance.TotalPages > currentPage) lines.Add("Press [->] for next page");
                lines.Add("Press [<-] for previous page");
            }

            if (l.Count > 0)
            {
                lines.Add("Press [Num] to emote");
                lines.Add("Press [Alt + Num] to add to favorite");
            }
            lines.Add($"Press [{EmoteLoader.PanelKey}] to quit");
        }

        void OnGUI()
        {
            if (!EmoteSystem.Ready || !Visible || Lines.Count == 0)
                return;

            if (style == null)
            {
                style = GUI.skin.GetStyle("label");
                style.fontSize = (LineHeigth - 8);
                style.font = EmoteLoader.GetFont();
                style.richText = true;
            }

            const float pad = 8f;

            float y = Screen.height - pad - Lines.Count * LineHeigth;

            foreach (var line in Lines)
            {
                GUI.Label(new Rect(pad, y, Screen.width - pad * 2, LineHeigth), line, style);
                y += LineHeigth;
            }
        }
    }
}
