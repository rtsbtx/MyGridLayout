using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ע�⣺��ʱ��֧�������֣�ֻ����ק
/// ʹ�÷�ʽ�ǳ��򵥷���
/// 1������ڰ�����scroll rect��gameObject�Ľű���
/// 2���̳�GridLayoutAdapterʵ���Լ���������
/// 3����ʼ������
/// 4���Ž�Update��ִ��
/// </summary>
public class MyGridLayout
{
    GridLayoutGroup gridLayoutGroup;
    RectTransform scrollContentRectTransform;
    BaseAdapter adapter;

    //public bool isNeedDefaultSelectStateColor = true;

    //���һ�ε��������
    int lastClickGridItemIndex;

    /// <summary>
    /// ��һ֡��ƫ����
    /// </summary>
    float preScrollOffset;

    //�ɼ���������ĸ߶�(��������߶�)
    float containerHeight = 0f;

    //�����������߶ȣ������������������
    int maxHeight;

    //List<T> datas = new List<T>();

    /// <summary>
    /// �����ݿ��
    /// </summary>
    int dataSize = 0;

    //����
    int columnCount;

    //ÿ�����ӵĸ߶�
    float cellHeight;

    //����֮��Ĵ�ֱ���
    float spaceHeight;

    //�������ݵĶ������(��ʼֵ)
    int originPaddingTop;

    //��Ÿ���UI
    LinkedList<GameObject> cacheItems = new LinkedList<GameObject>();

    /// <summary>
    /// ��ǰ���ص����һ�����ݵ�����(��СֵΪ(oneScreenNeedRow+2)*columnCount)
    /// </summary>
    int cacheGridItemLastDataIndex = -1;

    //ռ��һ����Ҫ������
    int oneScreenNeedRow;

    //ռ��һ����Ҫ���ٸ�
    int oneScreenNeedItems;

    //����GameObject
    GameObject scrollContentGameObj;

    //��ǰ�ѻ�����ƫ����
    float scrollOffset;

    //��������ƫ����
    float maxScrollOffset;

    GameObject scrollRectGameObject;

    private MyGridLayout()
    {
    }

    public MyGridLayout(GameObject scrollRectGameObject, BaseAdapter adapter)
    {
        this.scrollRectGameObject = scrollRectGameObject;
        this.adapter = adapter;
        //this.datas = datas;
        InitDatas();
        InitUIDatas();
        InitItemCache();
        SelectActiveItem(0);
    }

    private void InitDatas()
    {
        //MyDBManager.GetInstance().ConnDB();
        //this.datas = MyDBManager.GetInstance().GetRoleItemInBag(1, false);

        this.dataSize = this.adapter.GetDataCount();

    }

    private void InitUIDatas()
    {

        ScrollRect sr = scrollRectGameObject.GetComponent<ScrollRect>();
        gridLayoutGroup = scrollRectGameObject.GetComponentInChildren<GridLayoutGroup>();
        scrollContentGameObj = gridLayoutGroup.gameObject;
        scrollContentRectTransform = scrollContentGameObj.GetComponent<RectTransform>();

        float containerWidth = scrollRectGameObject.transform.rectTransform().rect.width;
        Debug.Log("containerWidth " + containerWidth);
        containerHeight = scrollRectGameObject.transform.rectTransform().rect.height;
        Debug.Log("containerHeight " + containerHeight);

        //scrollview �Ƽ���ʼ������
        sr.vertical = true;
        sr.horizontal = false;
        sr.movementType = ScrollRect.MovementType.Elastic;
        sr.elasticity = 0.06f;
        sr.inertia = false;
        sr.scrollSensitivity = 0;
        //=======================================

        //gridLayoutGroup��ʼ������
        if(gridLayoutGroup.padding.top <= 0) gridLayoutGroup.padding.top = 20; //�ͻ����߼��й���
        gridLayoutGroup.padding.bottom = 20;
        gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = 5;
        Vector2 cellSize = gridLayoutGroup.cellSize;
        cellSize.x = 160;
        cellSize.y = 190;
        gridLayoutGroup.cellSize = cellSize;

        Vector2 v2 = gridLayoutGroup.spacing;
        v2.x = (containerWidth - (gridLayoutGroup.constraintCount * gridLayoutGroup.cellSize.x)) / (gridLayoutGroup.constraintCount + 1);
        v2.y = 20;
        gridLayoutGroup.spacing = v2;
        gridLayoutGroup.padding.left = (int)v2.x;
        gridLayoutGroup.padding.right = (int)v2.x;
        //====================================

        columnCount = gridLayoutGroup.constraintCount;
        cellHeight = gridLayoutGroup.cellSize.y;
        spaceHeight = gridLayoutGroup.spacing.y;

        if (originPaddingTop == 0) originPaddingTop = gridLayoutGroup.padding.top;

        int totalRows = dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1;
        maxHeight = totalRows * (int)(cellHeight + spaceHeight) + gridLayoutGroup.padding.bottom;
        Debug.Log("finalHeight " + maxHeight);

        maxScrollOffset = maxHeight - containerHeight;
        Debug.Log("maxScrollOffset " + maxScrollOffset);

        //ռ��1����Ҫ������(����պ�������������+1������Ҳû��ϵ��ֻ��������һ�еĻ������)
        oneScreenNeedRow = (int)((containerHeight - originPaddingTop) / (cellHeight + spaceHeight)) + 1;
        Debug.Log("one screen needRow " + oneScreenNeedRow);

        oneScreenNeedItems = oneScreenNeedRow * columnCount;
        Debug.Log("one screen needItems " + oneScreenNeedItems);
    }

