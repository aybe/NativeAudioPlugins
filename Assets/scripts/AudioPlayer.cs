using System;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable InconsistentNaming

public class AudioPlayer : MonoBehaviour
{
    public AudioSource Source;

    [Min(1)]
    public int Scale = 2;

    private void OnGUI()
    {
        var matrix = GUI.matrix;

        GUI.matrix = Matrix4x4.Scale(new Vector3(Scale, Scale, Scale));
        using (new GuiEnabledScope(!Source.isPlaying))
            if (GUILayout.Button("Play"))
            {
                Source.Play();
            }

        using (new GuiEnabledScope(Source.isPlaying))
            if (GUILayout.Button("Pause"))
            {
                Source.Pause();
            }

        using (new GuiEnabledScope(Source.isPlaying))
            if (GUILayout.Button("Stop"))
            {
                Source.Stop();
            }

        var loop1 = Source.loop;
        var loop2 = GUILayout.Toggle(loop1, "Loop");

        if (loop1 != loop2)
        {
            Source.loop = loop2;
        }

        var time1 = Source.timeSamples;
        var time2 = (int)GUILayout.HorizontalSlider(time1, 0, Source.clip.samples - 1, GUILayout.Width(300.0f));

        if (time1 != time2)
        {
            Source.timeSamples = time2;
        }

        GUILayout.Label(TimeSpan.FromSeconds(Source.time).ToString(), Styles.Label);

        GUI.matrix = matrix;
    }

    private readonly struct GuiEnabledScope : IDisposable
    {
        private readonly bool enabled;

        public GuiEnabledScope(bool enabled)
        {
            this.enabled = GUI.enabled;
            GUI.enabled = enabled;
        }

        public void Dispose()
        {
            GUI.enabled = enabled;
        }
    }

    private abstract class Styles
    {
        public static readonly GUIStyle Label = new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter
        };
    }
}
