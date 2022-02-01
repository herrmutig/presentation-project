using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Controller : MonoBehaviour
{
    private Controls m_controls;
    private Movement m_movement;

    void Awake()
    {
        m_movement = GetComponent<Movement>();    
    }


    void OnEnable()
    {
        if (m_controls == null)
        {
            m_controls = new Controls();

            // Listen to Event.
            m_controls.Gameplay.Movement.performed += i => 
            {
                m_movement.SetMovementInput(i.ReadValue<Vector2>()); 
            };
        }

        m_controls.Enable();
    }

    void OnDisable()
    {
        if (m_controls != null)
            m_controls.Disable();
    }
}
