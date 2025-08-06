using UnityEngine;

public class SpiningCylinder : MonoBehaviour
{
    public float rotationSpeed = 120f;
    private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		Quaternion deltaRotation = Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f);
		rb.MoveRotation(rb.rotation * deltaRotation);
	}
}
