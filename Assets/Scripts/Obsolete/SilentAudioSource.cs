using UnityEngine;

namespace Obsolete
{
    [RequireComponent(typeof(AudioSource))]
    public class SilentAudioSource : MonoBehaviour
    {
        void OnAudioFilterRead(float[] data, int channels)
        {
        }
    }
}
