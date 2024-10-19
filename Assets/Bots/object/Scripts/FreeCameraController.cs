
using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float moveSpeed = 10f;    
    public float lookSpeed = 2f;     

    private float yaw = 0f;          
    private float pitch = 0f;        

    void Update()
    {
       
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90f, 90f); 

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

      
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime; 
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;   
        float moveY = 0f;

      
        if (Input.GetKey(KeyCode.Q)) moveY = -moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) moveY = moveSpeed * Time.deltaTime;

    
        transform.Translate(new Vector3(moveX, moveY, moveZ));
    }
}
