using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class CountDownText : MonoBehaviour
{
	public delegate void CountdownFinished(); //Delegate to refence the Countdown Finishing event.
	public static event CountdownFinished OnCountdownFinished; //Event that triggers when Countdown is finished.

	Text countdown; //Text element to show the count

	/// <summary>
    /// Start the count down when the game object is activated.
    /// </summary>
	void OnEnable()
	{
		countdown = GetComponent<Text>();
		countdown.text = "3";
		StartCoroutine("Countdown");
	}

	/// <summary>
    /// Coroutine to display and update the countdown timer
    /// </summary>
	IEnumerator Countdown()
	{
		int count = 3;
		for (int i = 0; i < count; i++)
		{
			countdown.text = (count - i).ToString();
			yield return new WaitForSeconds(1);
		}

		OnCountdownFinished();
	}
}
