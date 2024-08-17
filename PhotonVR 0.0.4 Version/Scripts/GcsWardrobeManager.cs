using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using Photon.VR;
using Photon.VR.Cosmetics;
using System;

namespace GlitchedCatStudios.Wardrobe
{
    public class GcsWardrobeManager : MonoBehaviour
    {
        public static GcsWardrobeManager instance;

        [Header("This package was made by Glitched Cat Studios!")]
        [Header("Please give credits in your game if you use this!\nFor more info, check the readme file in the Assets/Glitched Cat Studios/Wardrobe folder.")]
        [Space]

        [SerializeField] internal GcsHeadCosmetic[] headCosmetics;
        [Space]
        [SerializeField] internal GcsFaceCosmetic[] faceCosmetics;
        [Space]
        [SerializeField] internal GcsBodyCosmetic[] bodyCosmetics;
        [Space]
        [SerializeField] internal GcsHoldableCosmetic[] holdableCosmetics;
        [Space]


        [Header("Make a seperate catalog in the Economy Tab of PlayFab,\nAnd name it 'Cosmetics'.\nThat's where all your items will be kept!")]
        [Space]
        public string catalogName = "Cosmetics";

        [Header("The materials for the buttons")]
        [Space]
        public Material unselectedButton;
        public Material selectedButton;

        [Header("DON'T MESS WITH")]
        [Space]
        public Transform slot1;
        public Transform slot2;
        public Transform slot3;

        internal GcsWardrobeSelectedTab selectedTab = 0;
        private int tabIndex = 0;

        internal PhotonVRCosmeticsData cosData = null;

        private GameObject CosmeticInSlot1 = null;
        private GameObject CosmeticInSlot2 = null;
        private GameObject CosmeticInSlot3 = null;

        private float HeadAbsTabCount = 0;
        private float FaceAbsTabCount = 0;
        private float BodyAbsTabCount = 0;
        private float HoldAbsTabCount = 0;

        private List<GcsHeadCosmetic> OwnedHeadCosmetics = new List<GcsHeadCosmetic>();
        private List<GcsFaceCosmetic> OwnedFaceCosmetics = new List<GcsFaceCosmetic>();
        private List<GcsBodyCosmetic> OwnedBodyCosmetics = new List<GcsBodyCosmetic>();
        private List<GcsHoldableCosmetic> OwnedHoldableCosmetics = new List<GcsHoldableCosmetic>();


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Two GCS Wardrobe Managers cannot be in the same scene.");
                Destroy(this);
            }

