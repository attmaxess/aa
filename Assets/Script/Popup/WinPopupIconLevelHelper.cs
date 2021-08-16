using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WinPopupIconLevelHelper : MonoBehaviour
{
    public string iconString;
    public List<string> icons = new List<string>();
    [Space(10)]
    public Sprite enemy;
    public Sprite soi2dau;
    public Sprite nguoida;
    public Sprite congchua;
    public Sprite nguoikhonglo;
    public Sprite rong2dau;
    public Sprite khobau;

    [ContextMenu("Split")]
    public void Split()
    {
        icons = iconString.Split(' ').ToList();
        icons.RemoveAll((x) => x.Equals(' ') || string.IsNullOrEmpty(x) == true);
        WinPopup winPopup = GetComponent<WinPopup>();
        winPopup.listIconLevel = new List<Sprite>();
        for (int i = 0; i < icons.Count; i++)
        {
            winPopup.listIconLevel.Add(GetSprite(icons[i]));
        }
    }
    Sprite GetSprite(string iconName)
    {
        switch (iconName)
        {
            case "Enemy": return enemy;
            case "Soi2Dau": case "MaSoi": return soi2dau;
            case "NguoiDa": return nguoida;
            case "Rong2dau": case "Rong": return rong2dau;
            case "NguoiKhongLo": return nguoikhonglo;
            case "CongChua": return congchua;
            case "Khobau": return khobau;
            default: Debug.Log("chua co " + iconName); break;
        }
        return null;
    }
}