    /// <summary>
    /// ���ݷ����κθı�󣬶���Ҫ�����������
    /// </summary>
    public void NotifyDatasetChange()
    {

        int originDataSize = this.dataSize;
        this.dataSize = this.adapter.GetDataCount();
        InitUIDatas();

        if (this.dataSize > originDataSize) //����������
        {

            GameObject lastActiveGridItem = FindLastActiveGridItem();
            if (lastActiveGridItem != null) //˵��������
            {
                Debug.Log("lastActiveDataIndex " + lastActiveGridItem.name);
                //���һ��ʣ��Ŀ�λ
                int lastRowNonUseCount;
                if ((int.Parse(lastActiveGridItem.name) + 1) % columnCount == 0)
                {
                    lastRowNonUseCount = 0;
                }
                else
                {
                    lastRowNonUseCount = columnCount - ((int.Parse(lastActiveGridItem.name) + 1) % columnCount);
                }
                Debug.Log("lastRowNonUseCount " + lastRowNonUseCount);
                //�����˶�������
                int addDataCount = this.dataSize - originDataSize;
                Debug.Log("addDataCount " + addDataCount);
                if (addDataCount > lastRowNonUseCount) //���ӵ�����������ʣ���λ
                {
                    if (originDataSize <= (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("ԭ�������ڷ�Χ��");
                        if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("��Χ�� ���ӵ� ��Χ��");
                            //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                            SetInitHeight();
                        }
                        else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("��Χ�� ���ӵ� ��Χ��");
                            //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                            SetInitHeight();
                        }
                    }
                    else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("��Χ�� �� ��Χ�⣬���账��");
                        //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                    }
                }
                else
                {
                    Debug.Log("��ӵ�������С�ڵ������ظ��ӣ����账��");
                }
            }
            else //˵���������֮ǰ������
            {
                Debug.LogWarning("�������֮ǰ������");
                if (originDataSize != 0) Debug.LogError("�����쳣 originDataSize " + originDataSize);
                SetInitHeight();
            }
            RefreshAllGridItem();

        }
        else if (this.dataSize == originDataSize) //û����������
        {

            Debug.Log("������û��");
            RefreshAllGridItem();

        }
        else //����������
        {

            Debug.Log("originDataSize " + originDataSize);
            if (originDataSize <= (oneScreenNeedItems + 2 * columnCount)) //ԭ����������Χ��
            {
                Debug.Log("��Χ�� ���� ��Χ��");
                //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                RefreshAllGridItem();
                SetInitHeight();
            }
            else if (originDataSize > (oneScreenNeedItems + 2 * columnCount)) //ԭ����������Χ��
            {
                if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount)) //��Χ�� ���� ��Χ��
                {
                    Debug.Log("��Χ�� ���� ��Χ��");

                    //cacheGridItemLastDataIndex // padding // scrollOffset // scrollAreaHeight // cahceItem data index // RefreshAllGridItem()

                    cacheGridItemLastDataIndex = (oneScreenNeedRow + 2) * columnCount - 1;

                    gridLayoutGroup.padding.top = originPaddingTop;

                    SetInitHeight();

                    AdjustGridItemDataIndexToInitState();

                    RefreshAllGridItem();

                }
                else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("��Χ�� ���� ��Χ��");
                    //gridItemLastIndex // padding // scrollOffset // scrollAreaHeight // all cahceItem data index change // RefreshAllGridItem()
                    GameObject lastActiveGridItem = FindLastActiveGridItem();
                    int lastActiveGridItemDataIndex = int.Parse(lastActiveGridItem.name);
                    if (this.dataSize <= lastActiveGridItemDataIndex) //���������̫С����������cache������Χ
                    {
                        Debug.Log("ʣ��������С��������cache item data index���ֵ����Ҫ����UI");

                        //int reduceLineCount = GetReduceLineCount(originDataSize); //todo �߼������⣬���100�����ݼ��ٵ�10���أ������Ǻܶ��У�Ӧ����Ҫ���������˶�����?

                        int exceedCount = lastActiveGridItemDataIndex - this.dataSize + 1;

                        Debug.Log("exceedCount " + exceedCount);

                        int reduceLineCount = 0;

                        if ((int.Parse(lastActiveGridItem.name) + 1) % columnCount == 0)
                        {
                            Debug.Log("���һ��������������");
                        }
                        else
                        {
                            int lastRowActiveGridItemCount = (int.Parse(lastActiveGridItem.name) + 1) % columnCount;
                            if (exceedCount < lastRowActiveGridItemCount) //���ֵ�����������ŵ� (active grid item count - 1) �㹻�ۼ����������
                            {
                                Debug.Log("���ֵ�����������ŵ� (active grid item count - 1) �㹻�ۼ����������");
                            }
                            else if (exceedCount == lastRowActiveGridItemCount) //���ֵ�����������ŵ� active grid item count �պù��ۼ��������
                            {
                                reduceLineCount++;
                            }
                            else if (exceedCount > lastRowActiveGridItemCount)
                            {
                                exceedCount -= lastRowActiveGridItemCount;
                                reduceLineCount++;
                                reduceLineCount += (exceedCount / columnCount);
                            }
                        }

                        Debug.Log("reduceLineCount " + reduceLineCount);

                        if (reduceLineCount > 0)
                        {
                            cacheGridItemLastDataIndex -= reduceLineCount * columnCount;

                            //�������������ڸ߶�
                            Vector2 sd = scrollContentRectTransform.sizeDelta;
                            sd.y -= (reduceLineCount * (cellHeight + spaceHeight));
                            scrollContentRectTransform.sizeDelta = sd;

                            gridLayoutGroup.padding.top -= (int)(reduceLineCount * (cellHeight + spaceHeight));

                            AdjustGridItemDataIndex(reduceLineCount * columnCount);
                        }
                        else
                        {
                            Debug.Log("�ղ���1�У����账��UI");
                        }
                    }
                    else
                    {
                        Debug.Log("���账��ĩ��UI");
                    }
                    RefreshAllGridItem();

                }
            } //ԭ����������Χ��

        } //����������

    }

    private GameObject FindLastActiveGridItem()
    {
        LinkedListNode<GameObject> tmp = cacheItems.Last;
        while (true)
        {
            if (tmp.Value.activeInHierarchy)
            {
                return tmp.Value;
            }
            else
            {
                tmp = tmp.Previous;
            }
            if (tmp == null) break;
        }
        return null;
    }

    private void AdjustGridItemDataIndex(int adjustOffset)
    {
        Debug.Log("AdjustGridItemDataIndex()");
        LinkedListNode<GameObject> tmp = cacheItems.First;
        while (true)
        {
            if (tmp == null) break;
            int dataIndex = int.Parse(tmp.Value.name);
            dataIndex -= adjustOffset;
            tmp.Value.name = dataIndex.ToString();
            tmp = tmp.Next;
        }
    }

    private void AdjustGridItemDataIndexToInitState()
    {
        Debug.Log("AdjustGridItemDataIndexToInitState()");
        LinkedListNode<GameObject> tmp = cacheItems.First;
        int i = -1;
        while (true)
        {
            if (tmp == null) break;
            i++;
            tmp.Value.name = i.ToString();
            tmp = tmp.Next;
        }
    }

    private void RefreshAllGridItem()
    {
        Debug.Log("RefreshAllGridItem()");

        LinkedListNode<GameObject> node = cacheItems.First;
        while (true)
        {
            GameObject cacheGridItem = node.Value;
            int itemDataIndex = int.Parse(cacheGridItem.name);
            if (itemDataIndex < this.dataSize)
            {
                //RoleItem roleItem = this.datas[itemDataIndex];
                SetGridItem(cacheGridItem, itemDataIndex);
            }
            else
            {
                cacheGridItem.SetActive(false);
            }
            node = node.Next;
            if (node == null) break;
        }

        if (dataSize > 0)
        {
            int lastI = lastClickGridItemIndex;
            if (lastI >= this.dataSize)
            {
                lastI = this.dataSize - 1;
            }
            lastClickGridItemIndex = lastI;
            //todo �ĵ�OnSelectGridItem�ص���ȥ
            //ShowItemDesc(this.datas[lastI]);
            SelectActiveItem(lastI);
        }
        else
        {
            lastClickGridItemIndex = 0;
            //ShowItemDesc(null);
            this.adapter.OnGridItemSelect(null, -1);
        }
    }

    private void OnGridItemClick(GameObject gridItem)
    {
        int clickIndex = int.Parse(gridItem.transform.name);
        Debug.Log("OnGridItemClick clickIndex " + clickIndex);
        lastClickGridItemIndex = clickIndex;

        this.adapter.OnGridItemClick(gridItem, clickIndex);
        SelectActiveItem(clickIndex);

        //todo �ĵ�OnSelectGridItem�ص���ȥ
        //ShowItemDesc(this.datas[clickIndex]);
    }

    private void SelectActiveItem(int targetIndex)
    {

        if (this.dataSize == 0) return;

        int _targetIndex = targetIndex;

        int firstActiveGridItemDataIndex = int.Parse(cacheItems.First.Value.name);
        GameObject lastActiveGridItem = FindLastActiveGridItem();
        int lastActiveGridItemDataIndex = int.Parse(lastActiveGridItem.name);

        //int _targetIndex = targetIndex;
        if (_targetIndex > lastActiveGridItemDataIndex || _targetIndex < firstActiveGridItemDataIndex)
        {
            return;
        }

        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            int dataIndex = int.Parse(node.Value.name);
            if (_targetIndex == dataIndex)
            {
                if (this.adapter.IsNeedAutoSelectState()) node.Value.GetComponent<Image>().color = Color.green;
                this.adapter.OnGridItemSelect(node.Value, dataIndex);
            }
            else
            {
                if (this.adapter.IsNeedAutoSelectState()) node.Value.GetComponent<Image>().color = Color.white;
            }
        }
        while ((node = node.Next) != null);
    }

    private void SetGridItem(GameObject cacheItem, int index)
    {
        //cacheItem.GetComponentInChildren<Text>().text = roleItem.itemId + "_" + roleItem.itemName + "_" + roleItem.itemCount; //todo ������id
        //cacheItem.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);

        this.adapter.BindView(cacheItem, index);

        cacheItem.name = "" + index;
        cacheItem.transform.name = index.ToString();
        cacheItem.SetActive(true);
        Button bt = cacheItem.GetComponent<Button>();
        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() =>
        {
            OnGridItemClick(cacheItem);
        });

        if (this.adapter.IsNeedAutoSelectState())
        {
            if (index == lastClickGridItemIndex)
            {
                cacheItem.GetComponent<Image>().color = Color.green;
            }
            else
            {
                cacheItem.GetComponent<Image>().color = Color.white;
            }
        }

    }

    private void InitItemCache()
    {

        Transform gridItemParent = scrollRectGameObject.GetComponent<ScrollRect>().content;
        //if (dataSize > (oneScreenNeedItems + columnCount * 2)) //����������1��+2��
        //{

        for (int i = 0; i < (oneScreenNeedItems + columnCount * 2); i++)
        {
            cacheGridItemLastDataIndex++;
            //GameObject cacheItem = GameObject.Instantiate(gridItemUIPrefab, gridItemParent);
            GameObject cacheItem = adapter.GetGridItemView(cacheGridItemLastDataIndex, gridItemParent);
            cacheItems.AddLast(cacheItem);
            if (cacheGridItemLastDataIndex < this.dataSize)
            {
                //RoleItem roleItem = this.datas[cacheGridItemLastDataIndex];
                SetGridItem(cacheItem, cacheGridItemLastDataIndex);
            }
            else
            {
                cacheItem.name = "" + cacheGridItemLastDataIndex;
                cacheItem.transform.name = cacheGridItemLastDataIndex.ToString();
                cacheItem.SetActive(false);
            }
        }
        SetInitHeight();
    }

    private void SetInitHeight()
    {
        if (dataSize > (oneScreenNeedItems + columnCount * 2))
        {
            int needHeight = ((int)((oneScreenNeedRow + 2) * (cellHeight + spaceHeight))) + gridLayoutGroup.padding.bottom; //ռ��һ��+2�еĸ߶�
            Debug.Log("����ѭ�� init all grid item height " + needHeight);
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = needHeight;
            scrollContentRectTransform.sizeDelta = sd;
        }
        else
        {
            Debug.Log("�������ݸ߶�");
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = (dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1) * (cellHeight + spaceHeight) + gridLayoutGroup.padding.bottom;
            scrollContentRectTransform.sizeDelta = sd;
        }
    }

    /// <summary>
    /// ������Update��ִ��
    /// </summary>
    public void Update()
    {
        scrollOffset = scrollContentRectTransform.anchoredPosition.y;
        if (scrollOffset > maxScrollOffset)
        {
            scrollOffset = maxScrollOffset;
        }
        else if (scrollOffset < 0f)
        {
            scrollOffset = 0f;
        }
        if (scrollOffset - preScrollOffset > 1 && scrollOffset > 1 && Input.GetMouseButton(0)) //���ϻ��� && ��ֱƫ����Ҫ����0(1���Ӱ�ȫһ��)���⻬������ص����µײ��Զ�������
        {
            //�ϻ�
            this.OnScrollDragUp();
        }
        else if (preScrollOffset - scrollOffset > 1 && Input.GetMouseButton(0)) //Input.GetMouseButton(0)��ֹscroll rect�������Զ��ص������߼�
        {
            //�»�
            this.OnScrollDragDown();
        }
        preScrollOffset = scrollOffset;
    }

    private void OnScrollDragUp()
    {
        if (this.dataSize > oneScreenNeedItems + 2 * columnCount) //��������������ظ���
        {
            //Debug.Log("��������������ظ���");
            if (scrollOffset + (cellHeight + spaceHeight) >= (scrollContentRectTransform.sizeDelta.y - containerHeight))
            {
                //Debug.Log("��ײ� 1�� ���Ӹ߶ȣ����Լ��ظ���(���һ�иո�¶����)");
                if (cacheGridItemLastDataIndex < dataSize - 1)
                {
                    Debug.LogWarning("cacheGridItemLastDataIndex < dataSize-1 ˵������������Ҫ��������ʾ,��ʽ��ʼ�ײ����ظ���");
                    for (int i = 0; i < columnCount; i++)
                    {
                        cacheGridItemLastDataIndex++;
                        GameObject firstGO = cacheItems.First.Value;
                        firstGO.transform.SetAsLastSibling();
                        cacheItems.RemoveFirst();
                        cacheItems.AddLast(firstGO);
                        if (cacheGridItemLastDataIndex >= dataSize) //ĳ���У�һ���ֳ�������
                        {
                            firstGO.name = cacheGridItemLastDataIndex.ToString();
                            firstGO.SetActive(false);
                        }
                        else
                        {
                            //RoleItem roleItem = this.datas[cacheGridItemLastDataIndex];
                            SetGridItem(firstGO, cacheGridItemLastDataIndex);
                        }
                    }
                    Debug.Log("���ӹ�������߶ȣ�������paddingTop�߶�");
                    Vector2 sd = scrollContentRectTransform.sizeDelta;
                    sd.y += (cellHeight + spaceHeight);
                    scrollContentRectTransform.sizeDelta = sd;
                    gridLayoutGroup.padding.top += (int)(cellHeight + spaceHeight);
                }
                else
                {
                    Debug.Log("�����Ѿ�ȫ��������ȫ");
                }
            }
            else
            {
                //Debug.Log("��ײ�����" + (scrollContentRectTransform.sizeDelta.y - scrollOffset - containerHeight));
            }
        }
        else
        {
            //Debug.Log("����������������ظ���");
        }

        if (scrollOffset >= maxHeight - containerHeight)
        {
            Debug.Log("���������ĵײ�");
        }
    }

    private void OnScrollDragDown()
    {
        if (scrollOffset <= gridLayoutGroup.padding.top + cellHeight)
        {
            //Debug.Log("����һ��(cellHeight-spaceHeight)�ľ��뵽��С������Ҳ�����������и�¶�������ͽ���ִ����");
            if (gridLayoutGroup.padding.top > originPaddingTop)
            {
                //Debug.Log("padding top �߶Ȼ����Լ���");
                if (int.Parse(scrollContentGameObj.transform.GetChild(0).name) > 0) //�׸�gridItem data index > 0
                {
                    Debug.Log("�׸�gridItem data index > 0���������Լ������أ���ʽ��ʼ���ض���");
                    for (int i = 0; i < columnCount; i++)
                    {
                        cacheGridItemLastDataIndex--;
                        GameObject lastGO = cacheItems.Last.Value;
                        lastGO.transform.SetAsFirstSibling();
                        cacheItems.RemoveLast();
                        cacheItems.AddFirst(lastGO);
                        int firstIndex = cacheGridItemLastDataIndex - ((oneScreenNeedRow + 2) * columnCount) + 1;
                        //Debug.Log("firstIndex " + firstIndex);
                        //RoleItem roleItem = this.datas[firstIndex];
                        SetGridItem(lastGO, firstIndex);
                    }
                    Vector2 sd = scrollContentRectTransform.sizeDelta;
                    sd.y -= (cellHeight + spaceHeight);
                    scrollContentRectTransform.sizeDelta = sd;
                    gridLayoutGroup.padding.top -= (int)(cellHeight + spaceHeight);
                }
            }
        }

        if (scrollOffset <= 0f)
        {
            Debug.Log("���������Ķ���");
        }
    }

}

