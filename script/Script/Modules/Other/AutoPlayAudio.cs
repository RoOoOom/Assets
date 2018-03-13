using UnityEngine;
public class AutoPlayAudio : MonoBehaviour
{
    public string pkg = "";
    public string resName = "";
    public AudioPlay2D.AudioType type = AudioPlay2D.AudioType.Audio;
    public float volume = 1f;
    public bool loop = false;

    void Start()
    {
        AudioPlay2D.Play(pkg, type, resName, volume, loop);
    }

    void OnEnable()
    {
        AudioPlay2D.Resume(pkg, resName);
    }

    void OnDisable()
    {
        AudioPlay2D.Stop(pkg, resName);
    }

    void OnDestroy()
    {
        AudioPlay2D.Stop(pkg, resName);
    }
}