            StartCoroutine(Login());
        }

        public void ReloadWardrobe()
        {
            //Added this to stop cosmetics from duplicating
            OwnedHeadCosmetics.Clear();
            OwnedFaceCosmetics.Clear();
            OwnedBodyCosmetics.Clear();
            OwnedHoldableCosmetics.Clear();

            HeadAbsTabCount = 0;
            FaceAbsTabCount = 0;
            BodyAbsTabCount = 0;
            HoldAbsTabCount = 0;

            Destroy(CosmeticInSlot1);
            Destroy(CosmeticInSlot2);
            Destroy(CosmeticInSlot3);

            tabIndex = 0;

            StartCoroutine(Login());
        }

        private IEnumerator Login()
        {
            yield return new WaitUntil(() => PlayFabClientAPI.IsClientLoggedIn());

            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            (result) =>
            {
                foreach (var item in result.Inventory)
                {
                    foreach (GcsHeadCosmetic list in headCosmetics)
                    {
                        if (item.ItemId == list.name)
                        {
                            OwnedHeadCosmetics.Add(list);
                        }
                    }

                    foreach (GcsFaceCosmetic list in faceCosmetics)
                    {
                        if (item.ItemId == list.name)
                        {
                            OwnedFaceCosmetics.Add(list);
                        }
                    }

                    foreach (GcsBodyCosmetic list in bodyCosmetics)
                    {
                        if (item.ItemId == list.name)
                        {
                            OwnedBodyCosmetics.Add(list);
                        }
                    }

                    foreach (GcsHoldableCosmetic list in holdableCosmetics)
                    {
                        if (item.ItemId == list.name)
                        {
                            OwnedHoldableCosmetics.Add(list);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                    cosData = JsonUtility.FromJson<PhotonVRCosmeticsData>(PlayerPrefs.GetString("Cosmetics"));

                CalculateTabCounts();

                HandleWardrobeSwitching(0);
            },
            (error) =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        private void CalculateTabCounts()
        {
            float HeadCosCount = OwnedHeadCosmetics.Count;
            float HeadTabCount = HeadCosCount / 3;
            HeadAbsTabCount = Mathf.Ceil(HeadTabCount);
            if (HeadAbsTabCount == 0f)
                HeadAbsTabCount = 1f;

            float FaceCosCount = OwnedFaceCosmetics.Count;
            float FaceTabCount = FaceCosCount / 3;
            FaceAbsTabCount = Mathf.Ceil(FaceTabCount);
            if (FaceAbsTabCount == 0f)
                FaceAbsTabCount = 1f;

            float BodyCosCount = OwnedBodyCosmetics.Count;
            float BodyTabCount = BodyCosCount / 3;
            BodyAbsTabCount = Mathf.Ceil(BodyTabCount);
            if (BodyAbsTabCount == 0f)
                BodyAbsTabCount = 1f;

            float HoldCosCount = OwnedHoldableCosmetics.Count;
            float HoldTabCount = HoldCosCount / 3;
            HoldAbsTabCount = Mathf.Ceil(HoldTabCount);
            if (HoldAbsTabCount == 0f)
                HoldAbsTabCount = 1f;
        }

        internal void HandleTabSwitching(GcsWardrobeTabSwitchType switchType, GcsWardrobeSelectedTab tab)
        {
            GcsWardrobeInfo[] info = FindObjectsOfType<GcsWardrobeInfo>();

            foreach (var info1 in info)
            {
                info1.type = GcsWardrobeCosmeticType.Head;
                info1.headCosmetic = null;
                info1.faceCosmetic = null;
                info1.bodyCosmetic = null;
                info1.holdableCosmetic = null;
            }

            ResetButtonMaterials();

            Destroy(CosmeticInSlot1);
            Destroy(CosmeticInSlot2);
            Destroy(CosmeticInSlot3);

            CosmeticInSlot1 = null;
            CosmeticInSlot2 = null;
            CosmeticInSlot3 = null;

            List<GameObject> currentCosmetics = new List<GameObject>();
            int cosmeticCount = 0;

            float absTabCount = 0;

            if (tab == GcsWardrobeSelectedTab.Head)
            {
                currentCosmetics = OwnedHeadCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                cosmeticCount = OwnedHeadCosmetics.Count;
                absTabCount = HeadAbsTabCount;
            }
            else if (tab == GcsWardrobeSelectedTab.Face)
            {
                currentCosmetics = OwnedFaceCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                cosmeticCount = OwnedFaceCosmetics.Count;
                absTabCount = FaceAbsTabCount;
            }
            else if (tab == GcsWardrobeSelectedTab.Holdable)
            {
                currentCosmetics = OwnedHoldableCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                cosmeticCount = OwnedHoldableCosmetics.Count;
                absTabCount = HoldAbsTabCount;
            }
            else if (tab == GcsWardrobeSelectedTab.Body)
            {
                currentCosmetics = OwnedBodyCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                cosmeticCount = OwnedBodyCosmetics.Count;
                absTabCount = BodyAbsTabCount;
            }

            if (switchType == GcsWardrobeTabSwitchType.RightArrow)
            {
                if (tabIndex >= absTabCount)
                {
                    tabIndex = 1;
                }
                else
                {
                    tabIndex += 1;
                }
            }
            else if (switchType == GcsWardrobeTabSwitchType.LeftArrow)
            {
                if (tabIndex == 1)
                {
                    tabIndex = (int)absTabCount;
                }
                else
                {
                    tabIndex -= 1;
                }
            }
            else
            {
                tabIndex = 1;
            }

            int nextSlot1 = (tabIndex * 3) - 3;
            int nextSlot2 = nextSlot1 + 1;
            int nextSlot3 = nextSlot1 + 2;

            if (nextSlot1 < cosmeticCount) CosmeticInSlot1 = Instantiate(currentCosmetics[nextSlot1], slot1);
            if (nextSlot2 < cosmeticCount) CosmeticInSlot2 = Instantiate(currentCosmetics[nextSlot2], slot2);
            if (nextSlot3 < cosmeticCount) CosmeticInSlot3 = Instantiate(currentCosmetics[nextSlot3], slot3);

            if (tab == GcsWardrobeSelectedTab.Head)
            {
                if (CosmeticInSlot1 != null)
                {
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Head;
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().headCosmetic = OwnedHeadCosmetics[nextSlot1];
                }

                if (CosmeticInSlot2 != null)
                {
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Head;
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().headCosmetic = OwnedHeadCosmetics[nextSlot2];
                }
                
                if (CosmeticInSlot3 != null)
                {
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Head;
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().headCosmetic = OwnedHeadCosmetics[nextSlot3];
                }
            }
            else if (tab == GcsWardrobeSelectedTab.Face)
            {
                if (CosmeticInSlot1 != null)
                {
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Face;
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().faceCosmetic = OwnedFaceCosmetics[nextSlot1];
                }

                if (CosmeticInSlot2 != null)
                {
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Face;
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().faceCosmetic = OwnedFaceCosmetics[nextSlot2];
                }

                if (CosmeticInSlot3 != null)
                {
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Face;
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().faceCosmetic = OwnedFaceCosmetics[nextSlot3];
                }
            }
            else if (tab == GcsWardrobeSelectedTab.Body)
            {
                if (CosmeticInSlot1 != null)
                {
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Body;
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().bodyCosmetic = OwnedBodyCosmetics[nextSlot1];
                }

                if (CosmeticInSlot2 != null)
                {
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Body;
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().bodyCosmetic = OwnedBodyCosmetics[nextSlot2];
                }
                    
                if (CosmeticInSlot3 != null)
                {
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Body;
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().bodyCosmetic = OwnedBodyCosmetics[nextSlot3];
                }
            }
            else
            {
                if (CosmeticInSlot1 != null)
                {
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Holdable;
                    CosmeticInSlot1.GetComponentInParent<GcsWardrobeInfo>().holdableCosmetic = OwnedHoldableCosmetics[nextSlot1];
                }

                if (CosmeticInSlot2 != null)
                {
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Holdable;
                    CosmeticInSlot2.GetComponentInParent<GcsWardrobeInfo>().holdableCosmetic = OwnedHoldableCosmetics[nextSlot2];
                } 

                if (CosmeticInSlot3 != null)
                {
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().type = GcsWardrobeCosmeticType.Holdable;
                    CosmeticInSlot3.GetComponentInParent<GcsWardrobeInfo>().holdableCosmetic = OwnedHoldableCosmetics[nextSlot3];
                }
            }

            UpdateButtonColor();
        }

        internal void HandleWardrobeSwitching(int tab)
        {
            if (tab == 0)
            {
                HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Head);
                selectedTab = GcsWardrobeSelectedTab.Head;

                UpdateButtonColor();
            }
            else if (tab == 1)
            {
                HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Face);
                selectedTab = GcsWardrobeSelectedTab.Face;

                UpdateButtonColor();
            }
            else if (tab == 2)
            {
                HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Holdable);
                selectedTab = GcsWardrobeSelectedTab.Holdable;

                UpdateButtonColor();
            }
            else if (tab == 3)
            {
                HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Body);
                selectedTab = GcsWardrobeSelectedTab.Body;

                UpdateButtonColor();
            }
        }

        internal void ToggleHeadCosmetic(GcsHeadCosmetic cosmetic)
        {
            if (cosData.Head == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Head, string.Empty);

                cosData.Head = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Head, cosmetic.name);

                cosData.Head = cosmetic.name;
            }

            UpdateButtonColor();
        }

        internal void ToggleFaceCosmetic(GcsFaceCosmetic cosmetic)
        {
            if (cosData.Face == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Face, string.Empty);

                cosData.Face = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Face, cosmetic.name);

                cosData.Face = cosmetic.name;
            }

            UpdateButtonColor();
        }

        internal void ToggleBodyCosmetic(GcsBodyCosmetic cosmetic)
        {
            if (cosData.Body == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Body, string.Empty);

                cosData.Body = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(CosmeticType.Body, cosmetic.name);

                cosData.Body = cosmetic.name;
            }

            UpdateButtonColor();
        }

        internal void ToggleHoldableCosmetic(GcsHoldableCosmetic cosmetic)
        {
            if (cosmetic.holdableType == GcsWardrobeHoldableType.Left)
            {
                if (cosData.LeftHand == cosmetic.name)
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.LeftHand, string.Empty);

                    cosData.LeftHand = string.Empty;
                }
                else
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.LeftHand, cosmetic.name);

                    cosData.LeftHand = cosmetic.name;
                }

            }
            else if (cosmetic.holdableType == GcsWardrobeHoldableType.Right)
            {
                if (cosData.RightHand == cosmetic.name)
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.RightHand, string.Empty);

                    cosData.RightHand = string.Empty;
                }
                else
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.RightHand, cosmetic.name);

                    cosData.RightHand = cosmetic.name;
                }
            }
            else if (cosmetic.holdableType == GcsWardrobeHoldableType.Both)
            {
                if (cosData.LeftHand == cosmetic.name && cosData.RightHand == cosmetic.name)
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.LeftHand, string.Empty);
                    PhotonVRManager.SetCosmetic(CosmeticType.RightHand, string.Empty);
                    cosData.LeftHand = string.Empty;
                    cosData.RightHand = string.Empty;
                }
                else
                {
                    PhotonVRManager.SetCosmetic(CosmeticType.LeftHand, cosmetic.name);
                    PhotonVRManager.SetCosmetic(CosmeticType.RightHand, cosmetic.name);
                    cosData.LeftHand = cosmetic.name;
                    cosData.RightHand = cosmetic.name;
                }
            }

            UpdateButtonColor();
        }

        private void UpdateButtonColor()
        {
            GcsWardrobeButtonManager[] buttonManagers = FindObjectsOfType<GcsWardrobeButtonManager>();

            foreach (var buttonManager in buttonManagers)
            {
                buttonManager.UpdateButtonColor();
            }
        }

        public void ResetButtonMaterials()
        {
            GcsWardrobeButtonManager[] buttons = FindObjectsOfType<GcsWardrobeButtonManager>();
            foreach (var button in buttons)
            {
                button.ResetMaterial();
            }
        }

    }

    [System.Serializable]
    public class GcsHeadCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsFaceCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsBodyCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsHoldableCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;

        [Header("What type of cosmetic it is.")]
        public GcsWardrobeHoldableType holdableType;
    }



    public enum GcsWardrobeHoldableType
    {
        Right = 0,
        Left = 1,
        Both = 3
    }

    public enum GcsWardrobeTabSwitchType
    {
        RightArrow,
        LeftArrow,
        AnotherTab
    }

    public enum GcsWardrobeSelectedTab
    {
        Head,
        Face,
        Body,
        Holdable
    }
}
