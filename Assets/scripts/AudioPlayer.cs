using System;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource AudioSource;

    [Min(1)] public int GUIScale = 4;


    private void OnGUI()
    {
        var matrix = GUI.matrix;

        GUI.matrix = Matrix4x4.Scale(new Vector3(GUIScale, GUIScale, GUIScale));

        if (GUILayout.Button("Play"))
        {
            AudioSource.Play();
        }

        if (GUILayout.Button("Pause"))
        {
            AudioSource.Pause();
        }

        if (GUILayout.Button("Stop"))
        {
            AudioSource.Stop();
        }

        var loop1 = AudioSource.loop;
        var loop2 = GUILayout.Toggle(loop1, "Loop");

        if (loop1 != loop2)
        {
            AudioSource.loop = loop2;
        }

        var time1 = AudioSource.timeSamples;
        var time2 = (int)GUILayout.HorizontalSlider(time1, 0, AudioSource.clip.samples - 1, GUILayout.Width(300.0f));

        if (time1 != time2)
        {
            AudioSource.timeSamples = time2;
        }

        GUILayout.Label(TimeSpan.FromSeconds(AudioSource.time).ToString(), Styles.Label);

        GUI.matrix = matrix;
    }

    private abstract class Styles
    {
        public static readonly GUIStyle Label = new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };
    }
}
