using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public Transform playerTransform;
    public Transform headTransform;
    public Transform otherCam;

    public float mouseSensitivity = 100f;

    private float m_xRot = 0f;
    private float m_yRot = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        m_yRot += mouseX;
        m_xRot -= mouseY;

        m_xRot = Mathf.Clamp(m_xRot, -90f, 90f);

        // rotate the player
        playerTransform.rotation = Quaternion.Euler(0f, m_yRot, 0f);

        //rotate the cams and the head
        transform.rotation = otherCam.transform.rotation = headTransform.rotation = Quaternion.Euler(m_xRot, m_yRot, 0);
    }
}
