using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private List<ItemScritable> itemList;
    private List<ItemScritable> _sendItemList = new List<ItemScritable>();

    /// <summary>
    /// 랜덤으로 아이템을 뽑고 리스트를 반환함
    /// </summary>
    /// <param name="onlyList">뽑아놓은 리스트만 반환</param>
    /// <returns>List<ItemScritable></returns>
    public List<ItemScritable> GetItem(bool onlyList = false)
    {
        if (onlyList) return _sendItemList;

        _sendItemList.Clear();

        var item = GameManager.GetInstance().ProbabilityItem(itemList);
        _sendItemList.Add(item);
        
        item = GameManager.GetInstance().ProbabilityItem(itemList);
        _sendItemList.Add(item);

        return _sendItemList;
    }
}
