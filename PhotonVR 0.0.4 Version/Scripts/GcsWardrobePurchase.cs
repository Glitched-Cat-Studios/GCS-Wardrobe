using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace GlitchedCatStudios.Wardrobe.Purchasing
{
    public class GcsWardrobePurchase : MonoBehaviour
    {
        [Header("Purchase")]
        public string itemId;
        public int price;
        public string catalogName = "Cosmetics";
        public string currencyCode = "HS";
        public string HandTag = "HandTag";

        [Header("Get Cosmetic")]
        public TextMeshPro priceText;

        private Playfablogin playfablogin;

        private bool hasPurchased = false;

        private void Start()
        {
            playfablogin = FindObjectOfType<Playfablogin>();

            priceText.text = price.ToString();

            StartCoroutine(LoadCosmetics());
        }

        IEnumerator LoadCosmetics()
        {
            yield return new WaitForSeconds(5);

            GetUserInventoryRequest request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, OnGetInventorySuccess, OnError);
        }

        private void OnGetInventorySuccess(GetUserInventoryResult result)
        {
            foreach (var cosmetic in result.Inventory)
            {
                if (cosmetic.CatalogVersion == catalogName)
                {
                    if (itemId == cosmetic.ItemId)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
        }

        private void OnError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == HandTag)
            {
                if (price <= playfablogin.coins)
                {
                    PurchaseItem();
                }
                else
                {
                    Debug.LogWarning("Warning: Insufficient funds for buying " + itemId);
                    return;
                }
            }
        }

        private void PurchaseItem()
        {
            if (!hasPurchased)
            {
                PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
                {
                    CatalogVersion = catalogName,
                    ItemId = itemId,
                    VirtualCurrency = currencyCode,
                    Price = price

                }, result => {

                    hasPurchased = true;

                    GcsWardrobeManager.instance.ReloadWardrobe();

                    gameObject.SetActive(false);

                }, error => {

                    Debug.LogError(error.GenerateErrorReport());

                });
            }
        }
    }
}