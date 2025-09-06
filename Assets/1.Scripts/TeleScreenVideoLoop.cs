using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TeleScreenVideoLoop : MonoBehaviour
{
	[Header("UI / Video")]
	[SerializeField] private RawImage screen;
	[SerializeField] private VideoPlayer vp;

	[SerializeField] private string fileNameOrUrl = "";


	private Coroutine co;

	private void OnEnable()
	{
		PlayLoop();
	}

	public void PlayLoop()
	{
		if (co != null) StopCoroutine(co);
		co = StartCoroutine(CoPlayLoop());
	}

	private IEnumerator CoPlayLoop()
	{
		if (!vp) yield break;

		string url = System.IO.Path.Combine(Application.streamingAssetsPath, "Telescreen.mp4");

		vp.url = url;

		vp.isLooping = true;

		if (screen) screen.enabled = false;

		vp.Prepare();
		while (!vp.isPrepared) yield return null;

		if (screen && vp.targetTexture && screen.texture != vp.targetTexture)
			screen.texture = vp.targetTexture;

		if (screen) screen.enabled = true;

		vp.Play();
		co = null;
	}
}
