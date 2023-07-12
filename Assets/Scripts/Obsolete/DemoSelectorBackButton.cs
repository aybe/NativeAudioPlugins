using UnityEngine;
using UnityEngine.SceneManagement;

namespace Obsolete
{
    public class DemoSelectorBackButton : MonoBehaviour
    {
        void OnGUI()
        {
            Rect r = new Rect(0, Screen.height - 20, 100, 20);
            if (GUI.Button(r, "BACK"))
                SceneManager.LoadScene("demos");
        }
    }
}
