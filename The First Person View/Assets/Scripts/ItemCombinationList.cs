using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCombinationList : MonoBehaviour
{
    [System.Serializable]
    public class ItemCombination
    {
        public string item1;
        public string item2;
        public string resultItem;

        public ItemCombination(string item1, string item2, string resultItem)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.resultItem = resultItem;
        }
    }

    public List<ItemCombination> combinations;

    void Awake()
    {
        combinations = new List<ItemCombination>
        {
            new ItemCombination("Temp_Cube", "Temp_Cube2", "CombinedItem1"),
        };
    }

    public string GetCombinedItem(string item1, string item2)
    {
        foreach (var combination in combinations)
        {
            if ((combination.item1 == item1 && combination.item2 == item2) ||
                (combination.item1 == item2 && combination.item2 == item1))
            {
                return combination.resultItem;
            }
        }
        return null;
    }
}