public abstract class BaseAdapter
{
    /// <summary>
    /// ��ʼ����ǰ������UI
    /// </summary>
    /// <param name="index"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public abstract GameObject GetGridItemView(int index, Transform parent);
    /// <summary>
    /// ��ǰ���������ݺ�UI ��
    /// </summary>
    /// <param name="gridItemView"></param>
    /// <param name="index"></param>
    public abstract void BindView(GameObject gridItemView, int index);
    public abstract void OnGridItemClick(GameObject gridItemView, int index);
    /// <summary>
    /// ����˻���ѡ�У�����ѡ��δ���ǵ���ˣ��п�����֮ǰѡ�е�ɾ���ˣ��ͻ�����ѡ��
    /// </summary>
    /// <param name="gridItemView"></param>
    /// <param name="index"></param>
    public abstract void OnGridItemSelect(GameObject gridItemView, int index);
    /// <summary>
    /// �Ƿ���ҪĬ�ϵ�ѡ��״̬��ѡ�б�����ɫ���ģ�
    /// </summary>
    /// <returns>�Ƿ���ҪĬ�ϵ�ѡ��״̬��ѡ�б�����ɫ���ģ�</returns>
    public abstract bool IsNeedAutoSelectState();
    public abstract int GetDataCount();
}

public abstract class GridLayoutAdapter<T> : BaseAdapter
{
    protected List<T> datas;
    private GridLayoutAdapter() { }
    public GridLayoutAdapter(List<T> datas)
    {
        this.datas = datas;
    }
    public override int GetDataCount()
    {
        return this.datas.Count;
    }
}

public static class ComponentExtensions
{
    public static RectTransform rectTransform(this Component cp)
    {
        return cp.transform as RectTransform;
    }
}