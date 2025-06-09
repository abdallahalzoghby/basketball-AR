using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceHoop : MonoBehaviour
{
    [SerializeField] GameObject m_HoopPrefab;
    [SerializeField] GameObject m_BallPrefab;

    public GameObject spawnedHoop { get; private set; }
    public GameObject spawnedBall { get; private set; }

    public static event Action onPlacedObject;
    private bool isPlaced = false;
    private ARRaycastManager m_RaycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (isPlaced) return;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began &&
                m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = s_Hits[0].pose;
                spawnedHoop = Instantiate(m_HoopPrefab, hitPose.position, Quaternion.Euler(0, 180, 0));
                isPlaced = true;
                StartCoroutine(SpawnBallAfterDelay(5f));
                onPlacedObject?.Invoke();
            }
        }
    }

    IEnumerator SpawnBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject arCamera = m_RaycastManager.transform.Find("AR Camera").gameObject;
        Vector3 spawnPos = arCamera.transform.position
                           + arCamera.transform.forward * 1.2f
                           + arCamera.transform.up * -0.4f;
        spawnedBall = Instantiate(m_BallPrefab, spawnPos, Quaternion.identity);
    }
}
