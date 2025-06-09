using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class bollControlScript : MonoBehaviour
{
    public float distanceFromCamera = 1.5f;
    public float throwPowerMultiplier = 0.01f;
    private Rigidbody rb;

    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool isThrown = false;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = true;
        ResetBallPosition();
    }

    void Update()
    {
        if (isThrown) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                Vector2 swipe = touchEndPos - touchStartPos;

                Vector3 direction = new Vector3(swipe.x, swipe.y, swipe.magnitude);
                direction = Camera.main.transform.TransformDirection(direction);
                float force = swipe.magnitude * throwPowerMultiplier;

                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(new Vector3(direction.x, direction.y * 1.5f, direction.z).normalized * force, ForceMode.Impulse);

                isThrown = true;

                if (GameManager.Instance != null)
                    GameManager.Instance.IncrementThrows();

                StartCoroutine(ResetAfterDelay(5f));
            }
        }
    }

    IEnumerator ResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetBall();
    }

    void ResetBall()
    {
        ResetBallPosition();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        isThrown = false;
    }

    void ResetBallPosition()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();
        startPosition = Camera.main.transform.position + cameraForward * distanceFromCamera;
        startPosition.y = Camera.main.transform.position.y - 0.5f;
        transform.position = startPosition;
        transform.rotation = Quaternion.LookRotation(cameraForward);
    }
}
