using System;
using UnityEngine;

public class FullEyeController : MonoBehaviour
{
    [SerializeField] private Animator fullEyeAnimator;

    public event Action OnSeeAnimationEnd;
    public event Action OnBlinkAnimationEnd;

    private void Awake()
    {
        fullEyeAnimator = GetComponent<Animator>();
    }

    public void FullEyeSee()
    {
        fullEyeAnimator.SetTrigger("See");
    }


    public void FullEyeBlink()
    {
        fullEyeAnimator.SetTrigger("Blink");
    }

    // Animation Event
    public void SeeAnimationEnd()
    {
        OnSeeAnimationEnd?.Invoke();
    }

    public void BlinkAnimationEnd()
    {
        OnBlinkAnimationEnd?.Invoke();
    }
}
