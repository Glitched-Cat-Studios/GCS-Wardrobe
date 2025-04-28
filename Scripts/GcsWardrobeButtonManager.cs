using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if PHOTONVR_004
using Photon.VR.Cosmetics;
#elif PHOTONVR_005
using Photon.VR.Saving;
#endif

namespace GlitchedCatStudios.Wardrobe
{
    public class GcsWardrobeButtonManager : MonoBehaviour
    {
        [SerializeField] private GcsWardrobeManager manager;
        [SerializeField] private GcsWardrobeButtonType type;
        [Space]
        public string handTag = "HandTag";

        [Header("For equip type only")]
        public int slotNumber = 1;

        private Renderer Renderer;
        private Material selected;
        private Material unselected;
        private TextMeshPro buttonText;

#if PHOTONVR_005
        private Dictionary<string, string> cosData = new Dictionary<string, string>();
        private string Head = "Head";
        private string Face = "Face";
        private string Body = "Body";
        private string LeftHand = "LeftHand";
        private string RightHand = "RightHand"; 
#endif

        private void Start()
        {
            Renderer = GetComponent<Renderer>();
            selected = manager.selectedButton;
            unselected = manager.unselectedButton;

            buttonText = GetComponentInChildren<TextMeshPro>();
        }

        internal void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(handTag))
            {
                switch (type)
                {
                    case GcsWardrobeButtonType.BackPage:
                        manager.HandleTabSwitching(GcsWardrobeTabSwitchType.LeftArrow, manager.selectedTab);
                        break;

                    case GcsWardrobeButtonType.ForwardPage:
                        manager.HandleTabSwitching(GcsWardrobeTabSwitchType.RightArrow, manager.selectedTab);
                        break;

                    case GcsWardrobeButtonType.HeadButton:
                        manager.HandleWardrobeSwitching(0);
                        break;

                    case GcsWardrobeButtonType.FaceButton:
                        manager.HandleWardrobeSwitching(1);
                        break;

                    case GcsWardrobeButtonType.BodyButton:
                        manager.HandleWardrobeSwitching(3);
                        break;

                    case GcsWardrobeButtonType.HoldableButton:
                        manager.HandleWardrobeSwitching(2);
                        break;

                    case GcsWardrobeButtonType.Equip:
                        HandleEquipButton(slotNumber - 1);
                        break;
                }

                if (type != GcsWardrobeButtonType.Equip) Renderer.material = selected;
            }
        }

        private void HandleEquipButton(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= manager.slots.Count) return;

            GcsWardrobeInfo info = manager.slots[slotIndex].info;

            if (info == null) return;

            switch (info.type)
            {
                case GcsWardrobeCosmeticType.Head:
                    if (info.headCosmetic != null && !string.IsNullOrEmpty(info.headCosmetic.name)) manager.ToggleHeadCosmetic(info.headCosmetic);
                    break;
                case GcsWardrobeCosmeticType.Face:
                    if (info.faceCosmetic != null && !string.IsNullOrEmpty(info.faceCosmetic.name)) manager.ToggleFaceCosmetic(info.faceCosmetic);
                    break;
                case GcsWardrobeCosmeticType.Body:
                    if (info.bodyCosmetic != null && !string.IsNullOrEmpty(info.bodyCosmetic.name)) manager.ToggleBodyCosmetic(info.bodyCosmetic);
                    break;
                case GcsWardrobeCosmeticType.Holdable:
                    if (info.holdableCosmetic != null && !string.IsNullOrEmpty(info.holdableCosmetic.name)) manager.ToggleHoldableCosmetic(info.holdableCosmetic);
                    break;
            }
        }

#if UNITY_EDITOR
        internal IEnumerator TriggerExitEditor(Collider skib)
        {
            yield return new WaitForSeconds(0.2f);
            OnTriggerExit(skib);
        }
