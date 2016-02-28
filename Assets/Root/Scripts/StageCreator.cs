﻿using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;

[RequireComponent (typeof(BlockCreator))]
public class StageCreator : MonoBehaviour {
	public static float BLOCK_WIDTH = 1.0f;
	public static float BLOCK_HEIGHT = 0.2f;
	public static int BLOCK_NUM_IN_SCREEN = 40;
	public static int SCREEN_HEIGHT = 24;

	GameObject Player;
	ScoreCtrl score_ctrl;
	PlayerCtrl player_ctrl;
	BlockCreator block_creator;

	private TextAsset stage_asset;  // ステージテキストを取り込む
	string stage_txt;
	public int[,] stage_data = new int[BLOCK_NUM_IN_SCREEN, SCREEN_HEIGHT];

	// Use this for initialization
	void Start () {
		this.Player = GameObject.FindGameObjectWithTag ("Player");
		this.score_ctrl = this.gameObject.GetComponent<ScoreCtrl> ();
		this.player_ctrl = GameObject.FindGameObjectWithTag ("PlayerA").GetComponent<PlayerCtrl> ();
		this.block_creator = GameObject.FindGameObjectWithTag ("Root").GetComponent<BlockCreator> ();
		// ステージテキストを取り込む
		stage_asset = Resources.Load ("stage1") as TextAsset;
		stage_txt = stage_asset.text;
		get_stage_data ();
		// ステージを作成する
		create_stage ();
	}

	// ステージ作成
	private void get_stage_data(){

		string[] lines = stage_txt.Split ('\n');
		int i=0, j=0;

		//lines内の各行に対して、順番に処理していくループ
		foreach(var line in lines){
			if(line == ""){ //行がなければ
				continue;  
			}

			//print (line);
			string[] words = line.Split ();

			//words内の各ワードに対して、順番に処理していくループ
			foreach(var word in words){
				if(word == ""){
					continue;
				}
				stage_data [i, j] = int.Parse (word);
				
				j++;
				if (j > SCREEN_HEIGHT-1){
					break;
				}
			}
			j = 0;
			i++;
			if (i > BLOCK_NUM_IN_SCREEN-1){
				break;
			}
		}
	}

	//
	private void create_stage(){
		int i, j;

		for(i=0; i<BLOCK_NUM_IN_SCREEN; i++){

			for(j=0; j<SCREEN_HEIGHT; j++){
				
				do {
					switch(stage_data[i,j]){
					// 中身が1なら赤作成
					case 1:
						block_creator.createBlock2(new Vector3((float)i, (float)j, 0.0f), 0);
						break;
					case 2:
						block_creator.createBlock2(new Vector3((float)i, (float)j, 0.0f), 1);
						break;
					case 3:
						block_creator.createBlock2(new Vector3((float)i, (float)j, 0.0f), 2);
						break;
					default:
						break;
					}
				} while(false);
			}
		}
	}
		

	// プレイヤーがステージ外へ出ているか
	public bool isOut(GameObject player){
		bool ret = false;

		do {
			// 左のしきい値の計算
			float limit_left = this.transform.position.x - (((float)BLOCK_NUM_IN_SCREEN + 8) / 2.0f);
			// ブロックがしきい値をでていたら削除の指示
			if(player.transform.position.x < limit_left){
				ret = true;
				break;
			}
			// 上のしきい値の計算
			float limit_up = this.transform.position.y + (float)SCREEN_HEIGHT;
			if(player.transform.position.y > limit_up){
				ret = true;
				break;
			}
			// 上のしきい値の計算
			float limit_down = this.transform.position.y - (float)SCREEN_HEIGHT;
			if(player.transform.position.y < limit_down){
				ret = true;
				break;
			}
		} while(false);

		return ret;
	}
}