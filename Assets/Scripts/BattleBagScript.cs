using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class BattleBagScript : MonoBehaviour
{

    public GameObject gridItemUIPrefab;

    private MyGridLayout mMyGridLayout;
    private List<Item> datas = null;

    private void Start()
    {
        InitTestDatas();

        MyAdapter myAdapter = new MyAdapter(datas, this, this.gridItemUIPrefab);

        mMyGridLayout = new MyGridLayout(this.gameObject, myAdapter);
    }

    private void Update()
    {
        mMyGridLayout.Update();
    }



    //=================test

    private void InitTestDatas()
    {
        datas = new List<Item>();
        for(int i=0; i<99; i++)
        {
            Item item = new Item();
            item.itemId = i;
            item.itemCount = 100 + i;
            item.itemName = "name" + i;
            datas.Add(item);
        }
    }

    int i = 0, j = 0;
    public void AddTestDataFirst()
    {
        i++;
        Item a = new Item();
        a.itemCount = 1;
        a.itemName = "F_" + i;
        this.datas.Insert(0, a);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void AddTestDataLast()
    {
        j++;
        Item a = new Item();
        a.itemCount = 1;
        a.itemName = "L_" + j;
        this.datas.Add(a);

        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void ReduceTestDataFirst()
    {
        if(this.datas.Count > 0) this.datas.RemoveAt(0);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    public void ReduceTestDataLast()
    {
        if (this.datas.Count > 0) this.datas.RemoveAt(this.datas.Count - 1);
        mMyGridLayout.NotifyDatasetChange();
        Debug.Log("data size " + this.datas.Count);
    }

    //============ test

}

public class Item
{
    public int itemId;
    public string itemName;
    public int itemCount;
    public string imageName;
}

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
        //gridItemView.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + item.imageName);
    }

    public override bool IsNeedAutoSelectState()
    {
        return true;
    }

    public override void OnGridItemClick(GameObject gridItem, int index)
    {
        Debug.LogWarning("OnItemClick " + index);
    }

    public override void OnGridItemSelect(GameObject gridItem, int index)
    {
        Debug.LogWarning("OnSelectGridItem " + index);
        BattleBagScript bbs;
        mWeakReference.TryGetTarget(out bbs);
        if (index >= 0)
        {
            if (bbs != null) Debug.Log("选中 " + this.datas[index]);
        }
        else
        {
            if (bbs != null) Debug.Log("清空选中");
        }
    }
}
