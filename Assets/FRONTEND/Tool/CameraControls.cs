using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControls : MonoBehaviour
{
    // [System.NonSerialized] indicates that the value cannot be changed in the inspector
    // this is important to prevent the sliders from being overridden

    // also all of these variables are instantiated with the same values the sliders start with
    // if not performed, then there'll be inconsistencies

    // move this many units forward per second
    [System.NonSerialized]
    public float move_speed = 15f;

    // rotate this many radians per second
    [System.NonSerialized]
    public float rotate_speed = 45f;

    // an alternate move_speed but for when the scroll wheel is used
    [System.NonSerialized]
    public float zoom_speed = 1000f;

    // Update is called once per frame
    void Update()
    {
        // check if allowed to monitor inputs via EventSystem
        // if EventSystem has nothing selected, then you've clicked off the UI
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            HandleMovementInputs();
            HandleRotationInputs();
            HandleZoomInputs();

            // as per the keybind info, if the user has clicked R
            // then reset the camera
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetCamera();
            }
        }
    }

    // allows the camera to be reset to its starting position and rotation
    // used for both the camera reset button and keybind
    public void ResetCamera()
    {
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
    }

    // my client would like the zoom to be implemented as if you're moving forward and backward
    // but on the scroll wheel instead, and with a separate speed slider
    private void HandleZoomInputs()
    {
        // moving forward/backward is along the local Z axis of the object
        float z_input = Input.GetAxisRaw("Mouse ScrollWheel");

        // convert to a vector for translation via multiplication by Z unit vector (forward)
        Vector3 velocity = z_input * Vector3.forward * zoom_speed;

        transform.Translate(velocity * Time.deltaTime);
    }

    // Input Manager defaults have been changed so that WASD and the arrow keys
    //  don't both count as movement inputs, so that arrow keys can be used here
    private void HandleRotationInputs()
    {
        Vector3 input = new Vector3
            (
                // X rotations look down (pos) and up (neg) hence the key choices
                GetCustomAxisInput(KeyCode.DownArrow, KeyCode.UpArrow),

                // Y rotations look right (pos) and left (neg) hence the key choices
                GetCustomAxisInput(KeyCode.RightArrow, KeyCode.LeftArrow),

                // Z rotations are tilts left (pos) and right (neg)
                // this isn't used often so it's not on the arrow keys
                // period is to the right of comma hence this ordering
                GetCustomAxisInput(KeyCode.Comma, KeyCode.Period)
            );

        // again, normalisation needs to occur otherwise rotations in combined directions are faster than non-combined ones
        Vector3 rotate_velocity = input.normalized * rotate_speed;

        transform.Rotate(rotate_velocity * Time.deltaTime);
    }

    private void HandleMovementInputs()
    {
        Vector3 input = new Vector3
            (
                Input.GetAxisRaw("Horizontal"),
                GetCustomAxisInput(KeyCode.Space, KeyCode.LeftControl),
                Input.GetAxisRaw("Vertical")
            );

        // input needs to be normalised so that diagonal movement isn't weirdly faster
        // (due to Pythagoras & vectors)
        Vector3 velocity = input.normalized * move_speed;

        // use Translate instead of simply adding the vector
        // Translate uses the object's local vector space, so that moving forwards always works even if rotations occur
        transform.Translate(velocity * Time.deltaTime);
    }

    /* OLD METHOD
     * This is an old method for combining the Vertical and MouseScrollWheel axis as valid move inputs
     * While keeping the fixed move speed even if both axes are used simultaneously
      
    private float GetCombinedVerticalAndScrollWheelAxis()
    {
        float combined_input = Input.GetAxisRaw("Vertical") + Input.GetAxisRaw("Mouse ScrollWheel");

        // this switch is necessary otherwise moving forward with both the A key and scrolling up will be faster than usual
        switch (combined_input)
        {
            // if net positive, move forward
            case > 0:
                return 1;

            // if net negative, move backward
            case < 0:
                return -1;

            // if they cancel each other out
            // or nothing is inputted
            // then don't move along this axis
            default:
                return 0;
        }
    }
     */

    // returns a value of -1 0 or 1, just like GetAxisRaw, so that it can be used alongside the Input Manager
    // written generally as there are several custom axes
    // e.g. up-down via space/ctrl, left-right rotation via left-right arrow keys
    private float GetCustomAxisInput(KeyCode keycode_positive, KeyCode keycode_negative)
    {
        // if both keys are held, it makes sense for these inputs to cancel out
        // this is consistent with how Unity implements its axes
        if (Input.GetKey(keycode_positive) & !Input.GetKey(keycode_negative))
        {
            return 1;
        }
        else if (Input.GetKey(keycode_negative) & !Input.GetKey(keycode_positive))
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }
}
