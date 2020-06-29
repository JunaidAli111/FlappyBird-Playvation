using UnityEngine;

public class Column : MonoBehaviour 
{
	//reference to the script of the both Top and Bottom Obstacles
	public SlideUpAndDown TopColumn;
	public SlideUpAndDown BottomColumn;

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.GetComponent<Bird>() != null)
		{
			//If the bird hits the trigger collider in between the columns then
			//tell the game control that the bird scored.
			GameControl.instance.BirdScored();

		}
	}

	//Start animating the obstacles, up and down.
	//This is for the special obstacles, as per the requirement of the assignment (Playvation)
	public void Animate(bool ShouldAnimate)
	{
		TopColumn.animate = ShouldAnimate;
		BottomColumn.animate = ShouldAnimate;
	}

	//Restore the obstacles to the original position.
	public void ResetToOriginalPos()
	{
		TopColumn.ResetColumn();
		BottomColumn.ResetColumn();
	}

}
