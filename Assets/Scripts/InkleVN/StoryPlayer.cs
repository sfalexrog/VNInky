using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using GraphicsPools;
using Ink.Runtime;
using InkleVN;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.UI;

public class StoryPlayer : MonoBehaviour
{

	public TextAsset StoryJson;

	public Text OutputText;
	public Image OutputBackground;
	public Button ProceedButton;

	public StoryUIController UI;	

	private Story _story;
	
	// Valid actor names; actor names from scripts will be compared against this set
	private HashSet<string> _registeredActors;

	// Reserved name for player actor
	private static string _playerActorName = "Я";
	// Gender-specific player actor names
	private static string _playerBoyName = "Player Boy";
	private static string _playerGirlName = "Player Girl";
	
	void Start()
	{
		_registeredActors = UI.GetActorNames();
		_story = new Story(StoryJson.text);
		ProceedButton.onClick.AddListener(delegate { OnProceed(); });
		
		_story.BindExternalFunction("gender", () => GetGender());
		UI.SetOnChoiceHandler(OnChoice);
		/*
		Debug.Log(LayoutUtility.GetPreferredHeight(OutputText.rectTransform));
		Debug.Log(LayoutUtility.GetPreferredHeight(OutputBackground.rectTransform));
		OutputText.text = "Lorem ipsum dolores etc etc etc\n\nThis is some multiline text";
		Debug.Log(LayoutUtility.GetPreferredHeight(OutputText.rectTransform));
		Debug.Log(LayoutUtility.GetPreferredHeight(OutputBackground.rectTransform));
		
		OutputText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredHeight(OutputText.rectTransform));
		OutputBackground.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredHeight(OutputText.rectTransform) + 30);
		*/
		
		OnProceed();
	}
	
	
	// FIXME: pull this value from the proper game state
	private static int GetGender()
	{
		return 0; // Boy
	}

	private class Phrase
	{
		public string ActorName;
		public string ActorEmotion;
		public string Text;
	}

	private Phrase ParsePhrase(string storyPhrase, List<string> tags)
	{
		Phrase parsedPhrase = new Phrase();
		var splitPhrase = storyPhrase.Split(':');
		// Check if there's an actor that says anything
		if (splitPhrase.Length > 1)
		{
			// Check against player actor name
			if (splitPhrase[0].Equals(_playerActorName))
			{
				parsedPhrase.ActorName = GetGender() == 0 ? _playerBoyName : _playerGirlName;
				StringBuilder sb = new StringBuilder();

				for (int i = 1; i < splitPhrase.Length; ++i)
				{
					sb.Append(splitPhrase[i]);
					if (i < splitPhrase.Length - 1)
					{
						sb.Append(": ");
					}
				}

				parsedPhrase.Text = sb.ToString();
			}
			else if (_registeredActors.Contains(splitPhrase[0]))
			{
				parsedPhrase.ActorName = splitPhrase[0];
				StringBuilder sb = new StringBuilder();

				for (int i = 1; i < splitPhrase.Length; ++i)
				{
					sb.Append(splitPhrase[i]);
					if (i < splitPhrase.Length - 1)
					{
						sb.Append(": ");
					}
				}

				parsedPhrase.Text = sb.ToString();
			}
			else
			{
				// Not an actor and not a player character -> the whole phrase
				// is just a non-character phrase
				parsedPhrase.Text = storyPhrase;
			}
		}
		else
		{
			// The whole phrase is text, there's no actor that speaks it
			parsedPhrase.Text = storyPhrase;
		}
		// A hack: if a tag only contains one word, it's an emotion
		foreach (var tag in tags)
		{
			var tagSplit = tag.Split(':');
			if (tagSplit.Length == 1)
			{
				parsedPhrase.ActorEmotion = tag;
				break;
			}
		}

		if (parsedPhrase.ActorName != null &&
		    parsedPhrase.ActorEmotion == null)
		{
			parsedPhrase.ActorEmotion = "default";
		}

		return parsedPhrase;
	}

	private void OnChoice(int choice)
	{
		_story.ChooseChoiceIndex(choice);
		OnProceed();
	}

	public void OnProceed()
	{
		if (!UI.WillAcceptTransitions) return;
		var transitionBuilder = new SceneTransitionRequest.Builder();
		if (_story.canContinue)
		{
			var nextStoryText = _story.Continue();
			var parsedText = ParsePhrase(nextStoryText, _story.currentTags);

			transitionBuilder.SetSpeaker(parsedText.ActorName)
				.SetPhrase(parsedText.Text)
				.SetSpeaker(parsedText.ActorName, parsedText.ActorEmotion);
			
			if (_story.currentTags.Count > 0)
			{
				Debug.Log("Current tags:");
				foreach (var tag in _story.currentTags)
				{
					Debug.Log($"\n{tag}");
				}
			}
			
			if (_story.currentChoices.Count > 0)
			{
				foreach (var choice in _story.currentChoices)
				{
					Debug.Log($"\nChoice {choice.index}: {choice.text}");
					transitionBuilder.AddChoice(choice.text);
				}
			}
		}
		else
		{
			// finish?
		}
		UI.Transition(transitionBuilder.Build());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
