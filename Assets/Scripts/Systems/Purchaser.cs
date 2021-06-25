using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class Purchaser : MonoBehaviour, IStoreListener
{
    private const string removeADs = "remove_ad";
    private const string doubleReward = "reward_2x";
    private const string coin5000 = "gold_5000";
    private const string coin10000 = "gold_10000";

    private IStoreController controller;
    private IExtensionProvider extensions;
    private bool isInit;

    public LobbyUI lobbyUI;
    public DataManager dataManager;

    private void Start()
    {
        InitializePurchasing();
    }

    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(removeADs, ProductType.NonConsumable);
        builder.AddProduct(doubleReward, ProductType.NonConsumable);
        builder.AddProduct(coin5000, ProductType.Consumable);
        builder.AddProduct(coin10000, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }
    public void BuyRemoveAd() => BuyProductID(removeADs);
    public void BuyDoubleReward() => BuyProductID(doubleReward);
    public void BuyCoin5000() => BuyProductID(coin5000);
    public void BuyCoin10000() => BuyProductID(coin10000);

   private void BuyProductID(string productId)
    {
        // 상품 있는지, 구매 가능한지 확인
        Product product = controller.products.WithID(productId);

        if(product != null && product.availableToPurchase)
        {
            Debug.Log(string.Format("구매성공 : '{0}'", product.definition.id));

            controller.InitiatePurchase(productId);
        }
        else
        {
            Debug.Log("해당 아이템이 없거나, 판매중이 아님");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");
        isInit = false;
       this.controller = controller;
        this.extensions = extensions;

        extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result => {
            if (result)
            {
                print("복구 성공");
              
                // This does not mean anything was restored,
                // merely that the restoration process succeeded.
            }
            else
            {
                print("복구 실패");
                // Restoration failed.
            }
        });
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("초기화 실패");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("구매 실패");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var resultID = args.purchasedProduct.definition.id;
     
        if(string.Equals(resultID, removeADs, StringComparison.Ordinal))
        {
            PlayerInfo.removeADs = true;
        }
       else if (string.Equals(resultID, doubleReward, StringComparison.Ordinal))
        {
            PlayerInfo.doubleReward = true;
        }
        else if (string.Equals(resultID, coin5000, StringComparison.Ordinal))
        {
            PlayerInfo.gold += 5000;
        }
        else if (string.Equals(resultID, coin10000, StringComparison.Ordinal))
        {
            PlayerInfo.gold += 10000;
        }

        if (isInit)
        {
            lobbyUI.ActiveShopBuyMessage(true);
            lobbyUI.SettingShop();
            dataManager.SaveGold(PlayerInfo.gold);
            lobbyUI.ShopDataSave();
        }
        else isInit = true;
     
        return PurchaseProcessingResult.Complete;
    }
}