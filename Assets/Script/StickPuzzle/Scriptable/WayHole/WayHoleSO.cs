using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WayReferences", menuName = "Scriptable/WayReferences")]
public class WayHoleSO : ScriptableObject
{
    public GameObject wayPrefab;
    [Serializable]
    public class WayCombo
    {
        public List<Sprite> sprites;
        public Sprite horizontal;
        public Sprite vertical;
        public Sprite spr45;
        public Color plain;
        public Color grass;
        public Material wayMat;
        public bool IsContain(Sprite sprite)
        {
            return sprites.Contains(sprite);
        }
    }
    public GameObject grassPrefab;
    public List<WayCombo> combos;
    public WayCombo FindCombo(Sprite way)
    {
        return combos.Find((x) => x.IsContain(way));
    }
    [Serializable]
    public class SpriteRef
    {
        public Sprite hole;
        public Sprite way;
    }

    public List<SpriteRef> refers;

    public void FindAndSet(Way way, Sprite toFind)
    {
        if (toFind == null) return;
        SpriteRef refer = refers.Find((x) => x.hole == toFind);
        if (refer != null)
            SetSpriteRef(way, refer);
    }
    void SetSpriteRef(Way way, SpriteRef refer)
    {
        
    }
}
