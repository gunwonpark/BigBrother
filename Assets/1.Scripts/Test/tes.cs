using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TextRearrangeEffect : MonoBehaviour
{
    private TMP_Text textMeshPro;

    // ���� �ܾ�� ��ǥ �ܾ�
    public string startWord = "SAEPEC";
    public string endWord = "ESCAPE";

    // �� ������ ���� ��ġ�� ��ǥ ��ġ
    private Vector3[] startPositions;
    private Vector3[] endPositions;

    // �ִϸ��̼� ���� �ð�
    public float duration = 2.0f;
    private float elapsedTime = 0f;

    void Start()
    {
        textMeshPro = GetComponent<TMP_Text>();
        if (textMeshPro == null)
        {
            Debug.LogError("TextMeshPro ������Ʈ�� ã�� �� �����ϴ�.");
            return;
        }

        StartCoroutine(PrepareAndPlayAnimation());
    }

    IEnumerator PrepareAndPlayAnimation()
    {
        // 1. �ʱ� �ؽ�Ʈ ���� �� ��ġ ���
        textMeshPro.text = startWord;
        // �ؽ�Ʈ ������Ʈ���� ������ ������Ʈ�Ͽ� ���� ������ ��� ������
        textMeshPro.ForceMeshUpdate();
        yield return null; // �� ������ ����Ͽ� ������Ʈ �Ϸ� ����

        // ���� ��ġ ����
        startPositions = new Vector3[startWord.Length];
        for (int i = 0; i < startWord.Length; i++)
        {
            // �� ������ �߽� ��ġ�� ����
            TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textMeshPro.mesh.vertices;
            startPositions[i] = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
        }


        // 2. ��ǥ �ؽ�Ʈ ���� �� ��ġ ���
        textMeshPro.text = endWord;
        textMeshPro.ForceMeshUpdate();
        yield return null; // �� ������ ���

        // ��ǥ ��ġ ����
        endPositions = new Vector3[endWord.Length];
        for (int i = 0; i < endWord.Length; i++)
        {
            TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[i];
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textMeshPro.mesh.vertices;
            endPositions[i] = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
        }


        // 3. �ִϸ��̼� ��� �غ�
        textMeshPro.text = startWord; // �ٽ� ���� �ܾ�� ǥ��
        textMeshPro.ForceMeshUpdate();
        yield return null;

        // �ִϸ��̼� ��� ����
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
        // �ε巯�� �������� ���� Ease-in-out ȿ��
        t = t * t * (3f - 2f * t);

        TMP_TextInfo textInfo = textMeshPro.textInfo;
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        // �� ������ ���� (S -> E, A -> S, E -> C, P -> A, E -> P, C -> E)
        int[] mapping = new int[] { 4, 0, 5, 1, 2, 3 }; // startWord �ε��� -> endWord �ε���

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int mappedIndex = mapping[i];

            // ���� ��ġ ��� (����)
            Vector3 currentPos = Vector3.Lerp(startPositions[i], endPositions[mappedIndex], t);

            // ���� ��ġ���� ���� ���
            Vector3 offset = currentPos - startPositions[i];

            // �� ���ڸ� �����ϴ� ����(vertex)���� �̵�
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            var verts = cachedMeshInfo[0].vertices;
            verts[vertexIndex + 0] += offset;
            verts[vertexIndex + 1] += offset;
            verts[vertexIndex + 2] += offset;
            verts[vertexIndex + 3] += offset;
        }

        // ������ ���� ������ �޽� ������Ʈ
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = cachedMeshInfo[i].vertices;
            textMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }

        // �ִϸ��̼��� ������ ���� �ؽ�Ʈ�� ����
        if (elapsedTime >= duration)
        {
            textMeshPro.text = endWord;
        }
    }
}