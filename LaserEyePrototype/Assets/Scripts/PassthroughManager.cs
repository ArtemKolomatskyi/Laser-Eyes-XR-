using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PassthroughManager : MonoBehaviour
{
    [SerializeField] private OVRPassthroughLayer defaultPassthroughLayer; // Default layer OnGameStarted
    [SerializeField] private OVRPassthroughLayer powerUpPassthroughLayer; // Alternate layer OnGameOver
    [SerializeField] private OVRPassthroughLayer gameOverPassthroughLayer; // Alternate layer OnPowerUp

    private OVRPassthroughLayer[] passthroughLayers;

    void Start()
    {
        // Initialize the array of passthrough layers
        passthroughLayers = new OVRPassthroughLayer[] { defaultPassthroughLayer, powerUpPassthroughLayer, gameOverPassthroughLayer };
        // Ensure only the first layer is enabled initially
        SetActiveLayer(0);
    }

    void Update()
    {
        // Example usage: cycle through layers on button press
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            // Cycle through layers as an example
            int nextLayerIndex = (GetActiveLayerIndex() + 1) % passthroughLayers.Length;
            SetActiveLayer(nextLayerIndex);
        }
    }

    // Method to set the active layer based on index
    public void SetActiveLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex >= passthroughLayers.Length)
        {
            Debug.LogError("Invalid layer index");
            return;
        }

        // Enable the specified layer and disable all others
        for (int i = 0; i < passthroughLayers.Length; i++)
        {
            passthroughLayers[i].enabled = (i == layerIndex);
        }
    }

    // Method to get the current active layer index
    private int GetActiveLayerIndex()
    {
        for (int i = 0; i < passthroughLayers.Length; i++)
        {
            if (passthroughLayers[i].enabled)
            {
                return i;
            }
        }

        // Return -1 if no layer is active (should not happen)
        return -1;
    }
}