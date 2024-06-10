using UnityEngine;
using VolumetricLines;
using DG.Tweening;
using System.Collections;

public class LaserControl : MonoBehaviour
{
    public GameObject laserPrefab; // Assign the Volumetric Line prefab in the inspector
    public Transform leftEmitter; // Assign the left emitter transform in the inspector
    public Transform rightEmitter; // Assign the right emitter transform in the inspector

    public KeyCode laserActivationKey = KeyCode.L; // Customizable key for laser activation
    public KeyCode cameraSwitchKey = KeyCode.C; // Customizable key for camera switch

    public Camera firstPersonCamera; // Assign the first-person camera in the inspector
    public Camera thirdPersonCamera; // Assign the third-person camera in the inspector

    public float growSpeed = 10f; // Speed at which the laser grows
    public float fadeDuration = 1f; // Duration for laser to fade
    public Vector3 laserScale = new Vector3(1f, 1f, 1f); // Scale of the laser beams

    private VolumetricLineBehavior leftLaser;
    private VolumetricLineBehavior rightLaser;
    private bool lasersActive = false;

    private Coroutine leftLaserCoroutine;
    private Coroutine rightLaserCoroutine;

    // Reference to the HMD Rotation Anchor
    public Transform hmdRotationAnchor;

    void Start()
    {
        // Instantiate laser objects at the start but keep them inactive
        leftLaser = Instantiate(laserPrefab, leftEmitter.position, leftEmitter.rotation, leftEmitter).GetComponent<VolumetricLineBehavior>();
        rightLaser = Instantiate(laserPrefab, rightEmitter.position, rightEmitter.rotation, rightEmitter).GetComponent<VolumetricLineBehavior>();

        leftLaser.gameObject.SetActive(false);
        rightLaser.gameObject.SetActive(false);

        // Set the laser scale
        leftLaser.transform.localScale = laserScale;
        rightLaser.transform.localScale = laserScale;

        // Set laser color to red
        leftLaser.LineColor = Color.red;
        rightLaser.LineColor = Color.red;

        // Ensure the cameras are set correctly at start
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }

    void Update()
    {
        // Check for input to activate/deactivate lasers
        if (Input.GetKeyDown(laserActivationKey))
        {
            ToggleLasers(true);
        }
        if (Input.GetKeyUp(laserActivationKey))
        {
            ToggleLasers(false);
        }

        // Check for input to switch cameras
        if (Input.GetKeyDown(cameraSwitchKey))
        {
            SwitchCamera();
        }

        // Update laser direction if in first-person view
        if (firstPersonCamera.enabled && lasersActive)
        {
            UpdateLaserDirection(leftLaser, leftEmitter);
            UpdateLaserDirection(rightLaser, rightEmitter);
        }
    }

    void ToggleLasers(bool state)
    {
        lasersActive = state;

        if (state)
        {
            leftLaser.gameObject.SetActive(true);
            rightLaser.gameObject.SetActive(true);

            leftLaserCoroutine = StartCoroutine(GrowLaser(leftLaser, leftEmitter));
            rightLaserCoroutine = StartCoroutine(GrowLaser(rightLaser, rightEmitter));
        }
        else
        {
            if (leftLaserCoroutine != null)
            {
                StopCoroutine(leftLaserCoroutine);
                leftLaserCoroutine = null;
            }
            if (rightLaserCoroutine != null)
            {
                StopCoroutine(rightLaserCoroutine);
                rightLaserCoroutine = null;
            }

            FadeLaser(leftLaser, fadeDuration);
            FadeLaser(rightLaser, fadeDuration);
        }
    }

    IEnumerator GrowLaser(VolumetricLineBehavior laser, Transform emitter)
    {
        laser.StartPos = emitter.localPosition;
        laser.EndPos = emitter.localPosition;
        laser.gameObject.SetActive(true);

        float length = 0f;

        while (true)
        {
            length += growSpeed * Time.deltaTime;
            laser.EndPos = emitter.localPosition + emitter.forward * length;
            yield return null;
        }
    }

    void UpdateLaserDirection(VolumetricLineBehavior laser, Transform emitter)
    {
        // Use the forward direction of the HMD Rotation Anchor to update laser direction
        laser.StartPos = emitter.localPosition;
        laser.EndPos = emitter.localPosition + hmdRotationAnchor.forward * 100f; // Adjust the multiplier for desired length
    }

    void FadeLaser(VolumetricLineBehavior laser, float duration)
    {
        Material mat = laser.transform.GetComponent<Material>();
        if (!mat) mat = laser.GetComponentInChildren<Material>();
 
        mat.DOFade(0, duration).OnComplete(() =>
        {
            laser.gameObject.SetActive(false);
            laser.SetStartAndEndPoints(laser.StartPos, laser.StartPos); // Reset to initial state
            laser.LineColor = Color.red; // Reset color to red
        });
    }

    void SwitchCamera()
    {
        firstPersonCamera.enabled = !firstPersonCamera.enabled;
        thirdPersonCamera.enabled = !thirdPersonCamera.enabled;
    }
}
