using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[Serializable]
public class MyWeightable
{
    [PropertyOrder(10)] public float weight;

    [NonSerialized]
    public float CustomWeight = 0;

    public float WeightGetter
    {
        get
        {
            if (CustomWeight > 0)
            {
                return CustomWeight;
            }
            else
                return weight;
        }
    }

    // Input: List of all item
    // return index of selected item afer randomization based on weight
    public static T Randomize<T>(List<T> items, Random randomizer = null) where T : MyWeightable
    {
        T SelectedItem = null;
        float totalWeight = 0;

        foreach (var item in items)
        {
            totalWeight += item.WeightGetter;
        }

        float randomValue = randomizer != null ? (float)(randomizer.NextDouble() * totalWeight) : UnityEngine.Random.Range(0, totalWeight);
        foreach (T item in items)
        {
            randomValue -= item.WeightGetter;
            if (randomValue <= 0)
            {
                SelectedItem = item;
                break;
            }
        }

        //reset custom weight for each time draw rewards!
        foreach (var item in items)
        {
            item.CustomWeight = 0;
        }

        return SelectedItem;
    }
}
