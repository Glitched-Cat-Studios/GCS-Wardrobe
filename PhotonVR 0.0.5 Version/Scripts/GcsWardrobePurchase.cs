using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GlitchedCatStudios.Wardrobe.Purchasing
{
    public class GcsWardrobePurchase : MonoBehaviour
    {
        [Header("This is deprecated, and is no longer receiving updates.\nPlease use the shop system instead!")]
        [Header("This script is made by Glitched Cat Studios!\n(Thanks 1stGen for fixing the duplicated cosmetics issue)")]
        [Header("Purchase")]
        public string itemId;
        public int price;
        public string catalogName = "Cosmetics";
        public string currencyCode = "HS";
        public string HandTag = "HandTag";

        [Header("Get Cosmetic")]
        public TextMeshPro priceText;

        private bool hasPurchased = false;
        private bool purchaseInProgress = false;
        private bool hasLoadedCosmetics = false;

        private void Start()
        {
            priceText.text = price.ToString();

            StartCoroutine(LoadCosmetics());
        }

        IEnumerator LoadCosmetics()
        {
            yield return new WaitUntil(() => PlayFabClientAPI.IsClientLoggedIn());

            if (!hasLoadedCosmetics)
            {
                hasLoadedCosmetics = true;
                GetUserInventoryRequest request = new GetUserInventoryRequest();
                PlayFabClientAPI.GetUserInventory(request, OnGetInventorySuccess, OnError);
            }
        }

        private void OnGetInventorySuccess(GetUserInventoryResult result)
        {
            foreach (var cosmetic in result.Inventory)
            {
                if (cosmetic.CatalogVersion == catalogName && itemId == cosmetic.ItemId)
                {
                    gameObject.SetActive(false);
                    break;
                }
            }
        }

        private void OnError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(HandTag) && !purchaseInProgress && !hasPurchased)
            {
                PurchaseItem();
            }
        }

        private void PurchaseItem()
        {
            if (!hasPurchased && !purchaseInProgress)
            {
                purchaseInProgress = true;

                PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
                {
                    CatalogVersion = catalogName,
                    ItemId = itemId,
                    VirtualCurrency = currencyCode,
                    Price = price
                }, result =>
                {
                    hasPurchased = true;
                    purchaseInProgress = false;

                    GcsWardrobeManager.instance.ReloadWardrobe();

                    gameObject.SetActive(false);

                }, error =>
                {
                    purchaseInProgress = false;
                    Debug.LogError(error.GenerateErrorReport());
                });
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GcsWardrobePurchase))]
    public class GcsWardrobePurchaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GcsWardrobePurchase manager = (GcsWardrobePurchase)target;

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.8f, 0.8f, 1.0f);
            if (GUILayout.Button("Check out the Shop System!", GUILayout.Width(175)))
            {
                Application.OpenURL("https://github.com/Glitched-Cat-Studios/GCS-Shop-System");
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }
    }
#endif
}
