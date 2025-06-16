using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SomeEmotesREPO
{
    [RequireComponent(typeof(Animator))]
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

        private PlayerController playerController;
        private PlayerAvatarVisuals playerVisuals;

        private static Vector3 camOffset = new Vector3(0f, 0f, -3.5f);

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

        public void SetPlayerController(PlayerController pc)
        {
            playerController = pc;
            StartCoroutine(SetVisuals());
        }

        private IEnumerator SetVisuals()
        {
            yield return new WaitForSeconds(0.75f);

            if (playerController.playerAvatarScript != null)
                playerVisuals = playerController.playerAvatarScript.playerAvatarVisuals;
            if (playerVisuals == null)
                playerVisuals = playerController.GetComponentInChildren<PlayerAvatarVisuals>();

            if (PV.IsMine)
            {
                instance = this;
            }

            if (GameManager.Multiplayer())
            {
                emoteLauncher = EmoteLoader.Instance.LoadEmote(playerController);
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
            emoteLauncher.SetRotation(playerController.transform.rotation);
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
            if (Input.GetKeyDown(KeyCode.P))
            {
                EmoteSelectionManager.Instance.SetVisible(!EmoteSelectionManager.Instance.Visible);
            }

            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow))
                direction += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow))
                direction += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow))
                direction += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow))
                direction += Vector3.right;

            if (direction != Vector3.zero)
            {
                direction = direction.normalized;
                camOffset += direction * Time.deltaTime * 0.005f;
            }

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

