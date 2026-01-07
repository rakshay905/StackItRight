// using UnityEngine;

// public class BlockController : MonoBehaviour
// {
//     public float moveSpeed = 1f;
//     public float minSpeed = 0.6f;
//     public float maxSpeed = 2.2f;


//     private bool isMoving = true;
//     private float minX;
//     private float maxX;

//     private float phase; // our own time accumulator

//     float direction = 1f; // +1 = right, -1 = left

//     // void Start()
//     // {
//     //     CalculateBounds();
//     //     phase = Random.value * Mathf.PI * 2f; // random start, looks natural
//     // }

//     void Start()
//     {
//         CalculateBounds();

//         // Normalize current X position into 0–1 range
//         float t = Mathf.InverseLerp(minX, maxX, transform.position.x);

//         // Convert t into phase so sin(phase) maps correctly
//         phase = Mathf.Asin(t * 2f - 1f);

//         // decide initial direction
//         direction = Random.value > 0.5f ? 1f : -1f;
//     }


//     void Update()
//     {
//         // 1️⃣ Global input lock (pause, resume, UI)
//         if (StackGameManager.InputLocked) return;

//         // 2️⃣ Game paused
//         if (Time.timeScale == 0f) return;

//         // 3️⃣ Not moving (already placed)
//         if (!isMoving) return;

//         // 4️⃣ Movement
//         // phase += moveSpeed * Time.deltaTime;
//         float speed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
//         // phase += speed * Time.deltaTime;
//         phase += speed * direction * Time.deltaTime;

//         float t = (Mathf.Sin(phase) + 1f) * 0.5f;
//         float x = Mathf.Lerp(minX, maxX, t);

//         transform.position = new Vector3(
//             x,
//             transform.position.y,
//             transform.position.z
//         );

//         // 5️⃣ Tap handling (CRITICAL FIX)
//         if (Input.GetMouseButtonDown(0))
//         {
//             // 🚫 Ignore UI clicks
//             // if (UnityEngine.EventSystems.EventSystem.current != null &&
//             //     UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
//             //     return;
//             if (UnityEngine.EventSystems.EventSystem.current != null &&
//                 UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() &&
//                 UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
//                 return;

//             StopBlock();
//         }
//     }

//     void CalculateBounds()
//     {
//         Camera cam = Camera.main;

//         float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
//         float frustumHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
//         float frustumWidth = frustumHeight * cam.aspect;

//         float halfBlock = transform.localScale.x * 0.5f;

//         minX = -frustumWidth * 0.5f + halfBlock;
//         maxX =  frustumWidth * 0.5f - halfBlock;
//     }

//     void StopBlock()
//     {
//         isMoving = false;
//         StackGameManager.Instance.PlaceBlock(gameObject);
//         Destroy(this);
//     }
// }


using UnityEngine;
using UnityEngine.EventSystems;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float minSpeed = 0.6f;
    public float maxSpeed = 2.2f;

    private bool isMoving = true;
    private float minX;
    private float maxX;
    private float phase;

    float direction = 1f; // +1 = right, -1 = left

    void Start()
    {
        CalculateBounds();

        float t = Mathf.InverseLerp(minX, maxX, transform.position.x);
        phase = Mathf.Asin(t * 2f - 1f);

        direction = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        if (StackGameManager.InputLocked) return;
        if (Time.timeScale == 0f) return;
        if (!isMoving) return;

        float speed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
        phase += speed * direction * Time.deltaTime;

        float t = (Mathf.Sin(phase) + 1f) * 0.5f;
        float x = Mathf.Lerp(minX, maxX, t);

        transform.position = new Vector3(
            x,
            transform.position.y,
            transform.position.z
        );

        if (IsTapDetected())
        {
            StopBlock();
        }
    }

    bool IsTapDetected()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return false;

            return true;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return false;

            return true;
        }
#endif
        return false;
    }

    void CalculateBounds()
    {
        Camera cam = Camera.main;

        float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float frustumHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        float halfBlock = transform.localScale.x * 0.5f;

        minX = -frustumWidth * 0.5f + halfBlock;
        maxX = frustumWidth * 0.5f - halfBlock;
    }

    void StopBlock()
    {
        if (StackGameManager.InputLocked) return;

        isMoving = false;
        StackGameManager.Instance.PlaceBlock(gameObject);
        Destroy(this);
    }
}
