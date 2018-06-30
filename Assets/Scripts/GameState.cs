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

	void Awake()
	{
		_story = new Story(StoryJson.text);
		ProceedButton.onClick.AddListener(delegate { OnProceed(); });

		_story.variablesState["gender"] = 0;
		
		Debug.Log("Avaliable variables: ");
		foreach (var variable in _story.variablesState)
		{
			Debug.Log($"{variable} : {_story.variablesState[variable]}");
		}
		_story.ChoosePathString("start");
		
		
		OnProceed();
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
	

	// Use this for initialization
	void Start () {
		_story = new Story(StoryJson.text);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
