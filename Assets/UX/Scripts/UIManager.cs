using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public struct UXHandle
{
    public UIManager.InstructionUI InstructionalUI;
    public UIManager.InstructionGoals Goal;

    public UXHandle(UIManager.InstructionUI ui, UIManager.InstructionGoals goal)
    {
        InstructionalUI = ui;
        Goal = goal;
    }
}

public class UIManager : MonoBehaviour
{
    [SerializeField] bool m_StartWithInstructionalUI = true;
    public bool startWithInstructionalUI
    {
        get => m_StartWithInstructionalUI;
        set => m_StartWithInstructionalUI = value;
    }

    public enum InstructionUI
    {
        CrossPlatformFindAPlane,
        FindAFace,
        FindABody,
        FindAnImage,
        FindAnObject,
        ARKitCoachingOverlay,
        TapToPlace,
        None
    };

    [SerializeField] InstructionUI m_InstructionalUI;
    public InstructionUI instructionalUI
    {
        get => m_InstructionalUI;
        set => m_InstructionalUI = value;
    }

    public enum InstructionGoals
    {
        FoundAPlane,
        FoundMultiplePlanes,
        FoundAFace,
        FoundABody,
        FoundAnImage,
        FoundAnObject,
        PlacedAnObject,
        None
    };

    [SerializeField] InstructionGoals m_InstructionalGoal;
    public InstructionGoals instructionalGoal
    {
        get => m_InstructionalGoal;
        set => m_InstructionalGoal = value;
    }

    [SerializeField] bool m_ShowSecondaryInstructionalUI;
    public bool showSecondaryInstructionalUI
    {
        get => m_ShowSecondaryInstructionalUI;
        set => m_ShowSecondaryInstructionalUI = value;
    }

    [SerializeField] InstructionUI m_SecondaryInstructionUI = InstructionUI.TapToPlace;
    public InstructionUI secondaryInstructionUI
    {
        get => m_SecondaryInstructionUI;
        set => m_SecondaryInstructionUI = value;
    }

    [SerializeField] InstructionGoals m_SecondaryGoal = InstructionGoals.PlacedAnObject;
    public InstructionGoals secondaryGoal
    {
        get => m_SecondaryGoal;
        set => m_SecondaryGoal = value;
    }

    [SerializeField] bool m_CoachingOverlayFallback;
    public bool coachingOverlayFallback
    {
        get => m_CoachingOverlayFallback;
        set => m_CoachingOverlayFallback = value;
    }

    [SerializeField] GameObject m_ARSessionOrigin;
    public GameObject arSessionOrigin
    {
        get => m_ARSessionOrigin;
        set => m_ARSessionOrigin = value;
    }

    [SerializeField] ARCameraManager m_CameraManager;
    public ARCameraManager cameraManager
    {
        get => m_CameraManager;
        set
        {
            if (m_CameraManager == value) return;
            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;
            m_CameraManager = value;
            if (m_CameraManager != null && enabled)
                m_CameraManager.frameReceived += FrameChanged;
        }
    }

    Func<bool> m_GoalReached;
    bool m_SecondaryGoalReached;
    Queue<UXHandle> m_UXOrderedQueue;
    UXHandle m_CurrentHandle;
    bool m_ProcessingInstructions;
    bool m_PlacedObject;

    [SerializeField] ARPlaneManager m_PlaneManager;
    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    [SerializeField] ARFaceManager m_FaceManager;
    public ARFaceManager faceManager
    {
        get => m_FaceManager;
        set => m_FaceManager = value;
    }

    [SerializeField] ARHumanBodyManager m_BodyManager;
    public ARHumanBodyManager bodyManager
    {
        get => m_BodyManager;
        set => m_BodyManager = value;
    }

    [SerializeField] ARTrackedImageManager m_ImageManager;
    public ARTrackedImageManager imageManager
    {
        get => m_ImageManager;
        set => m_ImageManager = value;
    }

    [SerializeField] ARTrackedObjectManager m_ObjectManager;
    public ARTrackedObjectManager objectManager
    {
        get => m_ObjectManager;
        set => m_ObjectManager = value;
    }

    [SerializeField] ARUXAnimationManager m_AnimationManager;
    public ARUXAnimationManager animationManager
    {
        get => m_AnimationManager;
        set => m_AnimationManager = value;
    }

    [SerializeField] LocalizationManager m_LocalizationManager;
    public LocalizationManager localizationManager
    {
        get => m_LocalizationManager;
        set => m_LocalizationManager = value;
    }

    [SerializeField] Animator m_MoveDeviceAnimation;
    public Animator moveDeviceAnimation
    {
        get => m_MoveDeviceAnimation;
        set => m_MoveDeviceAnimation = value;
    }

    [SerializeField] Animator m_TapToPlaceAnimation;
    public Animator tapToPlaceAnimation
    {
        get => m_TapToPlaceAnimation;
        set => m_TapToPlaceAnimation = value;
    }

    bool m_ShowingTapToPlace = false;
    bool m_ShowingMoveDevice = true;
    bool m_FadedOff = false;

