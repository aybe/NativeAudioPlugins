using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class GranulatorController : MonoBehaviour
{
    public AudioMixer mixer;
    public Material mat;
    public float TimeScale = 1.0f;

    private float t0;

    void Start()
    {
        t0 = Time.time;
    }

    void Update()
    {
        Time.timeScale = TimeScale;
        float t = Time.time - t0, val;
        mixer.SetFloat("MainVolume", Mathf.Min(t * 3.0f - 80.0f, -20.0f));
        mixer.SetFloat("WindowLen", Mathf.Clamp(t * 0.001f, 0.005f, 0.1f));
        mixer.SetFloat("Offset", 0.2f - 0.2f * Mathf.Cos(t * 0.03f));
        mixer.SetFloat("RndOffset", 0.2f * (1.5f - Mathf.Sin(t * 0.05f)));
        mixer.SetFloat("RndSpeed", 0.1f - 0.1f * Mathf.Cos(t * 0.04f) * Mathf.Cos(t * 0.37f));
        mixer.GetFloat("Offset", out val); mat.SetFloat("Offset", val * val * 10.0f);
        mixer.GetFloat("RndOffset", out val); mat.SetFloat("RndOffset", val * val * 10.0f);
        mixer.GetFloat("RndSpeed", out val); mat.SetFloat("RndSpeed", val * val * 10.0f);
    }
}
