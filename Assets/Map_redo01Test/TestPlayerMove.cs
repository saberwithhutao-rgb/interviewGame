using UnityEngine;
public class TestPlayerMove : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 1f;
    public CharacterController cc;
    private Vector3 velocity;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        cc.Move(dir * moveSpeed * Time.deltaTime);

        // ÖØÁ¦Âß¼­
        if (cc.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}
