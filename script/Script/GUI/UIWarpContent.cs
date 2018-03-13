using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIWarpContent : MonoBehaviour {

	public delegate void OnInitializeItem(GameObject go,int dataIndex);

    public OnInitializeItem onInitializeItem;
    public enum Arrangement
	{
		Horizontal,
		Vertical,
	}

	/// <summary>
	/// Type of arrangement -- vertical or horizontal.
	/// </summary>

	public Arrangement arrangement = Arrangement.Horizontal;

	/// <summary>
	/// Maximum children per line.
	/// If the arrangement is horizontal, this denotes the number of columns.
	/// If the arrangement is vertical, this stands for the number of rows.
	/// </summary>
	[Range(1,50)]
	public int maxPerLine = 1;

	/// <summary>
	/// The width of each of the cells.
	/// </summary>

	public float cellWidth = 200f;

	/// <summary>
	/// The height of each of the cells.
	/// </summary>

	public float cellHeight = 200f;

	/// <summary>
	/// The Width Space of each of the cells.
	/// </summary>
	[Range(0, 50)]
	public float cellWidthSpace = 0f;

	/// <summary>
	/// The Height Space of each of the cells.
	/// </summary>
	[Range(0, 50)]
	public float cellHeightSpace = 0f;


	[Range(0,30)]
	public int viewCount = 5;

	public ScrollRect scrollRect;

	public RectTransform content;

	public GameObject goItemPrefab;

    public string prefabBundName;
    public string prefabResName;

	private int dataCount;

	private int curScrollPerLineIndex = -1;

	private List<UIWarpContentItem> listItem;

	private Queue<UIWarpContentItem> unUseItem;

    //true时标记为WrapContent, false时标记为简单列表，不带优化功能（因为结合Toggle使用时，会出现奇奇怪怪的东西）
    public bool enableWrappable = true;


	void Awake(){
		listItem = new List<UIWarpContentItem> ();
		unUseItem = new Queue<UIWarpContentItem> ();
	}

	public void Init(int dataCount)
	{
        viewCount += 2; // 此处多加2列/行的原因是为了更好的体验，以及最后一列/行能够更好的展示

        if (scrollRect == null || content == null || (goItemPrefab == null  && string.IsNullOrEmpty(prefabBundName) && string.IsNullOrEmpty(prefabResName))) {
            //Debug.LogError ("异常:请检测<"+gameObject.name+">对象上UIWarpContent对应ScrollRect、Content、GoItemPrefab 是否存在值...."+scrollRect+" _"+content+"_"+goItemPrefab);
			return;
		}

        if (dataCount < 0) return;

        clear();
        if (dataCount == 0) return;

        setDataCount (dataCount); 
		scrollRect.onValueChanged.RemoveAllListeners();

        if (enableWrappable)
        {
            scrollRect.onValueChanged.AddListener(onValueChanged);
        }

        if (goItemPrefab == null)
        {
            AssetBundleManager.instance.GetResourceAsync<GameObject>(prefabBundName, prefabResName, (GameObject go, bool result) =>
            {
                if (result && null != go)
                {
                    goItemPrefab = go;
                    setUpdateRectItem(0);
                }
            });
        }
        else
        {
            setUpdateRectItem(0);
        } 
	}

    public void UpdateAll(int dataCount)
    {
        curScrollPerLineIndex = -1;
        switch (arrangement)
        {
            case Arrangement.Horizontal:
                scrollRect.horizontalNormalizedPosition = 0;
                break;
            case Arrangement.Vertical:
                scrollRect.verticalNormalizedPosition = 1;
                break;
        }
        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            if (i >= dataCount)
            {
                UIWarpContentItem item = listItem[i];
                item.Index = -1;
                listItem.Remove(item);
                unUseItem.Enqueue(item);
            }
        }
        setDataCount(dataCount);
        scrollRect.onValueChanged.RemoveAllListeners();
        if (enableWrappable)
        {
            scrollRect.onValueChanged.AddListener(onValueChanged);
        }
        for (int i = listItem.Count - 1; i >= 0; i--)
        {
            UIWarpContentItem item = listItem[i];
            item.Index = i;
        }
        setUpdateRectItem(0);
    }

    public void ChangeDataCount(int count)
    {
        if (dataCount == count)
        {
            return;
        }
        dataCount = count;
        setUpdateContentSize();
        setUpdateRectItem(0);
    }

    private void setDataCount(int count)
	{
		if (dataCount == count) 
		{
			return;
		}
		dataCount = count;
		setUpdateContentSize ();
	}

	private void onValueChanged(Vector2 vt2){
		if (goItemPrefab == null) {
			return;
		}

        switch (arrangement)
        {
            case Arrangement.Vertical:
                float y = vt2.y;
                if (y >= 1.0f || y <= 0.0f)
                {
                    return;
                }
                break;
            case Arrangement.Horizontal:
                float x = vt2.x;
                if (x <= 0.0f || x >= 1.0f)
                {
                    return;
                }
                break;
        }
        int _curScrollPerLineIndex = getCurScrollPerLineIndex ();
		if (_curScrollPerLineIndex == curScrollPerLineIndex){
			return;
		}
       
        setUpdateRectItem (_curScrollPerLineIndex);
	}

	/**
	 * @des:设置更新区域内item
	 * 功能:
	 * 1.隐藏区域之外对象
	 * 2.更新区域内数据
	 */
	private void setUpdateRectItem(int scrollPerLineIndex)
	{
		if (scrollPerLineIndex < 0) 
		{
			return;
		}
		curScrollPerLineIndex = scrollPerLineIndex;
		int startDataIndex = curScrollPerLineIndex * maxPerLine;
		int endDataIndex = (curScrollPerLineIndex + viewCount) * maxPerLine;
		//移除
		for (int i = listItem.Count - 1; i >= 0; i--) 
		{
			UIWarpContentItem item = listItem[i];
			int index = item.Index;
			if (index < startDataIndex || index >= endDataIndex) 
			{
				item.Index = -1;
				listItem.Remove (item);
				unUseItem.Enqueue (item);
			}
		}
		//显示
		for(int dataIndex = startDataIndex;dataIndex<endDataIndex;dataIndex++)
		{
			if (dataIndex >= dataCount) 
			{
				continue;
			}
			if (isExistDataByDataIndex (dataIndex)) 
			{
				continue;
			}
			createItem (dataIndex);
		}
	}



	/**
	 * @des:添加当前数据索引数据
	 */
	public void AddItem(int dataIndex)
	{
		if (dataIndex<0 || dataIndex > dataCount) 
		{
			return;
		}
		//检测是否需添加gameObject
		bool isNeedAdd = false;
		for (int i = listItem.Count-1; i>=0 ; i--) {
			UIWarpContentItem item = listItem [i];
			if (item.Index >= (dataCount - 1)) {
				isNeedAdd = true;
				break;
			}
		}
		setDataCount (dataCount+1);

		if (isNeedAdd) {
			for (int i = 0; i < listItem.Count; i++) {
				UIWarpContentItem item = listItem [i];
				int oldIndex = item.Index;
				if (oldIndex>=dataIndex) {
					item.Index = oldIndex+1;
				}
				item = null;
			}
			setUpdateRectItem (getCurScrollPerLineIndex());
		} else {
			//重新刷新数据
			for (int i = 0; i < listItem.Count; i++) {
				UIWarpContentItem item = listItem [i];
				int oldIndex = item.Index;
				if (oldIndex>=dataIndex) {
					item.Index = oldIndex;
				}
				item = null;
			}
		}

	}

	/**
	 * @des:删除当前数据索引下数据
	 */
	public void DelItem(int dataIndex){
		if (dataIndex < 0 || dataIndex >= dataCount) {
			return;
		}
		//删除item逻辑三种情况
		//1.只更新数据，不销毁gameObject,也不移除gameobject
		//2.更新数据，且移除gameObject,不销毁gameObject
		//3.更新数据，销毁gameObject

		bool isNeedDestroyGameObject = (listItem.Count >= dataCount);
		setDataCount (dataCount-1);

		for (int i = listItem.Count-1; i>=0 ; i--) {
			UIWarpContentItem item = listItem [i];
			int oldIndex = item.Index;
			if (oldIndex == dataIndex) {
				listItem.Remove (item);
				if (isNeedDestroyGameObject) {
					GameObject.Destroy (item.gameObject);
				} else {
					item.Index = -1;
					unUseItem.Enqueue (item);			
				}
			}
			if (oldIndex > dataIndex) {
				item.Index = oldIndex - 1;
			}
		}
		setUpdateRectItem(getCurScrollPerLineIndex());
	}


	/**
	 * @des:获取当前index下对应Content下的本地坐标
	 * @param:index
	 * @内部使用
	*/
	public Vector3 getLocalPositionByIndex(int index){
		float x = 0f;
		float y = 0f;
		float z = 0f;
		switch (arrangement) {
		case Arrangement.Horizontal: //水平方向
			x = (index / maxPerLine) * (cellWidth + cellWidthSpace);
            y = -(index % maxPerLine) * (cellHeight + cellHeightSpace);
			break;
		case  Arrangement.Vertical://垂着方向
            x =  (index % maxPerLine) * (cellWidth + cellWidthSpace);
			y = -(index / maxPerLine) * (cellHeight + cellHeightSpace);
			break;
		}

        //Debug.Log("lp: " + new Vector3(x, y, z));
		return new Vector3(x,y,z);
	}

	/**
	 * @des:创建元素
	 * @param:dataIndex
	 */
	private void createItem(int dataIndex){
		UIWarpContentItem item = null;
		if (unUseItem.Count > 0) {
			item = unUseItem.Dequeue();
		} else {
            //Debug.Log(goItemPrefab);
            //Debug.Log(content);
            if (goItemPrefab != null)
            {
                item = addChild(goItemPrefab, content).AddComponent<UIWarpContentItem>();
            } 
		}
        if (item != null)
        {
            item.WarpContent = this;
            item.Index = dataIndex;
            listItem.Add(item);
        } 
	}

	/**
	 * @des:当前数据是否存在List中
	 */
	private bool isExistDataByDataIndex(int dataIndex){
		if (listItem == null || listItem.Count <= 0) {
			return false;
		}
		for (int i = 0; i < listItem.Count; i++) {
			if (listItem [i].Index == dataIndex) {
				return true;
			}
		}
		return false;
	}


	/**
	 * @des:根据Content偏移,计算当前开始显示所在数据列表中的行或列
	 */
	private int getCurScrollPerLineIndex()
	{
        int index = 0;
        switch (arrangement)
        {
            case Arrangement.Horizontal: //水平方向
                index = Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.x) / (cellWidth + cellWidthSpace));
                break;
            case Arrangement.Vertical://垂着方向
                index = Mathf.FloorToInt(Mathf.Abs(content.anchoredPosition.y) / (cellHeight + cellHeightSpace));
                break;
            default:
                break;
        }
		return index;
	}

	/**
	 * @des:更新Content SizeDelta
	 */
	private void setUpdateContentSize()
	{
        //多加1行，是为了防止最后一列展示不出来
		int lineCount = Mathf.CeilToInt((float)dataCount/maxPerLine);
		switch (arrangement)
		{
		 case Arrangement.Horizontal:
			content.sizeDelta = new Vector2(cellWidth * lineCount + cellWidthSpace * (lineCount - 1), content.sizeDelta.y);
                break;
		 case Arrangement.Vertical:
                content.sizeDelta = new Vector2(content.sizeDelta.x, cellHeight * lineCount + cellHeightSpace * (lineCount - 1));
			break;
		}

        CenterOnChild centerOnChild = scrollRect.GetComponent<CenterOnChild>();
        if (null != centerOnChild)
        {
            if (arrangement == Arrangement.Horizontal)
            {
                centerOnChild.Init(content.sizeDelta.x);
            }
            else
            {
                centerOnChild.Init(content.sizeDelta.y);
            }
            
        }

	}

	/**
	 * @des:实例化预设对象 、添加实例化对象到指定的子对象下
	 */
	private GameObject addChild(GameObject goPrefab,Transform parent)
	{
		if (parent == null) {
			Debug.LogError("异常。UIWarpContent.cs addChild(goPrefab, parent = null)");
			return null;
		}

        if (goPrefab == null)
        {
            Debug.LogError("异常。UIWarpContent.cs addChild(goPrefab = null, parent)" + this.gameObject.name);
            return null;
        }
		GameObject goChild = GameObject.Instantiate (goPrefab) as GameObject;
		goChild.layer = parent.gameObject.layer;
        goChild.transform.SetParent (parent,false);
        goChild.gameObject.SetActive(true);

        return goChild;
	}

    public void reset()
    {

    }

	void OnDestroy(){

        clear();
        scrollRect = null;
		content = null;
		goItemPrefab = null;
		onInitializeItem = null; 
        listItem = null;
        unUseItem = null;

    }


    public void clear()
    {
        dataCount = 0;
        curScrollPerLineIndex = -1;
        for (int i = 0; i < listItem.Count; i++)
        {
            GameObject.Destroy(listItem[i].gameObject);
        }

        while(unUseItem.Count > 0)
        {
            GameObject.Destroy(unUseItem.Dequeue().gameObject);
        }

        listItem.Clear();
        unUseItem.Clear();

        switch (arrangement)
        {
            case Arrangement.Horizontal:
                scrollRect.horizontalNormalizedPosition = 0;
                break;
            case Arrangement.Vertical:
                scrollRect.verticalNormalizedPosition = 1;
                break;
        }
       
    }
}
