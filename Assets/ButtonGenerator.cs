using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGenerator : MonoBehaviour {

    public Button OriginalButton;

    public GameObject ButtonParent;

    public Button[] InstantiatedButtons;

    public void CreateButtons()
    {
        InstantiatedButtons = new Button[10];
        for(int i = 0; i < 10; ++i)
        {
            InstantiatedButtons[i] = Instantiate<Button>(OriginalButton, ButtonParent.transform);
            InstantiatedButtons[i].gameObject.SetActive(true);
            InstantiatedButtons[i].image.rectTransform.anchoredPosition = OriginalButton.image.rectTransform.anchoredPosition + new Vector2(0, -i * OriginalButton.image.rectTransform.rect.height * 1.5f);
            var buttonIdx = i;
            InstantiatedButtons[i].onClick.AddListener(delegate { Debug.Log("Pressed button " + buttonIdx.ToString()); });
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
