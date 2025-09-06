using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 수평 방향으로 스크롤을 하며 문장의 시작과 끝이 이어지게 한다
public class InfiniteScroller : MonoBehaviour
{
    public ScrollRect scrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentTransform;
    public HorizontalLayoutGroup layoutGroup;

    [SerializeField] private RectTransform[] textList; // 3개의 텍스트 요소로 설정 -> 현재 구조상 항상 Text는 viewPort기본 크기를 넘고있다

    [field : SerializeField] public bool IsDragging { get; private set; }

    private bool isCoroutineRunning = false;
    private float itemWidth;

    private float startPos;

    private void OnDestroy()
    {
        // 메모리 누수 방지
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

        // 아이템 하나의 너비 계산
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

        // 관성 저장
        Vector2 savedVelocity = scrollRect.velocity;
        contentTransform.anchoredPosition += new Vector2(positionOffset, 0);

        // UI가 위치 변경을 완전히 반영할 때 대기
        yield return new WaitForEndOfFrame();

        // 초기화된 위치에서 관성 유지
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