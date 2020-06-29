using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class associated with High Score Text UI
/// </summary>
public class HighScoreText : MonoBehaviour
{
	Text score;

	/// <summary>
    /// Display the high score Text as soon as game is over and the high score is retrieved from the firebase cloud
    /// </summary>
	void OnEnable()
	{
		score = GetComponent<Text>();
		score.text = "High Score: " + GameControl.instance.HighScore.ToString();
	}
}
