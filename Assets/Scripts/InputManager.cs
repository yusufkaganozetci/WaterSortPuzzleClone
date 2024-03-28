using UnityEngine;

public class InputManager : MonoBehaviour
{
    
    void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform.gameObject.CompareTag("Bottle"))
                {
                    EventManager.Instance.TriggerActionEvent(EventType.BottlePickRequested,
                        hit.transform.parent.transform);
                }
            }
        }
    }

}