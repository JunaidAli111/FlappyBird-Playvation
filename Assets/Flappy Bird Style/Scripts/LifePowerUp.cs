using UnityEngine;


public class LifePowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Bird>() != null)
        {
            //Powerup sound
            GameControl.instance.powerUpSound.Play();
            //As the bird collides or captures the power, award one life to the bird.
            GameControl.instance.UpdateLives(1);
            gameObject.SetActive(false);
        }
    }
}
