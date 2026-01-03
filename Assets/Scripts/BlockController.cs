using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float moveSpeed = 1f; // THIS NOW REALLY CONTROLS SPEED

    private bool isMoving = true;
    private float minX;
    private float maxX;

    private float phase; // our own time accumulator

    void Start()
    {
        CalculateBounds();
        phase = Random.value * Mathf.PI * 2f; // random start, looks natural
    }

    // void Update()
    // {
    //     if (StackGameManager.InputLocked) return;

    //     if (Time.timeScale == 0f) return;

    //     if (!isMoving) return;

    //     // advance phase using deltaTime (KEY FIX)
    //     phase += moveSpeed * Time.deltaTime;

    //     float t = (Mathf.Sin(phase) + 1f) * 0.5f;
    //     float x = Mathf.Lerp(minX, maxX, t);

    //     transform.position = new Vector3(
    //         x,
    //         transform.position.y,
    //         transform.position.z
    //     );

    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         StopBlock();
    //     }
    // }

    void Update()
    {
        // 1️⃣ Global input lock (pause, resume, UI)
        if (StackGameManager.InputLocked) return;

        // 2️⃣ Game paused
        if (Time.timeScale == 0f) return;

        // 3️⃣ Not moving (already placed)
        if (!isMoving) return;

        // 4️⃣ Movement
        phase += moveSpeed * Time.deltaTime;

        float t = (Mathf.Sin(phase) + 1f) * 0.5f;
        float x = Mathf.Lerp(minX, maxX, t);

        transform.position = new Vector3(
            x,
            transform.position.y,
            transform.position.z
        );

        // 5️⃣ Tap handling (CRITICAL FIX)
        if (Input.GetMouseButtonDown(0))
        {
            // 🚫 Ignore UI clicks
            // if (UnityEngine.EventSystems.EventSystem.current != null &&
            //     UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            //     return;
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            StopBlock();
        }
    }

    void CalculateBounds()
    {
        Camera cam = Camera.main;

        float distance = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float frustumHeight = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth = frustumHeight * cam.aspect;

        float halfBlock = transform.localScale.x * 0.5f;

        minX = -frustumWidth * 0.5f + halfBlock;
        maxX =  frustumWidth * 0.5f - halfBlock;
    }

    void StopBlock()
    {
        isMoving = false;
        StackGameManager.Instance.PlaceBlock(gameObject);
        Destroy(this);
    }
}


