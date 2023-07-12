using System.Runtime.InteropServices;
using UnityEngine;

namespace Obsolete
{
    public class GranulatorDebug : MonoBehaviour
    {
        [DllImport("AudioPluginDemo")]
        private static extern int Granulator_DebugGetGrainCount();

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void OnGUI()
        {
            GUILayout.Label("Current grain count: " + Granulator_DebugGetGrainCount() + " grains");
        }
    }
}
