# MyGridLayout
unity，gridlayout，item无限循环，无限数据量，支持任意动态增删数据

unity 2021.3.10 测试通过

内含示例以及说明，使用简单方便，仅需两步。


1、首先实现自己的适配器
```c#
public class MyAdapter : GridLayoutAdapter<Item>
{

    WeakReference<BattleBagScript> mWeakReference;
    GameObject gridItemUIPrefab;

    public MyAdapter(List<Item> datas, BattleBagScript battleBagScript, GameObject gridItemUIPrefab) : base(datas)
    {
        this.mWeakReference = new WeakReference<BattleBagScript>(battleBagScript);
        this.gridItemUIPrefab = gridItemUIPrefab;
    }

    public override GameObject GetGridItemView(int index, Transform parent)
    {
        return GameObject.Instantiate(gridItemUIPrefab, parent);
    }

    public override void BindView(GameObject gridItemView, int index)
    {
        Item item = this.datas[index];
        gridItemView.GetComponentInChildren<Text>().text = item.itemId + "_" + item.itemName + "_" + item.itemCount; 
        gridItemView.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + item.imageName);
    }

    public override bool IsNeedAutoSelectState()
    {
        return true;
    }

    public override void OnGridItemClick(GameObject gridItem, int index)
    {
        Debug.Log("OnItemClick " + index);
    }

    public override void OnGridItemSelect(GameObject gridItem, int index)
    {
        Debug.LogWarning("OnSelectGridItem " + index);
        BattleBagScript bbs;
        mWeakReference.TryGetTarget(out bbs);
        if (index >= 0)
        {
            if (bbs != null) Debug.Log("OnGridItemSelect " + index);
        }
        else
        {
            if (bbs != null) Debug.Log("取消选中");
        }
    }
}
```

2、然后初始化设置给MyGridLayout即可，MyGridLayout无任何继承关系，很干净，具体看MyGridLayout代码，包含注释
```c#
[RequireComponent(typeof(ScrollRect))]
public class BattleBagScript : MonoBehaviour
{

    public GameObject gridItemUIPrefab;

    MyGridLayout mMyGridLayout;

    List<Item> datas;

    private void Start()
    {
        
        //初始化自己的数据
        //datas = ...

        MyAdapter myAdapter = new MyAdapter(datas, this, this.gridItemUIPrefab);

        mMyGridLayout = new MyGridLayout(this.gameObject, myAdapter);
    }

    private void Update()
    {
        mMyGridLayout.Update();
    }
    
    public void AddOrReduceData()
    {
        //datas.Insert(...); 可以任意位置增加任意数据量
        //datas.RemoveAt(...); 可以任意位置删除任意数据量
        //datas[x].x = x; 可以任意位置修改任意数据量
        mMyGridLayout.NotifyDatasetChange();
    }

}
```
