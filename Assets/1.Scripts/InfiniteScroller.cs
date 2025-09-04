using System.Collections;
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

    [SerializeField] private RectTransform[] textList; // 3���� �ؽ�Ʈ ��ҷ� ���� -> ���� ������ �׻� Text�� viewPort�⺻ ũ�⸦ �Ѱ��ִ�

    [field : SerializeField] public bool IsDragging { get; private set; }

    private bool isCoroutineRunning = false;
    private float itemWidth;

    public IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        // ������ �ϳ��� �ʺ� ���
        itemWidth = textList[0].rect.width + layoutGroup.spacing;

        float scrollableWidth = contentTransform.rect.width - viewPortTransform.rect.width;
        float targetPosition = itemWidth;
        float targetNormalizedPosition = targetPosition / scrollableWidth;

        if (scrollableWidth > 0)
        {
            scrollRect.horizontalNormalizedPosition = targetNormalizedPosition;
        }
    }

    void Update()
    {
        if (textList == null || textList.Length == 0) return;
        if (isCoroutineRunning) return;

        float contentXPos = contentTransform.anchoredPosition.x;

        // ������ ��踦 �Ѿ��� �� (�������� �̵��ؾ� ��)
        if (contentXPos > 0)
        {
            StartCoroutine(RepositionContent(-itemWidth));
        }
        // ���� ��踦 �Ѿ��� �� (���������� �̵��ؾ� ��)
        else if (contentXPos < -itemWidth)
        {
            StartCoroutine(RepositionContent(itemWidth));
        }
    }

    private IEnumerator RepositionContent(float positionOffset)
    {
        isCoroutineRunning = true;

        // ���� ����
        Vector2 savedVelocity = scrollRect.velocity;
        contentTransform.anchoredPosition += new Vector2(positionOffset, 0);

        // UI�� ��ġ ������ ������ �ݿ��� �� ���
        yield return new WaitForEndOfFrame();

        // �ʱ�ȭ�� ��ġ���� ���� ����
        scrollRect.velocity = savedVelocity;

        isCoroutineRunning = false;
    }
}