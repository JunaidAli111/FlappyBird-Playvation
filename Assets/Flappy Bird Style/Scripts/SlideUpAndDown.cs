using UnityEngine;

/// <summary>
/// This script makes the obstacle slide or translate up and down on y-axis to create a special effect
/// Its purpose is to introduce a new type of obstacle in the game
/// </summary>
public class SlideUpAndDown : MonoBehaviour
{
    
    public float slideSpeed;                //speed with which obstacle will slide
    public float directionChangeTimer;      //Float value to count till the Change Rate
    public float directionChangeRate;       //After how much time the direction of animation should change
    public Vector3 originalPos;             //Original position of the obstacle

    public bool originalDirection;          //True Indicates the obstacle is a top obstacle and vice verca
    bool direction;                         //True indicates the obstacle to translate downwards and vice verca
    public bool animate;                    //Should the obstacle animate?

    void Start()
    {
        //store the essentials
        originalPos = transform.localPosition;
        direction = originalDirection;
    }


    // Update is called once per frame
    void Update()
    {
        //no animation
        if (!animate) return;

        //count the time
        directionChangeTimer += Time.deltaTime;
        //change the direction once enough time (Direction change rate) is gone
        if (directionChangeTimer > directionChangeRate)
        {
            //reverse animation direction
            direction = !direction;
            //start counting the time again till change of direction
            directionChangeTimer = 0;
        }

        //true indicates downard movement
        if (direction)
        {
            //move obstacle downward with the speed 'slideSpeed'
            transform.position += Vector3.down * slideSpeed * Time.deltaTime;
        }
        //upward movement
        else
        {
            //move obstacle upward with the speed 'slideSpeed'
            transform.position += Vector3.up * slideSpeed * Time.deltaTime;
        }
    }


    //reset the values to originals
    public void ResetColumn()
    {
        transform.localPosition = originalPos;
        directionChangeTimer = 0f;
        direction = originalDirection;
    }

}
