using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ���� �������� ��ũ���� �ϸ� ������ ���۰� ���� �̾����� �Ѵ�
public class InfiniteScroller : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentTransform;
    public HorizontalLayoutGroup layoutGroup;

    [SerializeField] private RectTransform[] textList; 

    [field : SerializeField] public bool IsDragging { get; private set; }

    private bool isCoroutineRunning = false;
    private float itemWidth;

    private float startPos;

    private void OnDestroy()
    {
      
        scrollRect.onValueChanged.RemoveListener(OnScrollChanged);
        DataManager.Instance.OnSlidingUnlocked -= EnableScrolling;
    }

    private void OnScrollChanged(Vector2 normalizedPos)
    {
        float pos = normalizedPos.x;
        if (pos < startPos - 0.01f)
        {
            DataManager.Instance.DoLeftSliding = true;
        }
        else if (pos > startPos + 0.01f)
        {
            DataManager.Instance.DoRightSliding = true;
        }
    }

    public IEnumerator Start()
    {

        yield return new WaitForEndOfFrame();

        if(DataManager.Instance.IsSlidingLocked)
        {
            DisableScrolling();
            DataManager.Instance.OnSlidingUnlocked += EnableScrolling;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);

        // ������ �ϳ��� �ʺ� ���
        itemWidth = textList[0].rect.width + layoutGroup.spacing;
        float scrollableWidth = contentTransform.rect.width - viewPortTransform.rect.width;
        float targetPosition = itemWidth;
        float targetNormalizedPosition = targetPosition / scrollableWidth;

        if (scrollableWidth > 0)
        {
            scrollRect.horizontalNormalizedPosition = targetNormalizedPosition;
        }

        startPos = scrollRect.horizontalNormalizedPosition;
        scrollRect.onValueChanged.AddListener(OnScrollChanged);
    }

    void Update()
    {
        if (textList == null || textList.Length == 0) return;
        if (isCoroutineRunning) return;

        float contentXPos = contentTransform.anchoredPosition.x;

        if (contentXPos > 0)
        {
            StartCoroutine(RepositionContent(-itemWidth));
        }
        else if (contentXPos < -itemWidth * 2)
        {
            StartCoroutine(RepositionContent(itemWidth));
        }
    }

    private IEnumerator RepositionContent(float positionOffset)
    {
        isCoroutineRunning = true;

     
        Vector2 savedVelocity = scrollRect.velocity;
        contentTransform.anchoredPosition += new Vector2(positionOffset, 0);

       
        yield return new WaitForEndOfFrame();

        
        scrollRect.velocity = savedVelocity;

        isCoroutineRunning = false;
    }

    public void DisableScrolling()
    {
        scrollRect.horizontal = false;
    }

    public void EnableScrolling()
    {
        scrollRect.horizontal = true;
    }
}