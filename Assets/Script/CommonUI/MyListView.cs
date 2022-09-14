using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MyListView : MonoBehaviour
{

    GridLayoutGroup gridLayoutGroup;
    RectTransform scrollContentRectTransform;

    //����UI prefab
    public GameObject bagGridItemPrefab;

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

    List<RoleItem> datas = new List<RoleItem>();

    /// <summary>
    /// �����ݿ��
    /// </summary>
    int dataSize = 111;

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

    //��ǰ���ص����һ�����ݵ�����
    int gridItemLastIndex = -1;

    //ռ��һ����Ҫ������
    int oneScreenNeedRow;

    //ռ��һ����Ҫ���ٸ�
    int oneScreenNeedItems;

    //�Ƿ�����ȫ��һ���Լ��أ�������Ҫ���޹���
    bool isLoadAll = false;

    //����GameObject
    GameObject scrollContentGameObj;

    //��ǰ�ѻ�����ƫ����
    float scrollOffset;

    //��������ƫ����
    float maxScrollOffset;

    void Start()
    {
        InitDatas();
        InitUIDatas();
        InitItemCache();
        SelectItem(0);
    }

    private void InitDatas()
    {
        MyDBManager.GetInstance().ConnDB();
        this.datas = MyDBManager.GetInstance().GetRoleItemInBag(1, false);

        //this.datas.RemoveRange(0, 13);
        //this.datas.Clear();

        this.dataSize = this.datas.Count;
    }

    private void InitUIDatas()
    {
        gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();

        scrollContentGameObj = gridLayoutGroup.gameObject;
        scrollContentRectTransform = scrollContentGameObj.GetComponent<RectTransform>();

        columnCount = gridLayoutGroup.constraintCount;
        cellHeight = gridLayoutGroup.cellSize.y;
        spaceHeight = gridLayoutGroup.spacing.y;
        if(originPaddingTop == 0) originPaddingTop = gridLayoutGroup.padding.top;

        containerHeight = transform.rectTransform().rect.height;
        Debug.Log("containerHeight " + containerHeight);

        float containerWidth = transform.rectTransform().rect.width;
        Vector2 v2 = gridLayoutGroup.spacing;
        v2.x = (containerWidth - (columnCount * gridLayoutGroup.cellSize.x)) / (columnCount + 1);
        gridLayoutGroup.spacing = v2;
        gridLayoutGroup.padding.left = (int)v2.x;
        gridLayoutGroup.padding.right = (int)v2.x;

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

    public void NotifyDatasetChange()
    {

        int originDataSize = this.dataSize;

        this.dataSize = this.datas.Count;
        //this.gridItemNewestNextPointer = 0;
        InitUIDatas();

        if (this.dataSize > originDataSize) //����������
        {

            //���һ��ʣ��Ŀ�λ
            int lastRowNonUseCount;
            LinkedListNode<GameObject> lastVisibleNode = cacheItems.Last;
            for (int i = 0; i < cacheItems.Count; i++)
            {
                if (lastVisibleNode != null && lastVisibleNode.Value.activeInHierarchy)
                {
                    break;
                }
                else
                {
                    lastVisibleNode = lastVisibleNode.Previous;
                }
            }
            if (lastVisibleNode != null && lastVisibleNode.Value.activeInHierarchy) //˵��������
            {
                Debug.Log("lastVisibleNode.Value.transform.GetChild(0).name " + lastVisibleNode.Value.transform.GetChild(0).name);
                if ((1 + int.Parse(lastVisibleNode.Value.transform.GetChild(0).name)) % columnCount == 0) //���һ��û�п�λ���պ�����
                {
                    lastRowNonUseCount = 0;
                }
                else
                {
                    lastRowNonUseCount = columnCount - ((1 + int.Parse(lastVisibleNode.Value.transform.GetChild(0).name)) % columnCount);
                }
                Debug.Log("lastRowNonUseCount " + lastRowNonUseCount);
                //�����˶�������
                int addDataCount = this.dataSize - originDataSize;
                Debug.Log("addDataCount " + addDataCount);

                RefreshAllGridItem();
                if (addDataCount > lastRowNonUseCount) //���ӵ�����������ʣ���λ
                {

                    if(originDataSize <= (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("�������ڷ�Χ��");
                        if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("��Χ���������ݣ�ֻ��Ҫ�б䶯");
                            int addLineCount;
                            int c = addDataCount - lastRowNonUseCount;
                            if (c % columnCount == 0)
                            {
                                addLineCount = c / columnCount;
                            }
                            else
                            {
                                addLineCount = c / columnCount + 1;
                            }
                            Debug.Log("addLineCount " + addLineCount);
                            Vector2 sd = scrollContentRectTransform.sizeDelta;
                            sd.y += (addLineCount * (cellHeight + spaceHeight));
                            scrollContentRectTransform.sizeDelta = sd;
                        }
                        else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                        {
                            Debug.Log("��Χ���������ݵ���Χ�⣬������Ҫ�б䶯 �� paddingTop�䶯");
                            SetInitHeight();
                            //if (scrollOffset >= maxHeight - containerHeight) //�����ǰ�������ײ��������+1��
                            //{
                            //    //+1��
                            //    ScrollTouchUp();
                            //}
                        }
                    }
                    else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
                    {
                        Debug.Log("���������ڷ�Χ��");
                    }

                }
                else
                {
                    Debug.Log("��ӵ�������С�ڵ������ظ��ӣ����账��");
                }
            }
            else //˵���������֮ǰ������
            {
                RefreshAllGridItem();
                SetInitHeight();
            }

            
        }
        else if (this.dataSize == originDataSize) //û����������
        {
            Debug.Log("������û��");
            RefreshAllGridItem();
        }
        else //����������
        {
            Debug.Log("originDataSize " + originDataSize);

            if (originDataSize <= (oneScreenNeedItems + 2 * columnCount)) //��Χ��
            {
                Debug.Log("��Χ�� ���� ��Χ��");
                RefreshAllGridItem();
                SetInitHeight();
            }
            else if (originDataSize > (oneScreenNeedItems + 2 * columnCount))
            {
                if (this.dataSize <= (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("��Χ�� ���� ��Χ��");
                    //RefreshAllGridItem();
                    //SetInitHeight();
                    //gridLayoutGroup.padding.top = originPaddingTop;
                }
                else if (this.dataSize > (oneScreenNeedItems + 2 * columnCount))
                {
                    Debug.Log("��Χ�� ���� ��Χ��");
                }
            }

            ////���һ��������
            //int lastRowVisibleGridItemCount = originDataSize % this.columnCount;
            //if(originDataSize % this.columnCount == 0)
            //{
            //    lastRowVisibleGridItemCount = columnCount;
            //}
            //RefreshAllGridItem();
            //int reduceDataCount = originDataSize - this.dataSize;
            //Debug.Log("reduceDataCount " + reduceDataCount + ", lastRowGridItemCount " + lastRowVisibleGridItemCount);
            //if (reduceDataCount >= lastRowVisibleGridItemCount) //��Ҫ����
            //{
            //    Debug.Log("��Ҫ����");
            //    int c = reduceDataCount - lastRowVisibleGridItemCount; //��Ҫ��1��
            //    int lineCountForDelete = 1;
            //    if(c < columnCount)
            //    {
            //        Debug.Log("����Ҫ+1");
            //    }
            //    else if(c == columnCount)
            //    {
            //        lineCountForDelete++;
            //    }
            //    else if (c > columnCount)
            //    {
            //        if(c % columnCount == 0)
            //        {
            //            lineCountForDelete += c / columnCount;
            //        }
            //        else
            //        {
            //            lineCountForDelete += (int)(c / columnCount + 1);
            //        }
            //    }

                //    Debug.Log("lineCountForDelete " + lineCountForDelete);



                //    Vector2 sd = scrollContentRectTransform.sizeDelta;
                //    sd.y -= (lineCountForDelete * (cellHeight + spaceHeight));

                //    if (this.dataSize > (oneScreenNeedItems + columnCount * 2)) //��������
                //    {
                //        int atLeastHeight = ((int)((oneScreenNeedRow + 2) * (cellHeight + spaceHeight))) + gridLayoutGroup.padding.bottom; //ռ��һ��+2�еĸ߶�
                //        if(sd.y < atLeastHeight)
                //        {
                //            Debug.LogWarning("���������-1");
                //        }
                //        else
                //        {
                //            Debug.LogWarning("1111");
                //            gridLayoutGroup.padding.top -= (lineCountForDelete * (int)(cellHeight + spaceHeight));

                //            scrollContentRectTransform.sizeDelta = sd;

                //            Vector2 os = scrollContentRectTransform.anchoredPosition;
                //            os.y -= (lineCountForDelete * (cellHeight + spaceHeight));
                //            scrollContentRectTransform.anchoredPosition = os;

                //        }
                //    }
                //    else
                //    {
                //        if(sd.y < originPaddingTop)
                //        {
                //            Debug.LogWarning("���������-2");
                //        }
                //        else
                //        {
                //            Debug.LogWarning("2222");
                //            gridLayoutGroup.padding.top -= (lineCountForDelete * (int)(cellHeight + spaceHeight));
                //            scrollContentRectTransform.sizeDelta = sd;
                //        }
                //    }



                //}
                //else
                //{
                //    Debug.Log("���ٵ������������һ���㹻�ۼ����������");
                //}
        }

    }

    private void RefreshAllGridItem()
    {
        Debug.Log("RefreshAllGridItem()");
        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject cacheGridItem = node.Value;
            int itemDataIndex = int.Parse(cacheGridItem.transform.GetChild(0).name);
            if (itemDataIndex < this.dataSize)
            {
                RoleItem roleItem = this.datas[itemDataIndex];
                SetGridItem(cacheGridItem, itemDataIndex, roleItem);
            }
            else
            {
                cacheGridItem.SetActive(false);
            }
        } while ((node = node.Next) != null);

        SelectItem(lastClickGridItemIndex);
    }

    public void OnGridItemClick(GameObject gridItem)
    {
        int clickIndex = int.Parse(gridItem.transform.GetChild(0).name);
        Debug.Log("OnGridItemClick clickIndex " + clickIndex);
        lastClickGridItemIndex = clickIndex;

        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject gridItemGO = node.Value;
            gridItemGO.GetComponent<Image>().color = Color.white;
        }
        while ((node = node.Next) != null);
        gridItem.GetComponent<Image>().color = Color.green;

        ShowItemDesc(this.datas[clickIndex]);
    }

    private void SelectItem(int targetIndex)
    {
        int _targetIndex = targetIndex;
        if (_targetIndex >= this.datas.Count)
        {
            _targetIndex = this.dataSize - 1;
        }
        LinkedListNode<GameObject> node = cacheItems.First;
        do
        {
            GameObject gridItemGO = node.Value;
            string dataIndex = gridItemGO.transform.GetChild(0).name;
            int intIndex = int.Parse(dataIndex);
            if (_targetIndex == intIndex)
            {
                OnGridItemClick(gridItemGO);
                break;
            }
        }
        while ((node = node.Next) != null);
    }

    private void SetGridItem(GameObject cacheItem, int index, RoleItem roleItem)
    {
        cacheItem.name = "cacheItem_" + index;
        cacheItem.transform.GetChild(0).name = index.ToString();
        cacheItem.SetActive(true);
        cacheItem.GetComponentInChildren<Text>().text = roleItem.itemName + "x" + roleItem.itemCount;
        //cacheItem.GetComponentInChildren<Text>().text = "x" + index;
        cacheItem.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
        Button bt = cacheItem.GetComponent<Button>();
        bt.onClick.RemoveAllListeners();
        bt.onClick.AddListener(() =>
        {
            OnGridItemClick(cacheItem);
        });
        if(index == lastClickGridItemIndex)
        {
            cacheItem.GetComponent<Image>().color = Color.green;
        }
        else
        {
            cacheItem.GetComponent<Image>().color = Color.white;
        }
    }

    private void InitItemCache()
    {

        Transform gridItemParent = GetComponent<ScrollRect>().content;
        //if (dataSize > (oneScreenNeedItems + columnCount * 2)) //����������1��+2��
        //{

        for (int i = 0; i < (oneScreenNeedItems + columnCount * 2); i++)
        {
            gridItemLastIndex++;
            GameObject cacheItem = Instantiate(bagGridItemPrefab, gridItemParent);
            cacheItems.AddLast(cacheItem);
            if(gridItemLastIndex < this.dataSize)
            {
                RoleItem roleItem = this.datas[gridItemLastIndex];
                SetGridItem(cacheItem, gridItemLastIndex, roleItem);
            }
            else
            {
                cacheItem.name = "cacheItem_" + gridItemLastIndex;
                cacheItem.transform.GetChild(0).name = gridItemLastIndex.ToString();
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

    private void Update()
    {
        scrollOffset = scrollContentRectTransform.anchoredPosition.y;
        if(scrollOffset > maxScrollOffset)
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
            this.ScrollTouchUp();
        }
        else if (preScrollOffset - scrollOffset > 1 && Input.GetMouseButton(0)) //Input.GetMouseButton(0)��ֹscroll rect�������Զ��ص������߼�
        {
            //�»�
            this.ScrollTouchDown();
        }
        preScrollOffset = scrollOffset;
    }

    private void ScrollTouchUp()
    {
        if (this.dataSize > oneScreenNeedItems + 2 * columnCount) //��������������ظ���
        {
            Debug.Log("��������������ظ���");
            if (scrollOffset + (cellHeight + spaceHeight) >= (scrollContentRectTransform.sizeDelta.y - containerHeight))
            {
                Debug.Log("��ײ� 1�� ���Ӹ߶ȣ����Լ��ظ���(���һ�иո�¶����)");
                if (gridItemLastIndex < dataSize - 1)
                {
                    Debug.Log("gridItemLastIndex < dataSize-1 ˵����������û�м��س�������ʽ��ʼ���ظ���");
                    for (int i = 0; i < columnCount; i++)
                    {
                        gridItemLastIndex++;
                        GameObject firstGO = cacheItems.First.Value;
                        firstGO.transform.SetAsLastSibling();
                        cacheItems.RemoveFirst();
                        cacheItems.AddLast(firstGO);
                        if (gridItemLastIndex >= dataSize) //ĳ���У�һ���ֳ�������
                        {
                            firstGO.name = "cacheItem_" + gridItemLastIndex;
                            firstGO.transform.GetChild(0).name = gridItemLastIndex.ToString();
                            firstGO.SetActive(false);
                        }
                        else
                        {
                            RoleItem roleItem = this.datas[gridItemLastIndex];
                            SetGridItem(firstGO, gridItemLastIndex, roleItem);
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
                    Debug.Log("�����Ѿ�ȫ����ʾ��ȫ");
                }
            }
            else
            {
                Debug.Log("��ײ�����" + (scrollContentRectTransform.sizeDelta.y - scrollOffset - containerHeight));
            }
        }
        else
        {
            Debug.Log("����������������ظ���");
        }

        if (scrollOffset >= maxHeight - containerHeight)
        {
            Debug.Log("���������ĵײ�");
        }
    }

    private void ScrollTouchDown()
    {
        if (scrollOffset <= gridLayoutGroup.padding.top + cellHeight)
        {
            Debug.Log("����һ��(cellHeight-spaceHeight)�ľ��뵽��С������Ҳ�����������и�¶�������ͽ���ִ����");
            if (gridLayoutGroup.padding.top > originPaddingTop)
            {
                Debug.Log("padding top �߶Ȼ����Լ���");
                if (int.Parse(scrollContentGameObj.transform.GetChild(0).GetChild(0).name) > 0) //�׸�gridItem data index > 0
                {
                    Debug.Log("�׸�gridItem data index > 0���������Լ������أ���ʽ��ʼ���ض���");
                    for (int i = 0; i < columnCount; i++)
                    {
                        gridItemLastIndex--;
                        GameObject lastGO = cacheItems.Last.Value;
                        lastGO.transform.SetAsFirstSibling();
                        cacheItems.RemoveLast();
                        cacheItems.AddFirst(lastGO);
                        int firstIndex = gridItemLastIndex - ((oneScreenNeedRow + 2) * columnCount) + 1;
                        //Debug.Log("firstIndex " + firstIndex);
                        RoleItem roleItem = this.datas[firstIndex];
                        SetGridItem(lastGO, firstIndex, roleItem);
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

    public void OnCloseButtonClick()
    {
        this.gameObject.SetActive(false);
    }







    public GameObject imageGO;
    public GameObject buttonGO;
    public GameObject nameGO;
    public GameObject countGO;
    public GameObject effectDescGO;
    public GameObject itemDescGO;

    private void ShowItemDesc(RoleItem roleItem)
    {
        if(roleItem != null)
        {
            imageGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/ItemImage/" + roleItem.imageName);
            Button useButton = buttonGO.GetComponent<Button>();
            buttonGO.SetActive(true);
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => { OnUseButtonClick(roleItem); });
            nameGO.GetComponent<Text>().text = roleItem.itemName;
            countGO.GetComponent<Text>().text = "������ " + roleItem.itemCount;
            effectDescGO.GetComponent<Text>().text = "��Ч��" + (roleItem.recoverHp > 0 ? " ��Ѫ+" + roleItem.recoverHp : "") + (roleItem.recoverMp > 0 ? " ����+" + roleItem.recoverMp : "");
            itemDescGO.GetComponent<Text>().text = roleItem.itemDesc;
        }
        else
        {
            imageGO.GetComponent<Image>().sprite = null;
            buttonGO.SetActive(false);
            nameGO.GetComponent<Text>().text = "";
            countGO.GetComponent<Text>().text = "";
            effectDescGO.GetComponent<Text>().text = "";
            itemDescGO.GetComponent<Text>().text = "";
        }
    }

    void OnUseButtonClick(RoleItem roleItem)
    {
        MyDBManager.GetInstance().ConnDB();
        MyDBManager.GetInstance().DeleteItemInBag(roleItem.itemId, 1, roleItem.itemCount);
        if (roleItem.itemCount == 1)
        {
            this.datas.Remove(roleItem);
            ShowItemDesc(null);
        }
        else
        {
            roleItem.itemCount--;
            ShowItemDesc(roleItem);
        }
        NotifyDatasetChange();

        //todo ���ԣ�����ѭ��ģʽ�£����м�ɾ��

        Debug.Log("OnUseButtonClick()");
    }




    public void AddDataTest()
    {
        //for(int i=0; i<10; i++)
        //{
        //    RoleItem a = new RoleItem();
        //    a.itemCount = 1;
        //    a.itemDesc = "desc";
        //    a.itemName = "name";
        //    a.recoverHp = 999;
        //    this.datas.Insert(0, a);
        //}
        
        //i++;
        //a = new RoleItem();
        //a.itemCount = 2;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 3;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 4;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);
        //i++;
        //a = new RoleItem();
        //a.itemCount = 5;
        //a.itemDesc = "desc";
        //a.itemName = "name" + i;
        //a.recoverHp = 999;
        //this.datas.Insert(i, a);

        if(this.datas.Count > 0) this.datas.RemoveAt(0);

        //if (this.datas.Count > 0) this.datas.RemoveAt(0);

        NotifyDatasetChange();
    }

}
