using Photon.VR.Cosmetics;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GlitchedCatStudios.Wardrobe
{
    public class GcsWardrobeButtonManager : MonoBehaviour
    {
        public GcsWardrobeButtonType type;
        [Space]
        public string handTag = "HandTag";

        private Renderer Renderer;

        private Material selected;
        private Material unselected;

        private TextMeshPro buttonText;

        private void Start()
        {
            Renderer = GetComponent<Renderer>();

            selected = GcsWardrobeManager.instance.selectedButton;
            unselected = GcsWardrobeManager.instance.unselectedButton;

            buttonText = GetComponentInChildren<TextMeshPro>();
        }

        internal void OnTriggerEnter(Collider other)
        {
            if (other.tag == handTag)
            {
                if (type == GcsWardrobeButtonType.BackPage)
                {
                    GcsWardrobeManager.instance.HandleTabSwitching(GcsWardrobeTabSwitchType.LeftArrow, GcsWardrobeManager.instance.selectedTab);

                    Renderer.material = selected;
                }
                else if (type == GcsWardrobeButtonType.LeftEquip)
                {
                    HandleEquipButton(GcsWardrobeManager.instance.slot1.GetComponent<GcsWardrobeInfo>());
                }
                else if (type == GcsWardrobeButtonType.MiddleEquip)
                {
                    HandleEquipButton(GcsWardrobeManager.instance.slot2.GetComponent<GcsWardrobeInfo>());
                }
                else if (type == GcsWardrobeButtonType.RightEquip)
                {
                    HandleEquipButton(GcsWardrobeManager.instance.slot3.GetComponent<GcsWardrobeInfo>());
                }
                else if (type == GcsWardrobeButtonType.ForwardPage)
                {
                    GcsWardrobeManager.instance.HandleTabSwitching(GcsWardrobeTabSwitchType.RightArrow, GcsWardrobeManager.instance.selectedTab);

                    Renderer.material = selected;
                }
                else if (type == GcsWardrobeButtonType.HeadButton)
                {
                    GcsWardrobeManager.instance.HandleWardrobeSwitching(0);

                    Renderer.material = selected;
                }
                else if (type == GcsWardrobeButtonType.FaceButton)
                {
                    GcsWardrobeManager.instance.HandleWardrobeSwitching(1);

                    Renderer.material = selected;
                }
                else if (type == GcsWardrobeButtonType.BodyButton)
                {
                    GcsWardrobeManager.instance.HandleWardrobeSwitching(3);

                    Renderer.material = selected;
                }
                else
                {
                    GcsWardrobeManager.instance.HandleWardrobeSwitching(2);

                    Renderer.material = selected;
                }
            }
        }

        internal void EditorButtonColorRevert()
        {
            StartCoroutine(EditorButtonColorRevert1());
        }

        private IEnumerator EditorButtonColorRevert1()
        {
            yield return new WaitForSeconds(0.2f);

            if (type == GcsWardrobeButtonType.BackPage)
            {
                Renderer.material = unselected;
            }
            else if (type == GcsWardrobeButtonType.ForwardPage)
            {
                Renderer.material = unselected;
            }
            else if (type == GcsWardrobeButtonType.HeadButton)
            {
                Renderer.material = unselected;
            }
            else if (type == GcsWardrobeButtonType.FaceButton)
            {
                Renderer.material = unselected;
            }
            else if (type == GcsWardrobeButtonType.BodyButton)
            {
                Renderer.material = unselected;
            }
            else if (type == GcsWardrobeButtonType.HoldableButton)
            {
                Renderer.material = unselected;
            }
        }

        internal void OnTriggerExit(Collider other)
        {
            if (other.tag == handTag)
            {
                if (type == GcsWardrobeButtonType.BackPage)
                {
                    Renderer.material = unselected;
                }
                else if (type == GcsWardrobeButtonType.ForwardPage)
                {
                    Renderer.material = unselected;
                }
                else if (type == GcsWardrobeButtonType.HeadButton)
                {
                    Renderer.material = unselected;
                }
                else if (type == GcsWardrobeButtonType.FaceButton)
                {
                    Renderer.material = unselected;
                }
                else if (type == GcsWardrobeButtonType.BodyButton)
                {
                    Renderer.material = unselected;
                }
                else if (type == GcsWardrobeButtonType.HoldableButton)
                {
                    Renderer.material = unselected;
                }
            }
        }

        private void HandleEquipButton(GcsWardrobeInfo info)
        {
            GcsWardrobeCosmeticType type = info.type;
            if (type == GcsWardrobeCosmeticType.Head)
            {
                if (info.headCosmetic != null)
                {
                    GcsWardrobeManager.instance.ToggleHeadCosmetic(info.headCosmetic);
                }
            }
            else if (type == GcsWardrobeCosmeticType.Face)
            {
                if (info.faceCosmetic != null)
                {
                    GcsWardrobeManager.instance.ToggleFaceCosmetic(info.faceCosmetic);
                }
            }
            else if (type == GcsWardrobeCosmeticType.Body)
            {
                if (info.bodyCosmetic != null)
                {
                    GcsWardrobeManager.instance.ToggleBodyCosmetic(info.bodyCosmetic);
                }
            }
            else
            {
                if (info.holdableCosmetic != null)
                {
                    GcsWardrobeManager.instance.ToggleHoldableCosmetic(info.holdableCosmetic);
                }
            }
        }

        internal void UpdateButtonColor()
        {
            PhotonVRCosmeticsData cosData = null;

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                cosData = JsonUtility.FromJson<PhotonVRCosmeticsData>(PlayerPrefs.GetString("Cosmetics"));

            if (type == GcsWardrobeButtonType.LeftEquip)
            {
                UpdateEquipButtonColor(GcsWardrobeManager.instance.slot1.GetComponent<GcsWardrobeInfo>(), cosData);
            }
            else if (type == GcsWardrobeButtonType.MiddleEquip)
            {
                UpdateEquipButtonColor(GcsWardrobeManager.instance.slot2.GetComponent<GcsWardrobeInfo>(), cosData);
            }
            else if (type == GcsWardrobeButtonType.RightEquip)
            {
                UpdateEquipButtonColor(GcsWardrobeManager.instance.slot3.GetComponent<GcsWardrobeInfo>(), cosData);
            }
        }

        private void UpdateEquipButtonColor(GcsWardrobeInfo info, PhotonVRCosmeticsData cosData)
        {
            GcsWardrobeCosmeticType type = info.type;

            if (type == GcsWardrobeCosmeticType.Head && info.headCosmetic != null)
            {
                Renderer.material = info.headCosmetic.name == cosData.Head ? selected : unselected;

                buttonText.text = info.headCosmetic.name == cosData.Head ? "UNEQUIP" : "EQUIP";
            }
            else if (type == GcsWardrobeCosmeticType.Face && info.faceCosmetic != null)
            {
                Renderer.material = info.faceCosmetic.name == cosData.Face ? selected : unselected;

                buttonText.text = info.faceCosmetic.name == cosData.Face ? "UNEQUIP" : "EQUIP";
            }
            else if (type == GcsWardrobeCosmeticType.Body && info.bodyCosmetic != null)
            {
                Renderer.material = info.bodyCosmetic.name == cosData.Body ? selected : unselected;

                buttonText.text = info.bodyCosmetic.name == cosData.Body ? "UNEQUIP" : "EQUIP";
            }
            else if (info.holdableCosmetic != null)
            {
                GcsWardrobeHoldableType holdType = info.holdableCosmetic.holdableType;

                if (holdType == GcsWardrobeHoldableType.Left)
                {
                    Renderer.material = info.holdableCosmetic.name == cosData.LeftHand ? selected : unselected;

                    buttonText.text = info.holdableCosmetic.name == cosData.LeftHand ? "UNEQUIP" : "EQUIP";
                }
                else if (holdType == GcsWardrobeHoldableType.Right)
                {
                    Renderer.material = info.holdableCosmetic.name == cosData.RightHand ? selected : unselected;

                    buttonText.text = info.holdableCosmetic.name == cosData.RightHand ? "UNEQUIP" : "EQUIP";
                }
                else
                {
                    Renderer.material = (info.holdableCosmetic.name == cosData.LeftHand && info.holdableCosmetic.name == cosData.RightHand) ? selected : unselected;

                    buttonText.text = (info.holdableCosmetic.name == cosData.LeftHand && info.holdableCosmetic.name == cosData.RightHand) ? "UNEQUIP" : "EQUIP";
                }
            }
        }

        internal void ResetMaterial()
        {
            Renderer.material = unselected;

            if (type == GcsWardrobeButtonType.LeftEquip)
            {
                buttonText.text = "EQUIP";
            }
            else if (type == GcsWardrobeButtonType.MiddleEquip)
            {
                buttonText.text = "EQUIP";
            }
            else if (type == GcsWardrobeButtonType.RightEquip)
            {
                buttonText.text = "EQUIP";
            }
        }

    }

#if UNITY_EDITOR
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
                manager.EditorButtonColorRevert();
            }
        }
    }
#endif

    public enum GcsWardrobeButtonType
    {
        BackPage,
        LeftEquip,
        MiddleEquip,
        RightEquip,
        ForwardPage,
        HeadButton,
        FaceButton,
        BodyButton,
        HoldableButton
    }
}