    void OnEnable()
    {
        ARUXAnimationManager.onFadeOffComplete += FadeComplete;
        if (m_CameraManager != null)
            m_CameraManager.frameReceived += FrameChanged;

        PlaceHoop.onPlacedObject += OnPlacedObject;

        GetManagers();
        m_UXOrderedQueue = new Queue<UXHandle>();

        if (m_StartWithInstructionalUI)
            m_UXOrderedQueue.Enqueue(new UXHandle(m_InstructionalUI, m_InstructionalGoal));

        if (m_ShowSecondaryInstructionalUI)
            m_UXOrderedQueue.Enqueue(new UXHandle(m_SecondaryInstructionUI, m_SecondaryGoal));
    }

    void OnDisable()
    {
        ARUXAnimationManager.onFadeOffComplete -= FadeComplete;
        if (m_CameraManager != null)
            m_CameraManager.frameReceived -= FrameChanged;

        PlaceHoop.onPlacedObject -= OnPlacedObject;
    }

    void Update()
    {
        if (m_AnimationManager.localizeText && !m_LocalizationManager.localizationComplete)
            return;

        if (m_UXOrderedQueue.Count > 0 && !m_ProcessingInstructions)
        {
            m_CurrentHandle = m_UXOrderedQueue.Dequeue();
            m_GoalReached = GetGoal(m_CurrentHandle.Goal);
            if (m_GoalReached.Invoke()) return;

            FadeOnInstructionalUI(m_CurrentHandle.InstructionalUI);
            m_ProcessingInstructions = true;
            m_FadedOff = false;
        }

        if (m_ProcessingInstructions && m_GoalReached.Invoke())
        {
            if (!m_FadedOff)
            {
                m_FadedOff = true;
                m_AnimationManager.FadeOffCurrentUI();
            }
        }
    }

    void FrameChanged(ARCameraFrameEventArgs args)
    {
        if (PlanesFound() && m_ShowingMoveDevice)
        {
            moveDeviceAnimation?.SetTrigger("FadeOff");
            tapToPlaceAnimation?.SetTrigger("FadeOn");

            m_ShowingTapToPlace = true;
            m_ShowingMoveDevice = false;
        }
    }

    void OnPlacedObject()
    {
        m_PlacedObject = true;

        if (m_ShowingTapToPlace)
        {
            tapToPlaceAnimation?.SetTrigger("FadeOff");
            m_ShowingTapToPlace = false;
        }
    }

    void GetManagers()
    {
        if (!m_ARSessionOrigin) return;

        m_ARSessionOrigin.TryGetComponent(out m_PlaneManager);
        m_ARSessionOrigin.TryGetComponent(out m_FaceManager);
        m_ARSessionOrigin.TryGetComponent(out m_BodyManager);
        m_ARSessionOrigin.TryGetComponent(out m_ImageManager);
        m_ARSessionOrigin.TryGetComponent(out m_ObjectManager);
    }

    Func<bool> GetGoal(InstructionGoals goal)
    {
        return goal switch
        {
            InstructionGoals.FoundAPlane => PlanesFound,
            InstructionGoals.FoundMultiplePlanes => MultiplePlanesFound,
            InstructionGoals.FoundABody => BodyFound,
            InstructionGoals.FoundAFace => FaceFound,
            InstructionGoals.FoundAnImage => ImageFound,
            InstructionGoals.FoundAnObject => ObjectFound,
            InstructionGoals.PlacedAnObject => PlacedObject,
            _ => () => false,
        };
    }

    void FadeOnInstructionalUI(InstructionUI ui)
    {
        switch (ui)
        {
            case InstructionUI.CrossPlatformFindAPlane:
                m_AnimationManager.ShowCrossPlatformFindAPlane(); break;
            case InstructionUI.FindAFace:
                m_AnimationManager.ShowFindFace(); break;
            case InstructionUI.FindABody:
                m_AnimationManager.ShowFindBody(); break;
            case InstructionUI.FindAnImage:
                m_AnimationManager.ShowFindImage(); break;
            case InstructionUI.FindAnObject:
                m_AnimationManager.ShowFindObject(); break;
            case InstructionUI.ARKitCoachingOverlay:
                if (m_AnimationManager.ARKitCoachingOverlaySupported())
                    m_AnimationManager.ShowCoachingOverlay();
                else if (m_CoachingOverlayFallback)
                    m_AnimationManager.ShowCrossPlatformFindAPlane();
                break;
            case InstructionUI.TapToPlace:
                m_AnimationManager.ShowTapToPlace(); break;
        }
    }

    bool PlanesFound() => m_PlaneManager && m_PlaneManager.trackables.count > 0;
    bool MultiplePlanesFound() => m_PlaneManager && m_PlaneManager.trackables.count > 1;
    bool FaceFound() => m_FaceManager && m_FaceManager.trackables.count > 0;
    bool BodyFound() => m_BodyManager && m_BodyManager.trackables.count > 0;
    bool ImageFound() => m_ImageManager && m_ImageManager.trackables.count > 0;
    bool ObjectFound() => m_ObjectManager && m_ObjectManager.trackables.count > 0;

    bool PlacedObject()
    {
        if (m_PlacedObject)
        {
            m_PlacedObject = false;
            return true;
        }
        return false;
    }

    void FadeComplete() => m_ProcessingInstructions = false;

    public void AddToQueue(UXHandle uxHandle) => m_UXOrderedQueue.Enqueue(uxHandle);

    public void TestFlipPlacementBool() => m_PlacedObject = true;
}