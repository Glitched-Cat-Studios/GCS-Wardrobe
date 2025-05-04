using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlayFab.ClientModels;
using PlayFab;

using Photon.VR;
using Oculus.Platform.Models;
using System;
using System.Runtime.Serialization;



#if PHOTONVR_004
using Photon.VR.Cosmetics;
#elif PHOTONVR_005
using Photon.VR.Saving;
#endif

namespace GlitchedCatStudios.Wardrobe
{
    public class GcsWardrobeManager : MonoBehaviour
    {
        [Obsolete("GcsWardrobeManager's instance will be removed next update (1.3). We highly suggest you switch to an inspector assigned instance instead to avoid errors.")]
        internal static GcsWardrobeManager instance { get; private set; }

        [Header("This package was made by Glitched Cat Studios!")]
        [Header("Please give credits in your game if you use this!\nFor more info, check the readme file in the Assets/Glitched Cat Studios/Wardrobe folder.")]
        [Space]

        [SerializeField] internal List<GcsHeadCosmetic> headCosmetics = new List<GcsHeadCosmetic>();
        [Space]
        [SerializeField] internal List<GcsFaceCosmetic> faceCosmetics = new List<GcsFaceCosmetic>();
        [Space]
        [SerializeField] internal List<GcsBodyCosmetic> bodyCosmetics = new List<GcsBodyCosmetic>();
        [Space]
        [SerializeField] internal List<GcsHoldableCosmetic> holdableCosmetics = new List<GcsHoldableCosmetic>();
        [Space]


        [Header("Make a seperate catalog in the Economy Tab of PlayFab,\nAnd name it 'Cosmetics'.\nThat's where all your items will be kept!")]
        [Space]
        [SerializeField] internal string catalogName = "Cosmetics";

        [Header("The materials for the buttons")]
        [Space]
        [SerializeField] internal Material unselectedButton;
        [SerializeField] internal Material selectedButton;

        [Header("Slots")]
        [SerializeField] internal List<GcsWardrobeSlot> slots = new List<GcsWardrobeSlot>(3);


        internal GcsWardrobeSelectedTab selectedTab = 0;
        private int tabIndex = 0;

#if PHOTONVR_004
        internal PhotonVRCosmeticsData cosData = new PhotonVRCosmeticsData();
#elif PHOTONVR_005
        internal Dictionary<string, string> cosData = new Dictionary<string, string>();
#endif

        private List<GameObject> CosmeticsInSlots = new List<GameObject>();

        private float HeadAbsTabCount = 0;
        private float FaceAbsTabCount = 0;
        private float BodyAbsTabCount = 0;
        private float HoldAbsTabCount = 0;

        private List<GcsHeadCosmetic> OwnedHeadCosmetics = new List<GcsHeadCosmetic>();
        private List<GcsFaceCosmetic> OwnedFaceCosmetics = new List<GcsFaceCosmetic>();
        private List<GcsBodyCosmetic> OwnedBodyCosmetics = new List<GcsBodyCosmetic>();
        private List<GcsHoldableCosmetic> OwnedHoldableCosmetics = new List<GcsHoldableCosmetic>();

#if PHOTONVR_005
        private string Head = "Head";
        private string Face = "Face";
        private string Body = "Body";
        private string LeftHand = "LeftHand";
        private string RightHand = "RightHand";
#endif


        private void Awake()
        {
            if (instance == null) instance = this;

#if UNITY_EDITOR && !PHOTONVR_004 && !PHOTONVR_005
            throw new SymbolNotSetException("You have not set a scripting define symbol for GCS Wardrobe. Please make sure you set either PHOTONVR_004 or PHOTONVR_005 in Player Settings.");
#else
            StartCoroutine(Login());
#endif
        }

