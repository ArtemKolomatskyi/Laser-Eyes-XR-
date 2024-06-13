using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.XR;

public class AlienPortalManager : MonoBehaviour
{
    [SerializeField] private GameObject _portal;
    [SerializeField] private ParticleSystem _portalParticleSystem;
    [SerializeField] private AnimateLocalScale _portalAnimateLocalScale;
    [SerializeField] private Experiment369Intro _experiment369Intro;
    private bool openPortal = false;
    private Transform _playerCamera;
    private GameObject _spawnedPortal;
    private GameManager _gameManager;

    void Start()
    {
        _playerCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        if(!_portalParticleSystem) _portalParticleSystem = _portal.GetComponent<ParticleSystem>();
        if(!_portalAnimateLocalScale) _portalAnimateLocalScale = _portal.GetComponent<AnimateLocalScale>();
        _gameManager = FindObjectOfType<GameManager>();
        _gameManager.onGameWon.AddListener(HidePortal);
        _gameManager.onGameOver.AddListener(HidePortal);
        
        _portalParticleSystem.Stop();
    }

    public void OpenPortal()
    {
        openPortal = true;
    }
    
    private void ShowPortal()
    {
        _portalParticleSystem.Play();
        _portalAnimateLocalScale.MaximizeObject();
        StartCoroutine(ShowExperiment369());
    }

    private IEnumerator ShowExperiment369()
    {
        yield return new WaitForSeconds(_portalAnimateLocalScale.animTime);
        _experiment369Intro.ShowExperiment369(_portal.transform.position, 
            (_portal.transform.position + (_portal.transform.forward * 0.3f)),
            (_portal.transform.rotation * Quaternion.Euler(new Vector3(0f, 180f, 0f))));
        yield return new WaitForSeconds(_experiment369Intro.experiment369OutOfPortalDelay);
        _gameManager.onIntroStart.Invoke();     //TTS Speech Intro
    }
    
    
    private void HidePortal()
    {
        _portalAnimateLocalScale.MinimizeObject();
        StartCoroutine(HidePortalCoroutine());
    }
    private IEnumerator HidePortalCoroutine()
    {
        yield return new WaitForSeconds(_portalAnimateLocalScale.animTime);
        _portalParticleSystem.Stop();
    }
    
    
    private void FixedUpdate()
    {
        if (openPortal)
        {
            _OpenPortal();
        }
    }
    
    // private void 

    private void _OpenPortal()
    {
        Ray ray = GetRayFromCamera();

        MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT;
        Pose? bestPose = GetBestPoseFromRaycast(ray, Mathf.Infinity, new LabelFilter(),
            out MRUKAnchor sceneAnchor, out Vector3 surfaceNormal, positioningMethod);
        
        bool hitIsOnHorizontalSurface = Mathf.Abs(Vector3.Dot(surfaceNormal, Vector3.up)) >= 0.9f;
        
        if (bestPose.HasValue && sceneAnchor)
        {
            if (!hitIsOnHorizontalSurface)
            {
                _portal.transform.position = bestPose.Value.position;
                _portal.transform.rotation = bestPose.Value.rotation;
                
                #if UNITY_EDITOR
                    XRGizmos.DrawPointer(bestPose.Value.position, bestPose.Value.up, Color.green);
                #endif
                
                openPortal = false;
                ShowPortal();
            }
            else
            {
                //todo: Inform player to look at a vertical surface
                log("Camera is facing a horizontal surface");
                #if UNITY_EDITOR
                    XRGizmos.DrawPointer(bestPose.Value.position, bestPose.Value.up, Color.red);
                #endif
            }
        }
        else
        {
            //if there is no pose it means something went wrong and the current placement is not valid
        }
    }
    
    Ray GetRayFromCamera()
    {
        return new Ray(_playerCamera.position, _playerCamera.forward);
    }
    
    
    
    private Pose GetBestPoseFromRaycast(Ray ray, float maxDist, LabelFilter labelFilter, out MRUKAnchor sceneAnchor, 
        out Vector3 surfaceNormal, MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT)
    {
        sceneAnchor = null;
        Pose bestPose = new Pose();
        surfaceNormal = Vector3.up;

        if (MRUK.Instance.GetCurrentRoom().Raycast(ray, maxDist, labelFilter, out var closestHit, out sceneAnchor))
        {
            Vector3 defaultPose = closestHit.point;
            surfaceNormal = closestHit.normal;
            Vector3 poseUp = Vector3.up;
            // by default, use the surface normal for pose forward
            // caution: make sure all the cases of this being "up" are caught below
            Vector3 poseFwd = closestHit.normal;

            if (Vector3.Dot(closestHit.normal, Vector3.up) >= 0.9f && sceneAnchor.VolumeBounds.HasValue)
            {
                // this is a volume object, and the ray has hit the top surface
                // "snap" the pose Z to align with the closest edge
                Vector3 toPlane = ray.origin - sceneAnchor.transform.position;
                Vector3 planeYup = Vector3.Dot(sceneAnchor.transform.up, toPlane) > 0.0f ? sceneAnchor.transform.up : -sceneAnchor.transform.up;
                Vector3 planeXup = Vector3.Dot(sceneAnchor.transform.right, toPlane) > 0.0f ? sceneAnchor.transform.right : -sceneAnchor.transform.right;
                Vector3 planeFwd = sceneAnchor.transform.forward;

                Vector2 anchorScale = sceneAnchor.VolumeBounds.Value.size;
                Vector3 nearestCorner = sceneAnchor.transform.position + planeXup * anchorScale.x * 0.5f + planeYup * anchorScale.y * 0.5f;
                Vector3.OrthoNormalize(ref planeFwd, ref toPlane);
                nearestCorner -= sceneAnchor.transform.position;
                bool xUp = Vector3.Angle(toPlane, planeYup) > Vector3.Angle(nearestCorner, planeYup);
                poseFwd = xUp ? planeXup : planeYup;
                float offset = xUp ? anchorScale.x : anchorScale.y;
                switch (positioningMethod)
                {
                    case MRUK.PositioningMethod.CENTER:
                        defaultPose = sceneAnchor.transform.position;
                        break;
                    case MRUK.PositioningMethod.EDGE:
                        defaultPose = sceneAnchor.transform.position + poseFwd * offset * 0.5f;
                        break;
                    case MRUK.PositioningMethod.DEFAULT:
                        break;
                }
            }
            else if (Mathf.Abs(Vector3.Dot(closestHit.normal, Vector3.up)) >= 0.9f)
            {
                // This may be the floor, ceiling or any other horizontal plane surface
                poseFwd = new Vector3(ray.origin.x - closestHit.point.x, 0, ray.origin.z - closestHit.point.z).normalized;
            }
            bestPose.position = defaultPose;
            bestPose.rotation = Quaternion.LookRotation(poseFwd, poseUp);
        }
        else
        {
            Debug.Log("Best pose not found, no surface anchor detected.");
        }

        return bestPose;
    }
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
