using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{

	public TextAsset StoryJson;

	public Text OutputText;
	public Button ProceedButton;
	
	

	private Story _story;

	void Start()
	{
		_story = new Story(StoryJson.text);
		ProceedButton.onClick.AddListener(delegate { OnProceed(); });
		
		_story.BindExternalFunction("gender", () => GetGender());
		
		OnProceed();
	}

	private static int GetGender()
	{
		return 0; // Boy
	}


	void OnProceed()
	{
		if (_story.canContinue)
		{
			OutputText.text = _story.Continue();
			if (_story.currentChoices.Count > 0)
			{
				foreach (var choice in _story.currentChoices)
				{
					OutputText.text += $"\nChoice {choice.index}: {choice.text}";
				}

				OutputText.text += "\nWill choose first option";
				_story.ChooseChoiceIndex(0);
			}
		}
		else
		{
			OutputText.text = "End of story";
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
