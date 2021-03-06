﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenTransitionByButton : MonoBehaviour {

	#region public method

	public void ToTitle(){
		SoundManager.Instance.PlayTitleBGM ();
		FadeManager.Instance.LoadLevel ("scTitle0", 0.1f);
	}

	public void ToTimeAttack(){
		SoundManager.Instance.PlayBGM ();
		GameManager.Instance.stage_num = int.Parse(this.GetComponent<Text> ().text);
		FadeManager.Instance.LoadLevel ("scAttack", 0.1f);
	}

	public void BackToStageSelect(){
		Time.timeScale = 1.0f;
		SoundManager.Instance.PlayTitleBGM ();

		if(GameManager.Instance.game_mode == "TimeAttack"){
			FadeManager.Instance.LoadLevel ("scStageSelect", 0.1f);
		}
	}

	public void Redo(){
		SoundManager.Instance.PlayBGM ();

		if(GameManager.Instance.game_mode == "TimeAttack"){
			FadeManager.Instance.LoadLevel ("scAttack", 0.1f);

		} else if(GameManager.Instance.game_mode == "Scroll"){
			FadeManager.Instance.LoadLevel ("scScroll", 0.1f);
		}
	}

	#endregion
		
}
