using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SomeEmotesREPO
{
    public class EmoteSystem : MonoBehaviourPun, IPunObservable
    {
        private bool isEmoting = false;
        private int emoteId = -1;
        private float emoteTime = 0f;

        public bool IsEmoting
        {
            get
            {
                return isEmoting;
            }
            set
            {
                if (photonView.IsMine)
                {
                    isEmoting = value;

                    if (camTransform)
                    {
                        if (isEmoting) camTransform.localPosition = camOffset;
                        else camTransform.localPosition = Vector3.zero;
                    }
                }

                if (isEmoting)
                {
                    emoteTime = 0f;
                }
            }
        }
        public int EmoteID
        {
            get
            {
                return emoteId;
            }
            set
            {
                if (photonView.IsMine)
                {
                    emoteId = value;
                }
            }
        }

        private Transform camTransform;

        private PlayerAvatar playerAvatar;
        private PlayerAvatarVisuals playerVisuals;

        private static Vector3 camOffset = new Vector3(0f, 0f, -3.25f);

        private static EmoteSystem instance;
        public static EmoteSystem Instance => instance;

        private EmoteLauncher emoteLauncher;

        private PhotonView PV
        {
            get
            {
                return playerVisuals.playerAvatar.photonView;
            }
        }

        private void Awake()
        {
            if (Camera.main != null) camTransform = Camera.main.transform;
        }

        public void SetPlayerAvatar(PlayerAvatar pa)
        {
            playerAvatar = pa;
            StartCoroutine(SetVisuals());
        }

        private IEnumerator SetVisuals()
        {
            yield return new WaitForSeconds(0.75f);

            if (playerAvatar != null)
            {
                playerVisuals = playerAvatar.playerAvatarVisuals;
                if (playerVisuals == null)
                {
                    playerVisuals = playerAvatar.transform.parent.GetComponentInChildren<PlayerAvatarVisuals>();
                }
            }


            if (PV.IsMine)
            {
                instance = this;
            }

            if (GameManager.Multiplayer())
            {
                emoteLauncher = EmoteLoader.Instance.LoadEmote(playerAvatar);
                emoteLauncher.emoteSystem = this;
            }
        }

        public void PlayEmote(string emoteId)
        {
            if (!PV.IsMine) return;

            EmoteSelectionManager.Instance.SetVisible(false);
            IsEmoting = true;
            photonView.RPC(nameof(RPC_PlayEmote), RpcTarget.All, emoteId);
        }

        public void StopEmote()
        {
            if (!PV.IsMine) return;

            EmoteSelectionManager.Instance.SetVisible(false);
            IsEmoting = false;
            photonView.RPC(nameof(RPC_StopEmote), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_PlayEmote(string emoteId)
        {
            emoteLauncher.SetRotation(playerAvatar.transform.rotation);
            emoteLauncher.Animate(emoteId);
            SomeEmotesREPO.Logger.LogInfo($"[{photonView.Owner.NickName}] played emote {emoteId}.");
        }

        [PunRPC]
        private void RPC_StopEmote()
        {
            emoteLauncher.StopEmote();
        }

        void Update()
        {
            if (IsEmoting)
            {
                emoteTime += Time.deltaTime;

                if (IsPlayerTakeControlBack())
                {
                    IsEmoting = false;
                    StopEmote();
                }
            }

            if (playerVisuals)
            {
                if (PV.IsMine && Input.GetKeyDown(KeyCode.P))
                {
                    EmoteSelectionManager.Instance.SetVisible(!EmoteSelectionManager.Instance.Visible);
                }

                if (!PV.IsMine)
                {
                    playerVisuals.animator.enabled = !IsEmoting;
                    playerVisuals.meshParent.SetActive(!IsEmoting);
                }
                else
                {
                    //according to repo PlayerAvatarVisuals.Start(), ignores the visuals for the client (always hidden)                    
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                isEmoting = (bool)stream.ReceiveNext();
                emoteId = (int)stream.ReceiveNext();
            }
            else if (stream.IsReading)
            {
                stream.SendNext(isEmoting);
                stream.SendNext(emoteId);
            }
        }

        public bool IsPlayerTakeControlBack()
        {
            if (!PV.IsMine)
                return false;

            Vector2 move = InputManager.instance.GetMovement();
            if (move.sqrMagnitude > 0.01f)
                return true;

            if (InputManager.instance.KeyDown(InputKey.Jump) ||
                InputManager.instance.KeyDown(InputKey.Interact) ||
                InputManager.instance.KeyDown(InputKey.Sprint) ||
                InputManager.instance.KeyDown(InputKey.Crouch) ||
                InputManager.instance.KeyDown(InputKey.Tumble))
            {
                return true;
            }

            return false;
        }
    }
}

