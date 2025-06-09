using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource buzzer;
    public AudioSource fingerTap;
    public AudioSource basketballBounce;
    public AudioSource win;

    public void PlayBuzzer()
    {
        buzzer.Play();
    }

    public void PlayFingerTap()
    {
        fingerTap.Play();
    }

    public void PlayBasketballBounce()
    {
        basketballBounce.Play();
    }

    public void PlayWin()
    {
        win.Play();
    }
}
