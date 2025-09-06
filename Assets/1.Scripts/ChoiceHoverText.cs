using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class ChoiceHoverText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField] private TMP_Text label;
	[SerializeField] private Color normalColor = Color.black;
	[SerializeField] private Color hoverColor = new(0.33f, 0.33f, 0.33f, 1f); // #555555
	[SerializeField] private bool underlineOnHover = false;

	public event Action Clicked;

	void Reset()
	{
		if (!label) label = GetComponent<TMP_Text>();
	}

	void OnEnable()
	{
		if (label)
		{
			label.color = normalColor;
			SetUnderline(false);
		}
	}

	public void SetText(string text)
	{
		if (label) label.text = text;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!label) return;
		label.color = hoverColor;
		SetUnderline(underlineOnHover);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!label) return;
		label.color = normalColor;
		SetUnderline(false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Clicked?.Invoke();
	}

	private void SetUnderline(bool on)
	{
		if (!label) return;
		if (on) label.fontStyle |= FontStyles.Underline;
		else label.fontStyle &= ~FontStyles.Underline;
	}
}
