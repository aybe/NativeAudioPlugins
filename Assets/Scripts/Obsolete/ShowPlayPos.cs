using UnityEngine;

namespace Obsolete
{
    [RequireComponent(typeof(AudioSource))]
    public class ShowPlayPos : MonoBehaviour
    {
        void Start()
        {
        }

        void OnGUI()
        {
            GUILayout.Label(GetComponent<AudioSource>().time.ToString());
        }
    }
}
