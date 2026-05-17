using UnityEngine;
using TMPro;

public enum PoseGameState
{
    Ready,
    Playing,
    Paused,
    Finished
}

public class PoseManager : MonoBehaviour
{
    [Header("Tracked VR Points")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Poses")]
    public TargetPose[] poses;
    public int currentPoseIndex = 0;

    [Header("Timing")]
    public float requiredHoldTime = 3.0f;

    [Header("Feedback Text Objects")]
    public TMP_Text poseNameText;
    public TMP_Text feedbackText;
    public TMP_Text timerText;

    [Header("Target Pose Display")]
    public SpriteRenderer targetPoseDisplay;

    [Header("Game State")]
    public PoseGameState gameState = PoseGameState.Ready;

    private float holdTimer;

    void Start()
    {
        gameState = PoseGameState.Ready;

        if (feedbackText != null)
            feedbackText.text = "Press A to Start";

        if (timerText != null)
            timerText.text = requiredHoldTime.ToString("F0");

        if (poseNameText != null && poses != null && poses.Length > 0)
            poseNameText.text = poses[currentPoseIndex].poseName;
    }

    void Update()
    {
        if (head == null || leftHand == null || rightHand == null)
        {
            Debug.LogWarning("PoseManager is missing Head, Left Hand, or Right Hand reference.");
            return;
        }

        // B button on the right controller records pose values for debugging.
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            PrintCurrentPose();
        }

        // A button on the right controller starts, pauses, or resumes the game.
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            ToggleStartPause();
        }

        if (gameState != PoseGameState.Playing)
            return;

        if (poses == null || poses.Length == 0)
            return;

        TargetPose currentPose = poses[currentPoseIndex];

        Vector3 currentLeftFromHead = head.InverseTransformPoint(leftHand.position);
        Vector3 currentRightFromHead = head.InverseTransformPoint(rightHand.position);

        float playerArmSpan = Vector3.Distance(leftHand.position, rightHand.position);
        float scaleFactor = 1f;

        if (currentPose.recordedArmSpan > 0f)
        {
            scaleFactor = playerArmSpan / currentPose.recordedArmSpan;
        }

        Vector3 scaledLeftTarget = currentPose.leftHandFromHead * scaleFactor;
        Vector3 scaledRightTarget = currentPose.rightHandFromHead * scaleFactor;

        float leftError = Vector3.Distance(currentLeftFromHead, scaledLeftTarget);
        float rightError = Vector3.Distance(currentRightFromHead, scaledRightTarget);
        float averageError = (leftError + rightError) / 2f;

        Debug.Log("Pose: " + currentPose.poseName +
                  " | Left Error: " + leftError.ToString("F2") +
                  " | Right Error: " + rightError.ToString("F2") +
                  " | Average Error: " + averageError.ToString("F2"));

        bool poseMatched = averageError <= currentPose.allowedDistance;

        if (poseMatched)
        {
            holdTimer += Time.deltaTime;

            float remainingHoldTime = Mathf.Max(0f, requiredHoldTime - holdTimer);

            Debug.Log("Pose matched. Remaining hold time: " + remainingHoldTime.ToString("F2"));

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(remainingHoldTime).ToString();

            if (feedbackText != null)
                feedbackText.text = "Hold it!";

            if (holdTimer >= requiredHoldTime)
            {
                CompletePose();
                return;
            }
        }
        else
        {
            holdTimer = 0f;

            if (timerText != null)
                timerText.text = requiredHoldTime.ToString("F0");

            if (feedbackText != null)
            {
                if (averageError < currentPose.allowedDistance * 1.5f)
                    feedbackText.text = "Close!";
                else
                    feedbackText.text = "Far Away";
            }
        }
    }

    void StartCurrentPose()
    {
        holdTimer = 0f;

        TargetPose currentPose = poses[currentPoseIndex];

        if (poseNameText != null)
            poseNameText.text = currentPose.poseName;

        if (feedbackText != null)
            feedbackText.text = "Match the pose!";

        if (timerText != null)
            timerText.text = requiredHoldTime.ToString("F0");

        if (targetPoseDisplay != null)
            targetPoseDisplay.sprite = currentPose.poseImage;
    }

    void CompletePose()
    {
        Debug.Log("Pose complete: " + poses[currentPoseIndex].poseName);

        if (feedbackText != null)
            feedbackText.text = "Great!";

        NextPose();
    }

    void NextPose()
    {
        currentPoseIndex++;

        if (currentPoseIndex >= poses.Length)
            currentPoseIndex = 0;

        StartCurrentPose();
    }

    void PrintCurrentPose()
    {
        Vector3 left = head.InverseTransformPoint(leftHand.position);
        Vector3 right = head.InverseTransformPoint(rightHand.position);
        float armSpan = Vector3.Distance(leftHand.position, rightHand.position);

        Debug.Log("=== Recorded Pose ===");
        Debug.Log("Left Hand From Head: " + left);
        Debug.Log("Right Hand From Head: " + right);
        Debug.Log("Arm Span: " + armSpan);
    }

    void ToggleStartPause()
    {
        if (gameState == PoseGameState.Ready)
        {
            StartGame();
        }
        else if (gameState == PoseGameState.Playing)
        {
            PauseGame();
        }
        else if (gameState == PoseGameState.Paused)
        {
            ResumeGame();
        }
    }

    void StartGame()
    {
        if (poses == null || poses.Length == 0)
        {
            if (feedbackText != null)
                feedbackText.text = "No poses loaded";

            return;
        }

        currentPoseIndex = 0;
        gameState = PoseGameState.Playing;
        StartCurrentPose();
    }

    void PauseGame()
    {
        gameState = PoseGameState.Paused;

        if (feedbackText != null)
            feedbackText.text = "Paused";
    }

    void ResumeGame()
    {
        gameState = PoseGameState.Playing;

        if (feedbackText != null)
            feedbackText.text = "Match the pose!";
    }
}
