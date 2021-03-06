﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class TimeScoreCtrl : MonoBehaviour {

	#region const

	private const int STAGE_NUM = 18;	// 全ステージ数
	private const int RANK_NUM = 3;		// ランクの段階数（Cランク除く）

	#endregion


	#region private property

	[SerializeField]
	private Text scoreText;		// スコア表示
	[SerializeField]
	private Text highscoreText;	// ハイスコアを表示するGUIText
	private float high_score;	// ハイスコア

	// PlayerPrefsで保存するためのキー　ステージごとに別に保存
	private string highScoreKey     = "highScore" + GameManager.Instance.stage_num;
	private string thistimeScoreKey = "thistimeScore";

	private TextAsset rank_asset;  // ステージテキストを取り込む
	string rank_txt;
	private float[,] rank_data = new float[STAGE_NUM, RANK_NUM];
	private string rankKey    = "rank" + GameManager.Instance.stage_num;

	[SerializeField]
	private GameObject[] rank_stamp_thistime;

	[SerializeField]
	private GameObject[] rank_stamp_hscore;

	[SerializeField]
	private Text to_next_rank;

	[SerializeField]
	private GameObject retire_txt;	// リタイアテキスト
	private bool isRegister;

	[SerializeField]
	private iTween.EaseType stampEaseType, retireEaseType;

	[SerializeField]
	UnityAdsController unity_ads_controller;

	#endregion


	#region event

	// Use this for initialization
	void Start () {
		Initialize ();

		// ハイスコアからステージごとのランクを決定するデータを得る
		#region decision rank
		rank_asset = Resources.Load ("rank_data1") as TextAsset;
		rank_txt = rank_asset.text;
		get_rank_data();
		#endregion

		string stageID = "stage" + GameManager.Instance.stage_num;

		// ゲームリタイアかどうか
		if(!GameManager.Instance.isRetire){
			// タイムがはやくなれば
			if (high_score > GameManager.Instance.time_score) {
				high_score = GameManager.Instance.time_score;
				Save();
			}

			// スコア・ハイスコアを表示する
			highscoreText.text = "HIGHSCORE : " + high_score;
			scoreText.text = "SCORE : " + GameManager.Instance.time_score;

			// ハイスコアからランクを登録する
			rank_register();

			// 現スコアの評価を与える
			rank_thistime ();

			// ゲームクリア時の分析
			Analytics.CustomEvent("gameClear", new Dictionary<string, object>{
				{"stage number", stageID},
				{"thistime score", GameManager.Instance.time_score},
				{"high score", high_score},
			});

		} else {
			// スコア・ハイスコアを表示する
			highscoreText.text = "";
			scoreText.text = "";
			retire_txt.SetActive (true);
			StartCoroutine (RetireAnim (retire_txt));

			// ランクが登録されていない状態でリタイアした場合はハイスコアを表示しない
			if(isRegister){
				highscoreText.text = "HIGHSCORE : " + high_score;
			}

			// ゲームリタイア時の分析
			Analytics.CustomEvent("gameRetire", new Dictionary<string, object>{
				{"stage number", stageID},
				{"thistime score time", GameManager.Instance.time_score},
			});

		}

		if(isRegister){
			// 次のランク解放のためのスコアを表示する
			next_rank ();
		} else {
			to_next_rank.text = "No Records";
		}
			
		// Unity Ads表示
		unity_ads_controller.WaitAndShowUnityAds (1.0f);
	}

	#endregion


	#region private method

	// 初期化
	private void Initialize(){ 
		
		// ハイスコアを取得する。保存されてなければ100を取得する。
		high_score = PlayerPrefs.GetFloat (highScoreKey, 100f);
		isRegister = true;
		if(high_score > 99f){
			isRegister = false;
		}
	}

	// ハイスコアの保存
	private void Save ()
	{
		// ハイスコアを保存する
		PlayerPrefs.SetFloat (highScoreKey, high_score);
		PlayerPrefs.SetFloat (thistimeScoreKey, GameManager.Instance.time_score);
		PlayerPrefs.Save ();
		// ゲーム開始前の状態に戻す
		Initialize ();
	}


	private void get_rank_data(){

		string[] lines = rank_txt.Split ('\n');
		int i=0, j=0;
		bool isBreak = false;

		//lines内の各行に対して、順番に処理していくループ
		foreach(var line in lines){
			if(line == ""){ //行がなければ
				continue;  
			}

			//print (line);
			string[] words = line.Split ();

			//words内の各ワードに対して、順番に処理していくループ
			foreach(var word in words){
				if(word.StartsWith("#")){	// ワードの先頭文字が#なら
					isBreak = true;
					break;
				}
				if(word == ""){
					continue;
				}
				rank_data [i, j] = float.Parse (word);
				j++;
				if (j > RANK_NUM-1){
					break;
				}
			}
			if(!isBreak){
				j = 0;
				i++;
			}
			isBreak = false;

			if (i > STAGE_NUM-1){
				break;
			}
		}
	}

	//
	private void rank_register(){
		bool isSet = false;

		for(int i=RANK_NUM; i>0; i--){

			// 高ランクからチェックして当てはまった時点で保存してbreak
			if(high_score < rank_data[GameManager.Instance.stage_num-1, i-1]){
				PlayerPrefs.SetInt (rankKey, i+1);
				PlayerPrefs.Save ();
				isSet = true;
				break;
			}
		}
		if(!isSet){
			PlayerPrefs.SetInt (rankKey, 1);	// Cランクを登録
			PlayerPrefs.Save ();
		}
	}

	//
	private void rank_thistime(){
		bool isSet = false;

		for(int i=RANK_NUM-1; i>=0; i--){

			// 高ランクからチェックして当てはまった時点でアクティブにしてbreak
			if(GameManager.Instance.time_score < rank_data[GameManager.Instance.stage_num-1, i]){
				
				rank_stamp_thistime [i+1].SetActive (true);
				StampAnim (rank_stamp_thistime [i + 1]);

				isSet = true;
				break;
			}
		}
		if(!isSet){
			rank_stamp_thistime [0].SetActive (true);
			StampAnim (rank_stamp_thistime [0]);
		}
	}

	//
	private void next_rank(){
		bool isSet = false;

		for(int i=RANK_NUM-1; i>=0; i--){

			if(high_score < rank_data[GameManager.Instance.stage_num-1, i]){
				if(i==RANK_NUM-1){
					to_next_rank.text = "Max Rank";
				} else {
					to_next_rank.text = "Next : " + rank_data [GameManager.Instance.stage_num - 1, i+1];
				}
				rank_stamp_hscore [i+1].SetActive (true);
				isSet = true;
				break;
			}
		}
		if(!isSet){
			to_next_rank.text = "Next : " + rank_data [GameManager.Instance.stage_num - 1, 0];
			rank_stamp_hscore[0].SetActive (true);
		}
	}

	// スタンプアニメーション
	private void StampAnim(GameObject obj){
		obj.transform.localScale = new Vector3 (2, 2, 2);
		iTween.ScaleTo(obj.gameObject, iTween.Hash("scale", Vector3.one,
			"time", 1,
			"easetype", stampEaseType
		));

		SoundManager.Instance.PlaySoundEffect (SoundManager.Instance.sound_stamp, 0.7f);
	}

	// リタイアアニメーション
	IEnumerator RetireAnim(GameObject obj){
		obj.SetActive (true);
		obj.GetComponent<Text> ().color = new Color(1, 0, 0, 1);
	
		iTween.MoveTo(obj.gameObject, iTween.Hash("position", Vector3.zero,
			"islocal", true,
			"time", 1,
			"easetype", retireEaseType
		));

		iTween.RotateTo(obj.gameObject, iTween.Hash("z", -30,
			"islocal", true,
			"time", 1,
			"easetype", retireEaseType
		));

		SoundManager.Instance.PlaySoundEffect (SoundManager.Instance.sound_retire, 0.1f);

		yield return new WaitForSeconds (2f);

		float timer = 0.8f;
		while(true){
			if (timer < 0)
				break;
			
				timer -= Time.deltaTime;		
				obj.GetComponent<Text> ().color = new Color (1, 0, 0, 0.2f + timer);
				yield return null;
		}
	}

	#endregion

}