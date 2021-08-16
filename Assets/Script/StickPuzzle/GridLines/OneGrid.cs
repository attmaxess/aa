using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OneGrid : MonoBehaviour
{
    public Text uiText;
    public int point
    {
        get { return this._point; }
        set { this._point = value; UpdateText(value); }
    }
    [SerializeField] int _point = 10;
    public GridListLevel gridListLevel
    {
        get
        {
            if (this._GridListLevel == null)
                this._GridListLevel = GetComponentInParent<GridListLevel>();
            return this._GridListLevel;
        }
    }
    GridListLevel _GridListLevel;

    public BigGrid bigGrid
    {
        get
        {
            if (this._bigGrid == null)
                this._bigGrid = GetComponentInParent<BigGrid>();
            return this._bigGrid;
        }
    }
    BigGrid _bigGrid;

    public OneGridTouchController touchController
    {
        get
        {
            if (!_touchController) _touchController = GetComponent<OneGridTouchController>();
            return _touchController;
        }
    }
    OneGridTouchController _touchController;

    public eGridType gridType
    {
        get { return this._gridType; }
        set { this._gridType = value; }
    }
    [SerializeField] eGridType _gridType = eGridType.Player;
    public Vector2 cordinate
    {
        get { return this._cordinate; }
        set { this._cordinate = value; }
    }
    [SerializeField] Vector2 _cordinate = Vector2.zero;

    public Image _imageAvatar;
    public Image _imageGrid;
    [Space(20)]
    public OneGridDefinition data = new OneGridDefinition();
    [SerializeField] RectTransform rectTransform = null;

    public GameObject trOnGrid1;

    [Space(20)]
    public GameObject currentFlyHealth;
    public bool IsSelected
    {
        get { return _IsSelected; }
        set { _IsSelected = value; SetSelected(value); }
    }
    [SerializeField] bool _IsSelected = false;

    void SetSelected(bool isSelect)
    {
        _imageAvatar.sprite = isSelect ? data.spriteHighlight : data.spriteDefault;
        _imageGrid.sprite = isSelect ? data.gridHighlight : data.gridDefault;
        _imageGrid.color = isSelect ? data.colorHighlight : data.colorDefault;
    }

    [ContextMenu("SelfSetup")]
    public void SelfSetup()
    {
        CalculateSceneCordinate();
        Setup(this.cordinate, this.gridType);
    }
    public void Setup(Vector2 cordinate, eGridType gridType = eGridType.TenHP)
    {
        OneGridDefinition definition = gridListLevel.grid.Find((x) => x.gridType == this.gridType);

        if (definition != null)
        {
            this.data.SyncData(definition);

            _imageAvatar.sprite = data.spriteDefault;
            _imageGrid.sprite = data.gridDefault;
            _imageGrid.color = data.colorDefault;
            rectTransform.sizeDelta = _imageAvatar.sprite != null ? new Vector2(_imageAvatar.sprite.texture.width, _imageAvatar.sprite.texture.height) : Vector2.zero;

            this.point = data.point;
            this.gridType = gridType;
            this.cordinate = cordinate;

            transform.name = this.gridType.ToString() + " " + this.cordinate + " " + this.point;
        }
    }

    public void Start()
    {
        SelfSetup();
        touchController.checkRemovePosibility += CheckBusyToResetTouchController;
    }

    public bool IsConnectAble()
    {
        return _gridType == eGridType.Player
            || _gridType == eGridType.TenHP;
    }
    public bool IsPlayer()
    {
        return _gridType == eGridType.Player;
    }

    public bool IsVerticleOrHorizontal(OneGrid other)
    {
        if (other == null) return false;
        else if (other == this) return false;
        else
        {
            if (this.cordinate.x == other.cordinate.x) return (Mathf.Abs(this.cordinate.y - other.cordinate.y) == 1f);
            else if (this.cordinate.y == other.cordinate.y) return (Mathf.Abs(this.cordinate.x - other.cordinate.x) == 1f);
            else return false;
        }
    }

    [ContextMenu("CalculateSceneCordinate")]
    public void CalculateSceneCordinate()
    {
        int childIndex = transform.GetSiblingIndex();
        int x = Mathf.RoundToInt(childIndex / bigGrid.columns);
        int y = childIndex - x * bigGrid.columns;
        this.cordinate = new Vector2(x, y);
    }
    void UpdateText(int value)
    {
        uiText.text = value != 0 ? value.ToString() : string.Empty;
    }
    void CheckBusyToResetTouchController(out bool CanReset)
    {
        CanReset = this.currentFlyHealth == null;
    }
    public void RestorePoint()
    {
        point = this.data.point;        
    }
    public bool IsNext(OneGrid other)
    {
        return (other.cordinate.x - this.cordinate.x == 0 && Mathf.Abs(other.cordinate.y - this.cordinate.y) == 1) ||
            (other.cordinate.y - this.cordinate.y == 0 && Mathf.Abs(other.cordinate.x - this.cordinate.x) == 1);
    }
}
