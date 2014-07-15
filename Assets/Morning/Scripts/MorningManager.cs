﻿using UnityEngine;
using System;
using System.Collections;

public class MorningManager : SceneManager {
	NetworkManager mNetworkManager;
	DialogueManager mDialogueManager;
	Game mGame;

	string[] mGoodDialogues = new string[]{
		"Wow! That was one of our best ever nights. Keep it up!",
		// TODO: More good dialogues
	};

	string[] mBadDialogues = new string[]{
		"That was bad",
		// TODO: Bad dialogues
	};

	string[] mMiddleDialogues = new string[]{
		"That was okay",
		// TODO: Middle dialogues
	};

	void Awake() {
		mNetworkManager = (NetworkManager)FindObjectOfType (typeof(NetworkManager));
		mDialogueManager = (DialogueManager)FindObjectOfType (typeof(DialogueManager));
		mGame = (Game)FindObjectOfType(typeof(Game));
	}

	void Start () {
		// First we need to set everyone to "Not Ready"
		if (Network.isServer) {
			foreach (Player player in mNetworkManager.players) {
				player.networkView.RPC ("SetReady", RPCMode.All, false);
			}
		}

		if (mNetworkManager.myPlayer.uDay == 1) {
			FirstDay();
		} else if(mNetworkManager.myPlayer.uDay < mGame.NUMBER_OF_DAYS) {
			MiddleDay();
		} else {
			LastDay();
		}
	}

	void FirstDay() {
		string[] day1FirstDialogue = new string[]{
			"This is (station name). The public access TV station you've worked at for several years",
			"Here comes your boss, (boss' name)"
		};

		Action day1FirstDialogueFinished =
		() => {

			Action day1DialogueFinished =
			() => {
				mNetworkManager.myPlayer.networkView.RPC ("SetShowName", RPCMode.All, "TempShowName");
				mDialogueManager.StartDialogue("Waiting for other players to continue");
				mNetworkManager.myPlayer.networkView.RPC("SetReady", RPCMode.All, true);
			};

			// TODO: Add the input for the show name

			string[] day1SecondDialogue = new string[]{
				"Good morning (player's name)!",
				"Listen... I've been meaning to talk to you",
				"You see, things aren't going so great at (station name)",
				"(other station name) are killing us in the ratings",
				"The bosses... they wanted to let you go... (old tv theme) just isn't doing it any more",
				"We're trying to recreate (station name) to be new, exciting, vibrant",
				"You've got one week to turn it around - and we're giving you a new show to run",
				"Our market research suggests that (theme) would really be a big hit with modern audiences",
			};
			
			mDialogueManager.StartDialogue(day1SecondDialogue, day1DialogueFinished);
		};

		mDialogueManager.StartDialogue(day1FirstDialogue, day1FirstDialogueFinished);
	}

	void MiddleDay() {
		// TODO: figure out if we're in the top/middle/bottom 3rd of players
		// for now we just show middle
		Action dialogueFinished =
			() => {
				Application.LoadLevel("Feedback");
		};

		mDialogueManager.StartDialogue(mMiddleDialogues[0], dialogueFinished);
	}

	void LastDay() {
		// TODO: figure out if we're in the top/middle/bottom 3rd of players
		// for now we just show middle
		Action firstDialogueFinished =
			() => {
				Action dialogueFinished =
					() => {
						Application.LoadLevel("Feedback");
				};

				string[] lastDialogue = new string[] {
					"This is the last day......", // TODO
				};

				mDialogueManager.StartDialogue(lastDialogue, dialogueFinished);
		};

		mDialogueManager.StartDialogue(mMiddleDialogues[0], firstDialogueFinished);
	}

	/**
	 * This is called on the server when any player changes their ready status
	 */
	public override void ReadyStatusChanged(Player pPlayer) {
		if (pPlayer.uReady) {
			if (!mGame.DEBUG_MODE2) {
				// Check if all players are ready - if so we can start
				foreach (Player p in mNetworkManager.players) {
					if (!p.uReady) {
						return;
					}
				}
			}

			// Everyone is ready, let's move to the next scene
			networkView.RPC ("MoveToNextScene", RPCMode.All);
		}
	}

	/**
	 * Move to the prop selection scene
	 */
	[RPC] void MoveToNextScene() {
		mDialogueManager.EndDialogue();
		Application.LoadLevel ("PropSelection");
	}
}
