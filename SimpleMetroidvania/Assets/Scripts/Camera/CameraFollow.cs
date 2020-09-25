using UnityEngine;

// Old camera script, I am currently not using it and using Cinemachine instead
public class CameraFollow : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private float smoothSpeed = 5f;
	[SerializeField] private Vector3 offset;

	[SerializeField] private Vector2 minCameraPos;
	[SerializeField] private Vector2 maxCameraPos;

	private void LateUpdate()
	{
		Vector3 desiredPosition = target.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
		smoothedPosition.z = -1;

		transform.position = new Vector3(Mathf.Clamp(smoothedPosition.x, minCameraPos.x, maxCameraPos.x), Mathf.Clamp(smoothedPosition.y, minCameraPos.y, maxCameraPos.y), smoothedPosition.z);
	}
}
