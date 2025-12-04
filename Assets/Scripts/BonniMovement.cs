using UnityEngine;
using UnityEngine.InputSystem; 

public class BonniMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        // Read keyboard input (WASD / Arrow keys)
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                input.x -= 1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                input.x += 1;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                input.y += 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                input.y -= 1;
        }

        Vector3 movement = new Vector3(input.x, input.y, 0f);
        transform.position += movement.normalized * speed * Time.deltaTime;
    }
}