        public void ReloadWardrobe()
        {
            OwnedHeadCosmetics.Clear();
            OwnedFaceCosmetics.Clear();
            OwnedBodyCosmetics.Clear();
            OwnedHoldableCosmetics.Clear();

            HeadAbsTabCount = 0;
            FaceAbsTabCount = 0;
            BodyAbsTabCount = 0;
            HoldAbsTabCount = 0;

            foreach (var cosmetic in CosmeticsInSlots)
            {
                Destroy(cosmetic);
            }

            CosmeticsInSlots.Clear();

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
#if PHOTONVR_004
                    cosData = JsonUtility.FromJson<PhotonVRCosmeticsData>(PlayerPrefs.GetString("Cosmetics"));
#elif PHOTONVR_005
                    cosData = PhotonVRValueSaver.GetDictionary("Cosmetics");

                if (!cosData.ContainsKey(Head))
                {
                    cosData.Add(Head, "");
                }

                if (!cosData.ContainsKey(Face))
                {
                    cosData.Add(Face, "");
                }

                if (!cosData.ContainsKey(Body))
                {
                    cosData.Add(Body, "");
                }

                if (!cosData.ContainsKey(LeftHand))
                {
                    cosData.Add(LeftHand, "");
                }

                if (!cosData.ContainsKey(RightHand))
                {
                    cosData.Add(RightHand, "");
                }

                PhotonVRValueSaver.SaveDictionary("Cosmetics", cosData);
#endif
                
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
            float HeadTabCount = HeadCosCount / slots.Count;
            HeadAbsTabCount = Mathf.Ceil(HeadTabCount);
            if (HeadAbsTabCount == 0f)
                HeadAbsTabCount = 1f;

            float FaceCosCount = OwnedFaceCosmetics.Count;
            float FaceTabCount = FaceCosCount / slots.Count;
            FaceAbsTabCount = Mathf.Ceil(FaceTabCount);
            if (FaceAbsTabCount == 0f)
                FaceAbsTabCount = 1f;

            float BodyCosCount = OwnedBodyCosmetics.Count;
            float BodyTabCount = BodyCosCount / slots.Count;
            BodyAbsTabCount = Mathf.Ceil(BodyTabCount);
            if (BodyAbsTabCount == 0f)
                BodyAbsTabCount = 1f;

            float HoldCosCount = OwnedHoldableCosmetics.Count;
            float HoldTabCount = HoldCosCount / slots.Count;
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

            foreach (var cosmetic in CosmeticsInSlots)
            {
                Destroy(cosmetic);
            }
            CosmeticsInSlots.Clear();

            List<GameObject> currentCosmetics = new List<GameObject>();
            int cosmeticCount = 0;

            float absTabCount = 0;

            switch (tab)
            {
                case GcsWardrobeSelectedTab.Head:
                    currentCosmetics = OwnedHeadCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                    cosmeticCount = OwnedHeadCosmetics.Count;
                    absTabCount = HeadAbsTabCount;
                    break;

                case GcsWardrobeSelectedTab.Face:
                    currentCosmetics = OwnedFaceCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                    cosmeticCount = OwnedFaceCosmetics.Count;
                    absTabCount = FaceAbsTabCount;
                    break;

                case GcsWardrobeSelectedTab.Body:
                    currentCosmetics = OwnedBodyCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                    cosmeticCount = OwnedBodyCosmetics.Count;
                    absTabCount = BodyAbsTabCount;
                    break;

                case GcsWardrobeSelectedTab.Holdable:
                    currentCosmetics = OwnedHoldableCosmetics.ConvertAll(cosmetic => cosmetic.prefab);
                    cosmeticCount = OwnedHoldableCosmetics.Count;
                    absTabCount = HoldAbsTabCount;
                    break;
            }

            switch (switchType)
            {
                case GcsWardrobeTabSwitchType.LeftArrow:
                    if (tabIndex == 1)
                    {
                        tabIndex = (int)absTabCount;
                    }
                    else
                    {
                        tabIndex -= 1;
                    }
                    break;

                case GcsWardrobeTabSwitchType.RightArrow:
                    if (tabIndex >= absTabCount)
                    {
                        tabIndex = 1;
                    }
                    else
                    {
                        tabIndex += 1;
                    }
                    break;

                case GcsWardrobeTabSwitchType.AnotherTab:
                    tabIndex = 1;
                    break;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                int cosmeticIndex = (tabIndex * slots.Count) - slots.Count + i;

                //uncomment if any OutOfRangeExceptions pop up
                //Debug.Log($"Tab Index: {tabIndex}, Slots Count: {slots.Count}, Cosmetic Index: {cosmeticIndex}, Current Cosmetics Count: {currentCosmetics.Count}");

                if (cosmeticIndex >= 0 && cosmeticIndex < currentCosmetics.Count)
                {
                    GameObject cos = Instantiate(currentCosmetics[cosmeticIndex], slots[i].transform);
                    CosmeticsInSlots.Add(cos);
                }
            }


            switch (tab)
            {
                case GcsWardrobeSelectedTab.Head:
                    for (int i = 0; i < Mathf.Min(slots.Count, CosmeticsInSlots.Count); i++)
                    {
                        int cosmeticIndex = ((tabIndex - 1) * slots.Count) + i;

                        var wardrobeInfo = CosmeticsInSlots[i].GetComponentInParent<GcsWardrobeInfo>();
                        if (wardrobeInfo == null)
                        {
                            Debug.LogError($"CosmeticsInSlots[{i}] does not have GcsWardrobeInfo");
                            continue;
                        }

                        wardrobeInfo.type = GcsWardrobeCosmeticType.Head;
                        wardrobeInfo.headCosmetic = OwnedHeadCosmetics[cosmeticIndex];
                    }
                    break;

                case GcsWardrobeSelectedTab.Face:
                    for (int i = 0; i < Mathf.Min(slots.Count, CosmeticsInSlots.Count); i++)
                    {
                        int cosmeticIndex = ((tabIndex - 1) * slots.Count) + i;

                        var wardrobeInfo = CosmeticsInSlots[i].GetComponentInParent<GcsWardrobeInfo>();
                        if (wardrobeInfo == null)
                        {
                            Debug.LogError($"CosmeticsInSlots[{i}] does not have GcsWardrobeInfo");
                            continue;
                        }

                        wardrobeInfo.type = GcsWardrobeCosmeticType.Face;
                        wardrobeInfo.faceCosmetic = OwnedFaceCosmetics[cosmeticIndex];
                    }
                    break;

                case GcsWardrobeSelectedTab.Body:
                    for (int i = 0; i < Mathf.Min(slots.Count, CosmeticsInSlots.Count); i++)
                    {
                        int cosmeticIndex = ((tabIndex - 1) * slots.Count) + i;

                        var wardrobeInfo = CosmeticsInSlots[i].GetComponentInParent<GcsWardrobeInfo>();
                        if (wardrobeInfo == null)
                        {
                            Debug.LogError($"CosmeticsInSlots[{i}] does not have GcsWardrobeInfo");
                            continue;
                        }

                        wardrobeInfo.type = GcsWardrobeCosmeticType.Body;
                        wardrobeInfo.bodyCosmetic = OwnedBodyCosmetics[cosmeticIndex];
                    }
                    break;

                case GcsWardrobeSelectedTab.Holdable:
                    for (int i = 0; i < Mathf.Min(slots.Count, CosmeticsInSlots.Count); i++)
                    {
                        int cosmeticIndex = ((tabIndex - 1) * slots.Count) + i;

                        var wardrobeInfo = CosmeticsInSlots[i].GetComponentInParent<GcsWardrobeInfo>();
                        if (wardrobeInfo == null)
                        {
                            Debug.LogError($"CosmeticsInSlots[{i}] does not have GcsWardrobeInfo");
                            continue;
                        }

                        wardrobeInfo.type = GcsWardrobeCosmeticType.Holdable;
                        wardrobeInfo.holdableCosmetic = OwnedHoldableCosmetics[cosmeticIndex];
                    }
                    break;
            }

            UpdateButtonColor();
        }

