using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LifePowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Bird>() != null)
        {
            //As the bird collides or captures the power, award one life to the bird.
            GameControl.instance.UpdateLives(1);
            gameObject.SetActive(false);
        }
    }
}
