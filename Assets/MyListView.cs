using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyListView : MonoBehaviour
{

    ScrollRect scrollRect;
    GridLayoutGroup gridLayoutGroup;
    GameObject scrollContentGameObj;
    RectTransform scrollContentRectTransform;

    public GameObject contentGameObj;
    public GameObject bagGridItemPrefab;

    void Start()
    {
        //GetComponent<RectTransform>().SetAsLastSibling();

        scrollRect = GetComponent<ScrollRect>();
        gridLayoutGroup = GetComponentInChildren<GridLayoutGroup>();

        scrollContentGameObj = gridLayoutGroup.gameObject;
        scrollContentRectTransform = scrollContentGameObj.GetComponent<RectTransform>();

        columnCount = gridLayoutGroup.constraintCount;
        cellHeight = gridLayoutGroup.cellSize.y;
        spaceHeight = gridLayoutGroup.spacing.y;
        originPaddingTop = gridLayoutGroup.padding.top;
        originPaddingBottom = gridLayoutGroup.padding.bottom;

        containerHeight = this.gameObject.transform.parent.rectTransform().rect.height;
        Debug.Log("containerHeight " + containerHeight);

        int totalRows = dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1;
        finalHeight = totalRows * (int)(cellHeight + spaceHeight) + 20;
        Debug.Log("finalHeight " + finalHeight);

        maxScrollOffset = finalHeight - containerHeight;
        Debug.Log("maxScrollOffset " + maxScrollOffset);

        initItemCache();
    }

    /// <summary>
    /// ��һ֡��ƫ����
    /// </summary>
    float preScrollOffset;

    float containerHeight = 0f;
    int finalHeight;

    /// <summary>
    /// �����ݿ��
    /// </summary>
    int dataSize = 111;
    int columnCount;
    float cellHeight;
    float spaceHeight;
    int originPaddingTop;
    int originPaddingBottom;
    LinkedList<GameObject> cacheItems = new LinkedList<GameObject>();

    int bottomDataPointer = 0;

    int oneScreenNeedRow;
    int oneScreenNeedItems;

    bool isLoadAll = false;

    public void OnGridItemClick(GameObject gridItem)
    {
        Debug.Log("OnGridItemClick " + gridItem.transform.GetChild(0).name);
    }

    private void initItemCache()
    {

        //ռ��1����Ҫ������
        oneScreenNeedRow = (int)(containerHeight / (cellHeight + spaceHeight)) + 1;
        Debug.Log("one screen needRow " + oneScreenNeedRow);
        //ռ��1����Ҫ���ܸ�������
        oneScreenNeedItems = oneScreenNeedRow * columnCount;
        Debug.Log("one screen needItems " + oneScreenNeedItems);
        if (dataSize > (oneScreenNeedItems + columnCount * 2)) //����������1��+2��
        {
            int height = ((int)((oneScreenNeedRow + 2) * (cellHeight + spaceHeight))) + (int)spaceHeight;
            Debug.Log("����ѭ�� init content height " + height);
            for (int i = 0; i < (oneScreenNeedItems + columnCount * 2); i++)
            {
                GameObject cacheItem = Instantiate(bagGridItemPrefab, contentGameObj.transform);
                cacheItem.name = "cacheItem " + i;
                cacheItem.transform.GetChild(0).name = i.ToString();
                cacheItems.AddLast(cacheItem);
                cacheItem.GetComponentInChildren<Text>().text = "������ƽ�x" + i;
                //cacheItem.GetComponentInChildren<Image>().sprite = ;
                cacheItem.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnGridItemClick(cacheItem);
                });
                bottomDataPointer++;
            }
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = height;
            scrollContentRectTransform.sizeDelta = sd;
            isLoadAll = false;
        }
        else //����ȫ������
        {
            for (int i = 0; i < dataSize; i++)
            {
                GameObject item = Instantiate(bagGridItemPrefab, contentGameObj.transform);
                //item.GetComponentInChildren<Image>().sprite = 
                item.GetComponentInChildren<Text>().text = "������ƽ�x" + i;
                item.transform.GetChild(0).name = i.ToString();
                item.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnGridItemClick(item);
                });
            }
            Vector2 sd = scrollContentRectTransform.sizeDelta;
            sd.y = (dataSize % columnCount == 0 ? dataSize / columnCount : dataSize / columnCount + 1) * (cellHeight + spaceHeight) + originPaddingBottom;
            scrollContentRectTransform.sizeDelta = sd;
            isLoadAll = true;
        }
        Debug.Log("cacheItems count " + cacheItems.Count);

    }

    /// <summary>
    /// ��ֱ����ƫ���� /  (cellHeight + spaceHeight)
    /// </summary>
    int lastRowIntervelNumber = 0;

    float scrollOffset;

    float maxScrollOffset;

    /// <summary>
    /// 0�ϻ����� 1�»���
    /// </summary>
    int mode = 0;

    bool c1 = false;

    private void Update()
    {
        if (isLoadAll) return;

        scrollOffset = scrollContentRectTransform.anchoredPosition.y;
        if(scrollOffset > maxScrollOffset)
        {
            scrollOffset = maxScrollOffset;
        }
        else if (scrollOffset < 0f)
        {
            scrollOffset = 0f;
        }

        //Debug.Log("bottomDataPointer " + bottomDataPointer);
        if (scrollOffset - preScrollOffset > 1 && scrollOffset > 1 && Input.GetMouseButton(0)) //���ϻ��� && ��ֱƫ����Ҫ����0(1���Ӱ�ȫһ��)���⻬������ص����µײ��Զ�������
        {
            c1 = false;
            if (mode == 0)
            {
                c1 = (int)(scrollOffset / (cellHeight + spaceHeight)) > lastRowIntervelNumber;
            }
            else if (mode == 1)
            {
                c1 = (int)((maxScrollOffset - scrollOffset) / (cellHeight + spaceHeight)) < lastRowIntervelNumber;
            }
            //��������1�еĸ߶�
            if (c1 && bottomDataPointer < dataSize)
            {
                //if (bottomDataPointer < dataSize) {
                Debug.Log("�ײ����ظ��� " + scrollContentRectTransform.anchoredPosition.y);
                for (int i = 0; i < columnCount; i++)
                {
                    GameObject firstGO = cacheItems.First.Value;
                    firstGO.transform.SetAsLastSibling();
                    cacheItems.RemoveFirst();
                    cacheItems.AddLast(firstGO);
                    firstGO.GetComponentInChildren<Text>().text = "������ƽ�x" + bottomDataPointer;
                    //firstGO.GetComponentInChildren<Image>().sprite = ;
                    firstGO.transform.GetChild(0).name = bottomDataPointer.ToString();
                    bottomDataPointer++;
                    if (bottomDataPointer > dataSize)
                    {
                        firstGO.SetActive(false);
                    }
                    else
                    {
                        firstGO.SetActive(true);
                    }
                }

                Vector2 sd = scrollContentRectTransform.sizeDelta;
                sd.y += (cellHeight + spaceHeight);
                scrollContentRectTransform.sizeDelta = sd;
                gridLayoutGroup.padding.top += (int)(cellHeight + spaceHeight);

                //if(gridLayoutGroup.padding.bottom > originPaddingBottom)
                //{
                //    gridLayoutGroup.padding.bottom -= (int)(cellHeight + spaceHeight);
                //}

            }

            if (scrollOffset >= finalHeight - containerHeight)
            {
                Debug.LogWarning("���������ĵײ�����Ҫ�ı�ģʽ");
                mode = 1;
            }
        }
        else if (preScrollOffset - scrollOffset > 1 && Input.GetMouseButton(0)) //Input.GetMouseButton(0)��ֹscroll rect�������Զ��ص������߼�
        {
            c1 = false;
            if(mode == 0)
            {
                c1 = (int)(scrollOffset / (cellHeight + spaceHeight)) < lastRowIntervelNumber;
            }
            else if (mode == 1)
            {
                c1 = (int)((maxScrollOffset - scrollOffset) / (cellHeight + spaceHeight)) > lastRowIntervelNumber;
            }
            if (c1 && gridLayoutGroup.padding.top > originPaddingTop)
            {
                Debug.Log("��������ԭ����item " + scrollContentRectTransform.anchoredPosition.y);
                for (int i = 0; i < columnCount; i++)
                {
                    GameObject lastGO = cacheItems.Last.Value;
                    lastGO.transform.SetAsFirstSibling();
                    cacheItems.RemoveLast();
                    cacheItems.AddFirst(lastGO);
                    lastGO.SetActive(true);
                    lastGO.GetComponentInChildren<Text>().text = "������ƽ�x" + (bottomDataPointer - ((oneScreenNeedRow + 2) * columnCount + 1));
                    //lastGO.GetComponentInChildren<Image>().sprite = ;
                    lastGO.transform.GetChild(0).name = (bottomDataPointer - ((oneScreenNeedRow + 2) * columnCount + 1)).ToString();

                    bottomDataPointer--;
                }
                Vector2 sd = scrollContentRectTransform.sizeDelta;
                sd.y -= (cellHeight + spaceHeight);
                scrollContentRectTransform.sizeDelta = sd;
                gridLayoutGroup.padding.top -= (int)(cellHeight + spaceHeight);
            }
            if (scrollOffset <= 0f)
            {
                Debug.LogWarning("���������Ķ�������Ҫ�ı�ģʽ");
                mode = 0;
            }
        }
        if(mode == 0)
        {
            lastRowIntervelNumber = (int)(scrollOffset / (cellHeight + spaceHeight)); //��Ҫ���������ı߽�ֵ
        }
        else if(mode == 1)
        {
            lastRowIntervelNumber = (int)((maxScrollOffset - scrollOffset) / (cellHeight + spaceHeight)); //��Ҫ���������ı߽�ֵ
        }
        preScrollOffset = scrollOffset;
    }

    public void OnCloseButtonClick()
    {
        this.gameObject.SetActive(false);
    }





}