#endif

        internal void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(handTag))
            {
                if (type != GcsWardrobeButtonType.Equip)
                {
                    Renderer.material = unselected;
                }
            }
        }

        internal void UpdateButtonColor()
        {
            if (type != GcsWardrobeButtonType.Equip) return;

            if (slotNumber - 1 < 0 || slotNumber - 1 >= manager.slots.Count) return;

            GcsWardrobeInfo info = manager.slots[slotNumber - 1].info;

            if (info == null) return;

            switch (info.type)
            {
                case GcsWardrobeCosmeticType.Head:
                    if (info.headCosmetic == null)
                    {
                        ResetMaterial();
                        return;
                    }

                    if (string.IsNullOrEmpty(info.headCosmetic.name))
                    {
                        ResetMaterial();
                        return;
                    }
                    break;

                case GcsWardrobeCosmeticType.Face:
                    if (info.faceCosmetic == null)
                    {
                        ResetMaterial();
                        return;
                    }

                    if (string.IsNullOrEmpty(info.faceCosmetic.name))
                    {
                        ResetMaterial();
                        return;
                    }
                    break;

                case GcsWardrobeCosmeticType.Body:
                    if (info.bodyCosmetic == null)
                    {
                        ResetMaterial();
                        return;
                    }

                    if (string.IsNullOrEmpty(info.bodyCosmetic.name))
                    {
                        ResetMaterial();
                        return;
                    }
                    break;

                case GcsWardrobeCosmeticType.Holdable:
                    if (info.holdableCosmetic == null)
                    {
                        ResetMaterial();
                        return;
                    }

                    if (string.IsNullOrEmpty(info.holdableCosmetic.name))
                    {
                        ResetMaterial();
                        return;
                    }
                    break;
            }

#if PHOTONVR_004
            PhotonVRCosmeticsData cosData = new PhotonVRCosmeticsData();
            cosData.Head = string.Empty;
            cosData.Face = string.Empty;
            cosData.Body = string.Empty;
            cosData.LeftHand = string.Empty;
            cosData.RightHand = string.Empty;

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                cosData = JsonUtility.FromJson<PhotonVRCosmeticsData>(PlayerPrefs.GetString("Cosmetics"));

            switch (info.type)
            {
                case GcsWardrobeCosmeticType.Head:
                    if (info.headCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.headCosmetic.name == cosData.Head ? selected : unselected;
                        buttonText.text = info.headCosmetic.name == cosData.Head ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Face:
                    if (info.faceCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.faceCosmetic.name == cosData.Face ? selected : unselected;
                        buttonText.text = info.faceCosmetic.name == cosData.Face ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Body:
                    if (info.bodyCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.bodyCosmetic.name == cosData.Body ? selected : unselected;
                        buttonText.text = info.bodyCosmetic.name == cosData.Body ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Holdable:
                    if (info.holdableCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        bool isEquipped = info.holdableCosmetic.name == cosData.LeftHand || info.holdableCosmetic.name == cosData.RightHand;
                        Renderer.material = isEquipped ? selected : unselected;
                        buttonText.text = isEquipped ? "UNEQUIP" : "EQUIP";
                    }
                    break;
            }

#elif PHOTONVR_005
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                cosData = PhotonVRValueSaver.GetDictionary("Cosmetics");

            if (!cosData.ContainsKey(Head)) cosData.Add(Head, "");
            if (!cosData.ContainsKey(Face)) cosData.Add(Face, "");
            if (!cosData.ContainsKey(Body)) cosData.Add(Body, "");
            if (!cosData.ContainsKey(LeftHand)) cosData.Add(LeftHand, "");
            if (!cosData.ContainsKey(RightHand)) cosData.Add(RightHand, "");

            PhotonVRValueSaver.SaveDictionary("Cosmetics", cosData);

            switch (info.type)
            {
                case GcsWardrobeCosmeticType.Head:
                    if (info.headCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.headCosmetic.name == cosData[Head] ? selected : unselected;
                        buttonText.text = info.headCosmetic.name == cosData[Head] ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Face:
                    if (info.faceCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.faceCosmetic.name == cosData[Face] ? selected : unselected;
                        buttonText.text = info.faceCosmetic.name == cosData[Face] ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Body:
                    if (info.bodyCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        Renderer.material = info.bodyCosmetic.name == cosData[Body] ? selected : unselected;
                        buttonText.text = info.bodyCosmetic.name == cosData[Body] ? "UNEQUIP" : "EQUIP";
                    }
                    break;

                case GcsWardrobeCosmeticType.Holdable:
                    if (info.holdableCosmetic == null)
                    {
                        Renderer.material = unselected;
                        buttonText.text = "EQUIP";
                    }
                    else
                    {
                        bool isEquipped = info.holdableCosmetic.name == cosData[LeftHand] || info.holdableCosmetic.name == cosData[RightHand];
                        Renderer.material = isEquipped ? selected : unselected;
                        buttonText.text = isEquipped ? "UNEQUIP" : "EQUIP";
                    }
                    break;
            }
#endif
        }

        internal void ResetMaterial()
        {
            Renderer.material = unselected;
            if (type == GcsWardrobeButtonType.Equip)
            {
                buttonText.text = "EQUIP";
            }
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GcsWardrobeButtonManager))]
    public class GcsWardrobeButtonManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GcsWardrobeButtonManager manager = (GcsWardrobeButtonManager)target;

            GUILayout.Space(15);

            if (GUILayout.Button("Test Button Press"))
            {
                GameObject Hand = GameObject.FindGameObjectWithTag(manager.handTag);
                Collider collider = Hand.GetComponent<Collider>();

                manager.OnTriggerEnter(collider);
                manager.OnTriggerExit(collider);
            }
        }
    }
#endif

    public enum GcsWardrobeButtonType
    {
        BackPage,
        Equip,
        ForwardPage,
        HeadButton,
        FaceButton,
        BodyButton,
        HoldableButton
    }
}
