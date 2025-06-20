using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SomeEmotesREPO
{
    public class EmoteLauncher : MonoBehaviour
    {
        public Animator animator;
        public EmoteSystem emoteSystem;
        public List<string> emoteNames = new List<string>();

        private Dictionary<string, AnimationClip> animationDict = new Dictionary<string, AnimationClip>();
        private AnimatorOverrideController overrideController;
        private string overrideKeyName = "Placeholder";

        private float initialRot;
        private Transform targetPlayer;

        private GameObject visuals;



        public void AddEntry(string name, AnimationClip clip)
        {
            animationDict.Add(name, clip);
        }

        public void Init(Transform target)
        {
            targetPlayer = target;
            visuals = transform.GetChild(0).gameObject;
            animator = GetComponentInChildren<Animator>();
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;

            foreach (var entry in animationDict)
            {
                emoteNames.Add(entry.Key);
            }

            InitBones();
            visuals.SetActive(false);
        }

        void Update()
        {
            if (targetPlayer)
            {
                transform.position = targetPlayer.position;
            }
        }

        void LateUpdate()
        {
            if (emoteSystem && emoteSystem.IsEmoting)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, initialRot, transform.eulerAngles.z);
            }
        }
        public void SetFavorite(string fav)
        {
            if (!string.IsNullOrEmpty(fav))
            {
                SetFavorites(new List<string> { fav });
            }
        }


        public void SetFavorites(List<string> favs)
        {
            if (emoteNames == null || emoteNames.Count == 0 || favs == null || favs.Count == 0)
                return;

            int max = Mathf.Min(EmoteSelectionManager.emotePerPages, emoteNames.Count);
            List<string> firstPageEmotes = emoteNames.Take(max).ToList();
            List<string> validFavs = favs.Where(f => firstPageEmotes.Contains(f)).Distinct().ToList();
            emoteNames.RemoveAll(e => validFavs.Contains(e));
            emoteNames.InsertRange(0, validFavs);

            EmoteLoader.Instance.SetFavorites(emoteNames.Take(max).ToList());
        }


        public void SetRotation(float _initialRot)
        {
            initialRot = _initialRot;
        }

        public bool Animate(string emoteName)
        {
            if (!animationDict.TryGetValue(emoteName, out var clip))
            {
                return false;
            }

            animator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            visuals.SetActive(true);

            overrideController[overrideKeyName] = clip;
            animator.ResetTrigger("StopEmote");
            animator.SetTrigger("TriggerEmote");
            return true;
        }

        public void StopEmote()
        {
            visuals.SetActive(false);
            animator.SetTrigger("StopEmote");
        }

        void InitBones()
        {
            leg_R = FindDeepChild(transform, "leg_R_0").GetComponent<SkinnedMeshRenderer>();
            leg_L = FindDeepChild(transform, "leg_L_0").GetComponent<SkinnedMeshRenderer>();
            body_bot = FindDeepChild(transform, "body_bot_0").GetComponent<SkinnedMeshRenderer>();
            body_top_sphere = FindDeepChild(transform, "body_top_sphere_0").GetComponent<SkinnedMeshRenderer>();
            arm_L = FindDeepChild(transform, "arm_L_0").GetComponent<SkinnedMeshRenderer>();
            arm_R = FindDeepChild(transform, "arm_R_0").GetComponent<SkinnedMeshRenderer>();
            health_LED = FindDeepChild(transform, "health_LED_0").GetComponent<SkinnedMeshRenderer>();
            health_frame = FindDeepChild(transform, "health_frame_0").GetComponent<SkinnedMeshRenderer>();
            health_shadow = FindDeepChild(transform, "health_shadow_0").GetComponent<SkinnedMeshRenderer>();
            head_bot_sphere = FindDeepChild(transform, "head_bot_sphere_0").GetComponent<SkinnedMeshRenderer>();
            head_bot_flat = FindDeepChild(transform, "head_bot_flat_0").GetComponent<SkinnedMeshRenderer>();
            head_top = FindDeepChild(transform, "head_top_0").GetComponent<SkinnedMeshRenderer>();
            eye_L = FindDeepChild(transform, "eye_L_0").GetComponent<SkinnedMeshRenderer>();
            iris_L = FindDeepChild(transform, "iris_L_0").GetComponent<SkinnedMeshRenderer>();
            eye_R = FindDeepChild(transform, "eye_R_0").GetComponent<SkinnedMeshRenderer>();
            iris_R = FindDeepChild(transform, "iris_R_0").GetComponent<SkinnedMeshRenderer>();
        }

        public void InitTexturesFrom(GameObject initial)
        {
            //HierarchyLogger.LogFullHierarchy(initial);

            if (leg_R != null) leg_R.materials = GetMaterialsFromBone(initial, "mesh_leg_r");
            if (leg_L != null) leg_L.materials = GetMaterialsFromBone(initial, "mesh_leg_l");
            if (body_bot != null) body_bot.materials = GetMaterialsFromBone(initial, "mesh_body_bot");
            if (body_top_sphere != null) body_top_sphere.materials = GetMaterialsFromBone(initial, "mesh_body_top_sphere");
            if (arm_L != null) arm_L.materials = GetMaterialsFromBone(initial, "mesh_arm_l");
            if (arm_R != null) arm_R.materials = GetMaterialsFromBone(initial, "mesh_arm_r");
            if (health_LED != null) health_LED.materials = GetMaterialsFromBone(initial, "mesh_health");
            if (health_frame != null) health_frame.materials = GetMaterialsFromBone(initial, "mesh_health frame");
            if (health_shadow != null) health_shadow.materials = GetMaterialsFromBone(initial, "mesh_health shadow");
            if (head_bot_sphere != null) head_bot_sphere.materials = GetMaterialsFromBone(initial, "mesh_head_bot_sphere");
            if (head_bot_flat != null) head_bot_flat.materials = GetMaterialsFromBone(initial, "mesh_head_bot_flat");
            if (head_top != null) head_top.materials = GetMaterialsFromBone(initial, "mesh_head_top");
            if (eye_L != null) eye_L.materials = GetMaterialsFromBone(initial, "mesh_eye_l");
            if (iris_L != null) iris_L.materials = GetMaterialsFromBone(initial, "mesh_pupil_l");
            if (eye_R != null) eye_R.materials = GetMaterialsFromBone(initial, "mesh_eye_r");
            if (iris_R != null) iris_R.materials = GetMaterialsFromBone(initial, "mesh_pupil_r");
        }

        Material[] GetMaterialsFromBone(GameObject initial, string name)
        {
            Transform mesh = FindDeepChild(initial.transform, name);
            if (mesh != null)
            {
                var materials = mesh.GetComponent<MeshRenderer>().materials;
                return materials;
            }
            return default;
        }

        Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name.ToLower() == childName.ToLower())
                    return child;

                Transform result = FindDeepChild(child, childName);
                if (result != null)
                    return result;
            }
            return null;
        }


        public SkinnedMeshRenderer
            leg_R,
            leg_L,
            body_bot,
            body_top_sphere,
            arm_L,
            arm_R,
            health_LED,
            health_frame,
            health_shadow,
            head_bot_sphere,
            head_bot_flat,
            head_top,
            eye_L,
            iris_L,
            eye_R,
            iris_R;
    }
}
