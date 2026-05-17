using UnityEngine;

[CreateAssetMenu(fileName = "NewTargetPose", menuName = "VR Dance/Target Pose")]
public class TargetPose : ScriptableObject
{
    public string poseName;

    public Vector3 leftHandFromHead;
    public Vector3 rightHandFromHead;

    public float allowedDistance = 0.25f;

    public float recordedArmSpan = 1.0f;

    public Sprite poseImage;

}
