using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LetterAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text sourceText; // "�� �� �� ��" (������ �ؽ�Ʈ)
    [SerializeField] private TMP_Text targetText; // "�����Ѵ�" (��ǥ ��ġ ���� �ؽ�Ʈ)
    [SerializeField] private float duration = 2.0f; // �ִϸ��̼� �ð�
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1); // ������ ���� Ŀ��

    // �� ������ �ִϸ��̼� ������ ��� ����ü
    private struct CharAnimationInfo
    {
        public int sourceCharIndex;
        public int targetCharIndex;
        public Vector3[] initialPositions;
        public Vector3[] finalPositions;
    }

    void Start()
    {
        // �����̽��ٸ� ������ �ִϸ��̼� ���� (�׽�Ʈ��)
        Debug.Log("Press Spacebar to start the animation.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(AnimateLetters());
        }
    }

    private IEnumerator AnimateLetters()
    {
        // 1. �ؽ�Ʈ �޽� ������ ������ ������Ʈ�Ͽ� �ֽ� ���·� ����ϴ�.
        sourceText.ForceMeshUpdate();
        targetText.ForceMeshUpdate();

        // 2. ���� ����: � ���ڰ� ���� ������ �� �����մϴ�.
        List<CharAnimationInfo> animationInfos = MapCharacters();

        // 3. �ִϸ��̼� ����
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float easedProgress = curve.Evaluate(progress); // Ŀ�긦 �����Ͽ� �� �ε巯�� ������ ����

            UpdateVertices(animationInfos, easedProgress);

            yield return null;
        }

        // 4. �ִϸ��̼� ���� �� ��ġ ����
        UpdateVertices(animationInfos, 1f);

        Debug.Log("Animation Complete!");
    }

    private List<CharAnimationInfo> MapCharacters()
    {
        TMP_TextInfo sourceInfo = sourceText.textInfo;
        TMP_TextInfo targetInfo = targetText.textInfo;

        var animationInfos = new List<CharAnimationInfo>();
        var usedSourceIndices = new HashSet<int>();

        for (int i = 0; i < targetInfo.characterCount; i++)
        {
            if (char.IsWhiteSpace(targetInfo.characterInfo[i].character)) continue;

            char targetChar = targetInfo.characterInfo[i].character;
            int targetVertexIndex = targetInfo.characterInfo[i].vertexIndex;

            for (int j = 0; j < sourceInfo.characterCount; j++)
            {
                if (usedSourceIndices.Contains(j) || char.IsWhiteSpace(sourceInfo.characterInfo[j].character)) continue;

                if (sourceInfo.characterInfo[j].character == targetChar)
                {
                    int sourceVertexIndex = sourceInfo.characterInfo[j].vertexIndex;

                    // ��ǥ ��ġ�� ����ϴ� �κ� ����
                    Vector3[] finalPositions = new Vector3[4];
                    Vector3[] targetLocalPositions = GetVertexPositions(targetInfo, targetVertexIndex);

                    for (int k = 0; k < 4; k++)
                    {
                        // 1. Target�� ���� ���� ��ġ�� ���� ��ġ�� ��ȯ
                        Vector3 worldPos = targetText.transform.TransformPoint(targetLocalPositions[k]);

                        // 2. ���� ��ġ�� �ٽ� Source�� ���� ��ġ�� ��ȯ
                        finalPositions[k] = sourceText.transform.InverseTransformPoint(worldPos);
                    }

                    animationInfos.Add(new CharAnimationInfo
                    {
                        sourceCharIndex = j,
                        targetCharIndex = i,
                        initialPositions = GetVertexPositions(sourceInfo, sourceVertexIndex),
                        finalPositions = finalPositions // ��ȯ�� ���� ��ġ�� ���
                    });

                    usedSourceIndices.Add(j);
                    break;
                }
            }
        }
        return animationInfos;
    }

    // Ư�� ������ 4�� ���� ��ġ�� �迭�� ��ȯ�ϴ� �Լ�
    private Vector3[] GetVertexPositions(TMP_TextInfo textInfo, int vertexIndex)
    {
        Vector3[] positions = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            positions[i] = textInfo.meshInfo[0].vertices[vertexIndex + i];
        }
        return positions;
    }

    private void UpdateVertices(List<CharAnimationInfo> animationInfos, float progress)
    {
        Vector3[] vertices = sourceText.textInfo.meshInfo[0].vertices;

        foreach (var info in animationInfos)
        {
            int sourceVertexIndex = sourceText.textInfo.characterInfo[info.sourceCharIndex].vertexIndex;

            // ���� ��ġ�� ��ǥ ��ġ ���̸� ����(Lerp)�Ͽ� ���� ��ġ ���
            for (int i = 0; i < 4; i++)
            {
                vertices[sourceVertexIndex + i] = Vector3.Lerp(info.initialPositions[i], info.finalPositions[i], progress);
            }
        }

        sourceText.textInfo.meshInfo[0].vertices = vertices;
        sourceText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); // ���� ������ ������Ʈ ��û
    }
}