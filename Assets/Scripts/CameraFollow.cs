using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public float smoothSpeed = 2f;
    public float yOffset = 4f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        float targetY = target.position.y + yOffset;

        if (targetY > pos.y)
        {
            pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smoothSpeed);
            transform.position = pos;
        }
    }

    // Called by GameManager
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