        internal void HandleWardrobeSwitching(int tab)
        {
            switch (tab)
            {
                case 0:
                    HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Head);
                    selectedTab = GcsWardrobeSelectedTab.Head;

                    UpdateButtonColor();
                    break;

                case 1:
                    HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Face);
                    selectedTab = GcsWardrobeSelectedTab.Face;

                    UpdateButtonColor();
                    break;

                case 2:
                    HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Holdable);
                    selectedTab = GcsWardrobeSelectedTab.Holdable;

                    UpdateButtonColor();
                    break;

                case 3:
                    HandleTabSwitching(GcsWardrobeTabSwitchType.AnotherTab, GcsWardrobeSelectedTab.Body);
                    selectedTab = GcsWardrobeSelectedTab.Body;

                    UpdateButtonColor();
                    break;
            }
        }

        internal void ToggleHeadCosmetic(GcsHeadCosmetic cosmetic)
        {
#if PHOTONVR_004
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
#elif PHOTONVR_005
            if (cosData[Head] == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(Head, string.Empty);

                cosData[Head] = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(Head, cosmetic.name);

                cosData[Head] = cosmetic.name;
            }
#endif
            UpdateButtonColor();
        }

        internal void ToggleFaceCosmetic(GcsFaceCosmetic cosmetic)
        {
#if PHOTONVR_004
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
#elif PHOTONVR_005
            if (cosData[Face] == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(Face, string.Empty);

                cosData[Face] = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(Face, cosmetic.name);

                cosData[Face] = cosmetic.name;
            }
#endif

            UpdateButtonColor();
        }

        internal void ToggleBodyCosmetic(GcsBodyCosmetic cosmetic)
        {
#if PHOTONVR_004
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
#elif PHOTONVR_005
            if (cosData[Body] == cosmetic.name)
            {
                PhotonVRManager.SetCosmetic(Body, string.Empty);

                cosData[Body] = string.Empty;
            }
            else
            {
                PhotonVRManager.SetCosmetic(Body, cosmetic.name);

                cosData[Body] = cosmetic.name;
            }
#endif

            UpdateButtonColor();
        }

        internal void ToggleHoldableCosmetic(GcsHoldableCosmetic cosmetic)
        {
#if PHOTONVR_004
            switch (cosmetic.holdableType)
            {
                case GcsWardrobeHoldableType.Left:
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
                    break;

                case GcsWardrobeHoldableType.Right:
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
                    break;

                case GcsWardrobeHoldableType.Both:
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
                    break;
            }
#elif PHOTONVR_005
            switch (cosmetic.holdableType)
            {
                case GcsWardrobeHoldableType.Left:
                    if (cosData[LeftHand] == cosmetic.name)
                    {
                        PhotonVRManager.SetCosmetic(LeftHand, string.Empty);

                        cosData[LeftHand] = string.Empty;
                    }
                    else
                    {
                        PhotonVRManager.SetCosmetic(LeftHand, cosmetic.name);

                        cosData[LeftHand] = cosmetic.name;
                    }
                    break;

                case GcsWardrobeHoldableType.Right:
                    if (cosData[RightHand] == cosmetic.name)
                    {
                        PhotonVRManager.SetCosmetic(RightHand, string.Empty);

                        cosData[RightHand] = string.Empty;
                    }
                    else
                    {
                        PhotonVRManager.SetCosmetic(RightHand, cosmetic.name);

                        cosData[RightHand] = cosmetic.name;
                    }
                    break;

                case GcsWardrobeHoldableType.Both:
                    if (cosData[LeftHand] == cosmetic.name && cosData[RightHand] == cosmetic.name)
                    {
                        PhotonVRManager.SetCosmetic(LeftHand, string.Empty);
                        PhotonVRManager.SetCosmetic(RightHand, string.Empty);
                        cosData[LeftHand] = string.Empty;
                        cosData[RightHand] = string.Empty;
                    }
                    else
                    {
                        PhotonVRManager.SetCosmetic(LeftHand, cosmetic.name);
                        PhotonVRManager.SetCosmetic(RightHand, cosmetic.name);
                        cosData[LeftHand] = cosmetic.name;
                        cosData[RightHand] = cosmetic.name;
                    }
                    break;

            }
#endif

            UpdateButtonColor();
        }

        private void UpdateButtonColor()
        {
            ResetButtonMaterials();

            GcsWardrobeButtonManager[] buttonManagers = GetComponentsInChildren<GcsWardrobeButtonManager>();

            foreach (var buttonManager in buttonManagers)
            {
                buttonManager.UpdateButtonColor();
            }
        }

        private void ResetButtonMaterials()
        {
            GcsWardrobeButtonManager[] buttons = GetComponentsInChildren<GcsWardrobeButtonManager>();
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
        public string name = string.Empty;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsFaceCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name = string.Empty;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsBodyCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name = string.Empty;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;
    }

    [System.Serializable]
    public class GcsHoldableCosmetic
    {
        [Header("Name of the Photon/PlayFab Cosmetic")]
        public string name = string.Empty;

        [Header("Just a prefab variant on the cosmetic\nMake sure you set the Position to 0, 0, 0!")]
        public GameObject prefab;

        [Header("What type of cosmetic it is.")]
        public GcsWardrobeHoldableType holdableType;
    }

    [System.Serializable]
    public class GcsWardrobeSlot
    {
        [Header("The transform of the slot")]
        public Transform transform;

        [Header("For storing internal data\nShould be on the slot object")]
        public GcsWardrobeInfo info;
    }

    public enum GcsWardrobeHoldableType : byte
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

    public class SymbolNotSetException : Exception
    {
        public SymbolNotSetException() : base() { }
        public SymbolNotSetException(string message) : base(message) { }
        public SymbolNotSetException(string message, Exception innerException) : base(message, innerException) { }
        protected SymbolNotSetException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}
