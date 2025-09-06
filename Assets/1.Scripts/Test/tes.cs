using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LetterAnimator : MonoBehaviour
{
    [SerializeField] private TMP_Text sourceText; // "다 한 좋 다" (움직일 텍스트)
    [SerializeField] private TMP_Text targetText; // "좋아한다" (목표 위치 계산용 텍스트)
    [SerializeField] private float duration = 2.0f; // 애니메이션 시간
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 움직임 제어 커브

    // 각 글자의 애니메이션 정보를 담는 구조체
    private struct CharAnimationInfo
    {
        public int sourceCharIndex;
        public int targetCharIndex;
        public Vector3[] initialPositions;
        public Vector3[] finalPositions;
    }

    void Start()
    {
        // 스페이스바를 누르면 애니메이션 시작 (테스트용)
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
        // 1. 텍스트 메쉬 정보를 강제로 업데이트하여 최신 상태로 만듭니다.
        sourceText.ForceMeshUpdate();
        targetText.ForceMeshUpdate();

        // 2. 글자 매핑: 어떤 글자가 어디로 가야할 지 결정합니다.
        List<CharAnimationInfo> animationInfos = MapCharacters();

        // 3. 애니메이션 실행
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float easedProgress = curve.Evaluate(progress); // 커브를 적용하여 더 부드러운 움직임 생성

            UpdateVertices(animationInfos, easedProgress);

            yield return null;
        }

        // 4. 애니메이션 종료 후 위치 보정
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

                    // 목표 위치를 계산하는 부분 수정
                    Vector3[] finalPositions = new Vector3[4];
                    Vector3[] targetLocalPositions = GetVertexPositions(targetInfo, targetVertexIndex);

                    for (int k = 0; k < 4; k++)
                    {
                        // 1. Target의 로컬 정점 위치를 월드 위치로 변환
                        Vector3 worldPos = targetText.transform.TransformPoint(targetLocalPositions[k]);

                        // 2. 월드 위치를 다시 Source의 로컬 위치로 변환
                        finalPositions[k] = sourceText.transform.InverseTransformPoint(worldPos);
                    }

                    animationInfos.Add(new CharAnimationInfo
                    {
                        sourceCharIndex = j,
                        targetCharIndex = i,
                        initialPositions = GetVertexPositions(sourceInfo, sourceVertexIndex),
                        finalPositions = finalPositions // 변환된 최종 위치를 사용
                    });

                    usedSourceIndices.Add(j);
                    break;
                }
            }
        }
        return animationInfos;
    }

    // 특정 글자의 4개 정점 위치를 배열로 반환하는 함수
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

            // 시작 위치와 목표 위치 사이를 보간(Lerp)하여 현재 위치 계산
            for (int i = 0; i < 4; i++)
            {
                vertices[sourceVertexIndex + i] = Vector3.Lerp(info.initialPositions[i], info.finalPositions[i], progress);
            }
        }

        sourceText.textInfo.meshInfo[0].vertices = vertices;
        sourceText.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices); // 정점 데이터 업데이트 요청
    }
}