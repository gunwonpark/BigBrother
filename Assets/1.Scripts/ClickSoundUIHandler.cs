using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSoundUIHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private string clickDownKey = "click_down";
	[SerializeField] private string clickUpKey = "click_up";

	[SerializeField] private bool playUpOnlyIfReleasedInside = true;

	private bool pressedInside = false;
	private bool pointerOver = false;

	public void OnPointerEnter(PointerEventData eventData) => pointerOver = true;
	public void OnPointerExit(PointerEventData eventData) => pointerOver = false;

	public void OnPointerDown(PointerEventData eventData)
	{
		pressedInside = true;
		if (!string.IsNullOrEmpty(clickDownKey))
			SoundManager.Instance.Play(clickDownKey, Sound.Effect);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		bool canPlayUp = !playUpOnlyIfReleasedInside || (pressedInside && pointerOver);
		if (canPlayUp && !string.IsNullOrEmpty(clickUpKey))
			SoundManager.Instance.Play(clickUpKey, Sound.Effect);

		pressedInside = false;
	}

	private void OnDisable()
	{
		pressedInside = false;
		pointerOver = false;
	}
}
