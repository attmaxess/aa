using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class HintBaseProperties : BaseLevelProperties
{
    public GameObject handPrefab;
    public HandHint hand
    {
        get
        {
            if (_hand == null) _hand = GetComponentInChildren<HandHint>();
            if (_hand == null) CreateHand();
            return _hand;
        }
    }
    HandHint _hand;
    void CreateHand()
    {
        _hand = Instantiate(handPrefab, level.levelHint.transform).GetComponent<HandHint>();
        _hand.name = "HandHint";        
    }    
}
