using UnityEngine;
using TMPro;

public class WaveTextUpdater : MonoBehaviour
{
    private TMP_Text waveText;

    void Start()
    {
        // Get the TMP_Text component from the child object
        waveText = GetComponentInChildren<TMP_Text>();

        if (waveText == null)
        {
            Debug.LogError("TMP_Text component not found in children of the plane game object.");
        }
    }

    void Update()
    {
        // Check for key presses from 1 to 9
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                UpdateWaveText(i);
            }
        }
    }

    void UpdateWaveText(int waveNumber)
    {
        waveText.text = "Wave " + waveNumber;
    }
}
