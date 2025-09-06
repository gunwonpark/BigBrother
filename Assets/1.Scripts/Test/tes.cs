using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TextRearrangeEffect : MonoBehaviour
{
    private TMP_Text textMeshPro;

    // 시작 단어와 목표 단어
    public string startWord = "SAEPEC";
    public string endWord = "ESCAPE";

    // 각 글자의 시작 위치와 목표 위치
    private Vector3[] startPositions;
    private Vector3[] endPositions;

    // 애니메이션 지속 시간
    public float duration = 2.0f;
    private float elapsedTime = 0f;

    void Start()
    {
        textMeshPro = GetComponent<TMP_Text>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(PrepareAndPlayAnimation());
    }

    IEnumerator PrepareAndPlayAnimation()
    {
        // 1. 초기 텍스트 설정 및 위치 계산
        textMeshPro.text = startWord;
        // 텍스트 지오메트리를 강제로 업데이트하여 문자 정보를 즉시 가져옴
        textMeshPro.ForceMeshUpdate();
        yield return null; // 한 프레임 대기하여 업데이트 완료 보장

        // 시작 위치 저장
        startPositions = new Vector3[startWord.Length];
        for (int i = 0; i < startWord.Length; i++)
        {
            // 각 문자의 중심 위치를 저장
            TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textMeshPro.mesh.vertices;
            startPositions[i] = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
        }


        // 2. 목표 텍스트 설정 및 위치 계산
        textMeshPro.text = endWord;
        textMeshPro.ForceMeshUpdate();
        yield return null; // 한 프레임 대기

        // 목표 위치 저장
        endPositions = new Vector3[endWord.Length];
        for (int i = 0; i < endWord.Length; i++)
        {
            TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textMeshPro.mesh.vertices;
            endPositions[i] = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
        }


        // 3. 애니메이션 재생 준비
        textMeshPro.text = startWord; // 다시 시작 단어로 표시
        textMeshPro.ForceMeshUpdate();
        yield return null;

        // 애니메이션 재생 시작
        elapsedTime = 0f;
    }


    void Update()
    {
        if (startPositions == null || endPositions == null || elapsedTime >= duration)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);
        // 부드러운 움직임을 위한 Ease-in-out 효과
        t = t * t * (3f - 2f * t);

        TMP_TextInfo textInfo = textMeshPro.textInfo;
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        // 각 글자의 매핑 (S -> E, A -> S, E -> C, P -> A, E -> P, C -> E)
        int[] mapping = new int[] { 4, 0, 5, 1, 2, 3 }; // startWord 인덱스 -> endWord 인덱스

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int mappedIndex = mapping[i];

            // 현재 위치 계산 (보간)
            Vector3 currentPos = Vector3.Lerp(startPositions[i], endPositions[mappedIndex], t);

            // 원래 위치와의 차이 계산
            Vector3 offset = currentPos - startPositions[i];

            // 각 글자를 구성하는 정점(vertex)들을 이동
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            var verts = cachedMeshInfo[0].vertices;
            verts[vertexIndex + 0] += offset;
            verts[vertexIndex + 1] += offset;
            verts[vertexIndex + 2] += offset;
            verts[vertexIndex + 3] += offset;
        }

        // 수정된 정점 정보로 메쉬 업데이트
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = cachedMeshInfo[i].vertices;
            textMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }

        // 애니메이션이 끝나면 최종 텍스트로 설정
        if (elapsedTime >= duration)
        {
            textMeshPro.text = endWord;
        }
    }
}