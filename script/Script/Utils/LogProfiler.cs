using UnityEngine;
using System;

namespace Consolation
{
    public class LogProfiler : MonoBehaviour
    {

        public int updateFrameRate = 4;

        private float _fps;

        private uint _usedHeapSize;

        private long _gcMemory;

        private void Update()
        {
            if (Time.frameCount % this.updateFrameRate == 0)
            {
                this._fps = 1f / Time.unscaledDeltaTime;
                this._usedHeapSize = UnityEngine.Profiling.Profiler.usedHeapSize / 1024u;
                this._gcMemory = GC.GetTotalMemory(false) / 1024L;
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.depth = 0;
            GUI.BeginGroup(new Rect(Screen.width - 250f, 0f, 250f, (float)Screen.height));
            GUILayout.Label("FPS:" + this._fps);
            GUILayout.Label("Used heap size: " + this._usedHeapSize + " KB");
            GUILayout.Label("Totle Memory(GC):" + this._gcMemory + " KB");
            if (GUILayout.Button("Close", GUILayout.Width(250)))
            {
                GameObject.Destroy(this);
            }
            GUI.EndGroup();
            GUI.color = Color.white;
        }
    }
}