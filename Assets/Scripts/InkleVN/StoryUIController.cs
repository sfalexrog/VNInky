using System.Collections;
using System.Collections.Generic;
using GraphicsPools;
using InkleVN;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class StoryUIController : MonoBehaviour {

    private ActorPool _actorPool;
    private BackgroundPool _backgroundPool;

    [Header("UI elements")]
    public Image Background;
    public Image BackgroundTransition;
    public Image PhraseBackground;
    public Text PhraseText;
    public Image ActorNameBox;
    public Text ActorName;
    public Image ActorImage;


    public Button TapTarget;

    [Header("UI prototype elements")]

    public Image UIContainer;
    public Button ChoiceButtonPrototype;

    [Header("Animation controls")]
    public float FadeOutDuration;
    public AnimationCurve FadeOutCurve;

    public float FadeInDuration;
    public AnimationCurve FadeInCurve;

    public float TransitionDuration;
    public AnimationCurve TransitionCurve;

    [Header("Animation targets")]
    public Image PlayerPhraseBackgroundAnchor;

    public Image DescriptionBackgroundAnchor;
    public Image NPCPhraseBackgroundAnchor;

    public Image ChoiceBackgroundAnchor;

    [Header("Choice colors")]
    public Color GoodChoice;
    public Color NeutralChoice;
    public Color BadChoice;

    public delegate void OnChoice(int choiceIndex);

    private OnChoice _choiceHandler;

    private Text _shadowText;

    private Button[] _choiceButtons;

    // Actor image and name canvas group (used to show/hide both actor image and actor name box)
    private CanvasGroup _actorGroup;

    public void SetOnChoiceHandler(OnChoice handler)
    {
        _choiceHandler = handler;
    }

    private bool _willAcceptTransitions;

    public bool WillAcceptTransitions => _willAcceptTransitions;

    // Animation for RectTransform
    private class RectAnimation
    {
        public float TimeStart;
        public float TimeEnd;

        public AnimationCurve Curve;
        public RectTransform Target;

        public Vector2 InitialPosition;
        public Vector2 TargetPosition;

        public Vector2 InitialSize;
        public Vector2 TargetSize;

        public bool DidFinish;

        public RectAnimation(RectTransform target, float timeStart, float duration, AnimationCurve curve,
            RectTransform targetTransform)
        {
            Target = target;
            TimeStart = timeStart;
            TimeEnd = timeStart + duration;
            Curve = curve;
            InitialPosition = target.anchoredPosition;
            TargetPosition = targetTransform.anchoredPosition;
            InitialSize = new Vector2(target.rect.width, target.rect.height);
            TargetSize = new Vector2(targetTransform.rect.width, targetTransform.rect.height);
            DidFinish = TimeEnd < TimeStart;

        }
    }

    // Animation for Graphic-derivatives
    private class FadeAnimation
    {
        public float TimeStart;
        public float TimeEnd;

        public AnimationCurve Curve;
        public Graphic Target;

        // Opacity values for text (since we're only doing that)
        public float InitialAlpha;
        public float TargetAlpha;

        public bool DidFinish;

        // Convenience constructor
        public FadeAnimation(Graphic target, float timeStart, float duration, AnimationCurve curve,
            float targetAlpha)
        {
            Target = target;
            TimeStart = timeStart;
            TimeEnd = timeStart + duration;
            Curve = curve;
            //InitialAlpha = target.color.a;
            // hack: initial alpha is always 1-targetAlpha
            InitialAlpha = 1.0f - targetAlpha;
            TargetAlpha = targetAlpha;
            DidFinish = TimeEnd < TimeStart;
        }
    }

    // Pseudo-animation to set text
    private class SetTextAnimation
    {
        // We do this momentarilly, so no duration here.
        public float TimeSet;

        public Text Target;
        public string TargetText;

        public bool DidFinish;

        public SetTextAnimation(Text target, float timeSet, string targetText = null)
        {
            TimeSet = timeSet;
            Target = target;
            TargetText = targetText;
            DidFinish = false;
        }
    }

    // Pseudo-animation to set sprite
    private class SetSpriteAnimation
    {
        public float TimeSet;

        public Image Target;
        public Sprite TargetSprite;

        public bool DidFinish;

        public SetSpriteAnimation(Image target, float timeSet, Sprite targetSprite)
        {
            TimeSet = timeSet;
            Target = target;
            TargetSprite = targetSprite;
            DidFinish = false;
        }
    }

    // Animation to fade in/out a canvas group
    private class FadeCGAnimation
    {
        public float TimeStart;
        public float TimeEnd;

        public AnimationCurve Curve;
        public CanvasGroup Target;

        // Opacity values for text (since we're only doing that)
        public float InitialAlpha;
        public float TargetAlpha;

        public bool DidFinish;

        // Convenience constructor
        public FadeCGAnimation(CanvasGroup target, float timeStart, float duration, AnimationCurve curve,
            float targetAlpha)
        {
            Target = target;
            TimeStart = timeStart;
            TimeEnd = timeStart + duration;
            Curve = curve;
            //InitialAlpha = target.color.a;
            // hack: initial alpha is always 1-targetAlpha
            InitialAlpha = 1.0f - targetAlpha;
            TargetAlpha = targetAlpha;
            DidFinish = TimeEnd < TimeStart;
        }
    }



    // Animation group: animations that should be played simultaneously.
    // Only one animation group will be played at a time.
    private class AnimGroup
    {
        // Start and end times for animation group. They are populated automatically.
        public float TimeStart;
        public float TimeEnd;

        // Animation lists for the current group
        public List<RectAnimation> RectAnimations;
        public List<FadeAnimation> FadeAnimations;
        public List<SetTextAnimation> SetTextAnimations;
        public List<SetSpriteAnimation> SetSpriteAnimations;
        public List<FadeCGAnimation> FadeCGAnimations;

        public AnimGroup()
        {
            TimeEnd = TimeStart = Time.time;
            RectAnimations = new List<RectAnimation>();
            FadeAnimations = new List<FadeAnimation>();
            SetTextAnimations = new List<SetTextAnimation>();
            SetSpriteAnimations = new List<SetSpriteAnimation>();
            FadeCGAnimations = new List<FadeCGAnimation>();
        }

        public AnimGroup AddAnimation(RectAnimation ra)
        {
            TimeStart = Mathf.Min(TimeStart, ra.TimeStart);
            TimeEnd = Mathf.Max(TimeEnd, ra.TimeEnd);
            RectAnimations.Add(ra);
            return this;
        }

        public AnimGroup AddAnimation(FadeAnimation ta)
        {
            TimeStart = Mathf.Min(TimeStart, ta.TimeStart);
            TimeEnd = Mathf.Max(TimeEnd, ta.TimeEnd);
            FadeAnimations.Add(ta);
            return this;
        }

        public AnimGroup AddAnimation(SetTextAnimation sta)
        {
            TimeStart = Mathf.Min(TimeStart, sta.TimeSet);
            TimeEnd = Mathf.Max(TimeEnd, sta.TimeSet);
            SetTextAnimations.Add(sta);
            return this;
        }
        
        public AnimGroup AddAnimation(SetSpriteAnimation ssa)
        {
            TimeStart = Mathf.Min(TimeStart, ssa.TimeSet);
            TimeEnd = Mathf.Max(TimeEnd, ssa.TimeSet);
            SetSpriteAnimations.Add(ssa);
            return this;
        }

        public AnimGroup AddAnimation(FadeCGAnimation fcga)
        {
            TimeStart = Mathf.Min(TimeStart, fcga.TimeStart);
            TimeEnd = Mathf.Max(TimeEnd, fcga.TimeEnd);
            FadeCGAnimations.Add(fcga);
            return this;
        }
    }

    private Queue<AnimGroup> _pendingAnimations;

    void Awake()
    {
        _willAcceptTransitions = true;
        _actorPool = new ActorPool(false);
        _backgroundPool = new BackgroundPool(false);
        _pendingAnimations = new Queue<AnimGroup>();
        _actorGroup = ActorImage.GetComponent<CanvasGroup>();
    }

    /**
	 * Get registered actor names. This is required to distinguish between
	 * a "real" actor name and something that just happened to have a colon before it.
	 */
    public HashSet<string> GetActorNames()
    {
        return _actorPool.GetActorNames();
    }

    private float GetDesiredTextWidth(Text textComponent, string textContents, Vector2 requestedSize)
    {
        if (_shadowText == null)
        {
            _shadowText = Instantiate<Text>(textComponent);
            _shadowText.color = new Color(0, 0, 0, 0);
        }
        _shadowText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requestedSize.x);
        _shadowText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requestedSize.y);

        _shadowText.text = textContents;
        return LayoutUtility.GetPreferredWidth(_shadowText.rectTransform);
    }

    private float GetDesiredTextHeight(Text textComponent, string textContents, Vector2 requestedSize)
    {
        if (_shadowText == null)
        {
            _shadowText = Instantiate<Text>(textComponent);
            _shadowText.color = new Color(0, 0, 0, 0);
        }
        _shadowText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requestedSize.x);
        _shadowText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requestedSize.y);

        _shadowText.text = textContents;
        _shadowText.rectTransform.ForceUpdateRectTransforms();
        return LayoutUtility.GetPreferredHeight(_shadowText.rectTransform);
    }

    // Animate actor group hiding
    private void HideActor()
    {
        // Account for floats being floats
        if (_actorGroup.alpha < 0.001f) return;
        var animGroup = new AnimGroup();
        animGroup.AddAnimation(new FadeCGAnimation(_actorGroup, Time.time, FadeOutDuration, FadeOutCurve, 0.0f));
        _pendingAnimations.Enqueue(animGroup);
    }

    private void ShowActor(SceneTransitionRequest str)
    {
        var timeOffset = 0.0f;
        // If the actor name is already on screen, change it gracefully
        if (_actorGroup.alpha > 0.99f)
        {
            if (ActorName.text != str.TransitionSpeaker)
            {
                var animGroup = new AnimGroup();
                animGroup.AddAnimation(new FadeAnimation(ActorName, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
                    .AddAnimation(new SetTextAnimation(ActorName, Time.time + FadeOutDuration, str.TransitionSpeaker))
                    .AddAnimation(new FadeAnimation(ActorName, Time.time + FadeOutDuration, FadeInDuration, FadeInCurve, 1.0f));
                timeOffset = FadeOutDuration + FadeInDuration;
                _pendingAnimations.Enqueue(animGroup);
            }
        }
        else
        {
            // Change the name without any animation, the fade in from the canvas group will do the rest
            ActorName.text = str.TransitionSpeaker;
            _pendingAnimations.Enqueue(new AnimGroup().AddAnimation(new FadeCGAnimation(_actorGroup, Time.time + timeOffset, FadeInDuration, FadeInCurve, 1.0f)));
        }
    }

    private void TransitionBackground(SceneTransitionRequest str)
    {
        var animGroup = new AnimGroup();
        BackgroundTransition.sprite = _backgroundPool.GetBackgroundSprite(str.TransitionBackground.ToLower());
        animGroup.AddAnimation(new FadeAnimation(BackgroundTransition, Time.time, FadeInDuration, FadeInCurve, 1.0f))
            .AddAnimation(new SetSpriteAnimation(Background, Time.time + FadeInDuration, BackgroundTransition.sprite))
            .AddAnimation(new FadeAnimation(BackgroundTransition, Time.time + FadeInDuration, 0.001f, FadeOutCurve, 0.0f));
        _pendingAnimations.Enqueue(animGroup);
    }

    private void TransitionMainText(SceneTransitionRequest str, Image anchorTarget)
    {
        var animGroup = new AnimGroup();
        var textFadeOut = new FadeAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f);
        animGroup.AddAnimation(textFadeOut);

        var lastAnimFinish = textFadeOut.TimeEnd;

        var defaultTextBoxSize = new Vector2(anchorTarget.rectTransform.rect.width - 30,
                                            anchorTarget.rectTransform.rect.height - 35);

        var requiredHeight = GetDesiredTextHeight(PhraseText, str.TransitionPhrase, defaultTextBoxSize);

        if (PhraseBackground.rectTransform.anchoredPosition != anchorTarget.rectTransform.anchoredPosition ||
            !Mathf.Approximately(requiredHeight, PhraseText.rectTransform.rect.height))
        {
            var textBoxResize = new RectAnimation(PhraseBackground.rectTransform, textFadeOut.TimeEnd, TransitionDuration, TransitionCurve, anchorTarget.rectTransform);
            // Fixup for correct size
            textBoxResize.TargetSize.y = requiredHeight + 35;
            animGroup.AddAnimation(textBoxResize);
            lastAnimFinish = textBoxResize.TimeEnd;
        }

        var textChange = new SetTextAnimation(PhraseText, lastAnimFinish, str.TransitionPhrase);
        animGroup.AddAnimation(textChange);
        var textFadeIn = new FadeAnimation(PhraseText, lastAnimFinish, FadeInDuration, FadeInCurve, 1.0f);
        animGroup.AddAnimation(textFadeIn);
        _pendingAnimations.Enqueue(animGroup);
    }

    private void CreateChoices(SceneTransitionRequest str)
    {
        var animGroup = new AnimGroup();
        var curTime = Time.time;
        if (_choiceButtons != null)
        {
            foreach(var btn in _choiceButtons)
            {
                Destroy(btn);
            }
        }
        _choiceButtons = new Button[str.TransitionChoices.Length];
        for(int i = 0; i < str.TransitionChoices.Length; ++i)
        {
            var choiceButton = Instantiate<Button>(ChoiceButtonPrototype, UIContainer.transform);
            var choice = str.TransitionChoices[i];
            choiceButton.gameObject.SetActive(true);

            var choiceButtonCG = choiceButton.GetComponent<CanvasGroup>();
            choiceButtonCG.alpha = 0.0f;
            animGroup.AddAnimation(new FadeCGAnimation(choiceButtonCG, curTime + i * FadeInDuration, FadeInDuration, FadeInCurve, 1.0f));

            choiceButton.image.rectTransform.anchoredPosition = ChoiceButtonPrototype.image.rectTransform.anchoredPosition + new Vector2(0.0f, -i * (ChoiceButtonPrototype.image.rectTransform.rect.height + 40));

            choiceButton.onClick.AddListener(delegate { _choiceHandler(choice.ChoiceID); });

            var choiceButtonText = choiceButton.GetComponentInChildren<Text>();
            choiceButtonText.text = choice.ChoiceText;

            _choiceButtons[i] = choiceButton;
        }
        _pendingAnimations.Enqueue(animGroup);
    }

    private void HideChoices()
    {
        if (_choiceButtons != null)
        {
            foreach(var btn in _choiceButtons)
            {
                btn.gameObject.SetActive(false);
                Destroy(btn);
            }
            _choiceButtons = null;
        }
    }
    
	
	/**
	 * Transition scene to another state
	 */
	public bool Transition(SceneTransitionRequest str)
	{
        HideChoices();
		if (!_willAcceptTransitions) return false;
        if (str.TransitionBackground != null)
        {
            TransitionBackground(str);
        }
        if (str.TransitionSpeaker == null)
		{
            HideActor();
            TransitionMainText(str, DescriptionBackgroundAnchor);
		}
		else
		{
            ShowActor(str);
			ActorImage.sprite = _actorPool.GetActorSprite(str.TransitionSpeaker, str.TransitionSpeakerEmotion);
			if (str.TransitionChoices == null)
			{
                // Enable generic tap target if there are no choices
                TapTarget.gameObject.SetActive(true);
				if (str.TransitionSpeaker.Contains("Player"))
				{
                    
                    TransitionMainText(str, PlayerPhraseBackgroundAnchor);
				}
				else
				{
                    TransitionMainText(str, NPCPhraseBackgroundAnchor);
				}
			}
            else
            {
                // Disable generic tap target
                TapTarget.gameObject.SetActive(false);
                TransitionMainText(str, ChoiceBackgroundAnchor);
                CreateChoices(str);
            }
		}

		return true;
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_pendingAnimations.Count > 0)
		{
			// Disable tap target during animation
			_willAcceptTransitions = false;
			// Play back animations
			var animGroup = _pendingAnimations.Peek();
			var t = Time.time;
			foreach (var textAnim in animGroup.FadeAnimations)
			{
				if (t < textAnim.TimeStart || 
				    textAnim.DidFinish) continue;
				var dt = (t - textAnim.TimeStart) / (textAnim.TimeEnd - textAnim.TimeStart);
				var c = textAnim.Curve.Evaluate(dt);
				var currentColor = textAnim.Target.color;
				currentColor.a = c * textAnim.TargetAlpha + (1 - c) * textAnim.InitialAlpha;
				textAnim.Target.color = currentColor;
				if (t > textAnim.TimeEnd) textAnim.DidFinish = true;
			}

            foreach (var cgAnim in animGroup.FadeCGAnimations)
            {
                if (t < cgAnim.TimeStart ||
                    cgAnim.DidFinish) continue;
                var dt = (t - cgAnim.TimeStart) / (cgAnim.TimeEnd - cgAnim.TimeStart);
                var c = cgAnim.Curve.Evaluate(dt);
                cgAnim.Target.alpha = c * cgAnim.TargetAlpha + (1 - c) * cgAnim.InitialAlpha;
                if (t > cgAnim.TimeEnd) cgAnim.DidFinish = true;
            }

            foreach (var rectAnim in animGroup.RectAnimations)
			{
				if (t < rectAnim.TimeStart ||
				    rectAnim.DidFinish) continue;
				var dt = (t - rectAnim.TimeStart) / (rectAnim.TimeEnd - rectAnim.TimeStart);
				var c = rectAnim.Curve.Evaluate(dt);
				var transitionPosition = c * rectAnim.TargetPosition + (1 - c) * rectAnim.InitialPosition;
				var transitionSize = c * rectAnim.TargetSize + (1 - c) * rectAnim.InitialSize;
				rectAnim.Target.anchoredPosition = transitionPosition;
				rectAnim.Target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, transitionSize.x);
				rectAnim.Target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transitionSize.y);
				if (t > rectAnim.TimeEnd) rectAnim.DidFinish = true;
			}

            foreach(var setText in animGroup.SetTextAnimations)
            {
                if (t >= setText.TimeSet && !setText.DidFinish)
                {
                    setText.Target.text = setText.TargetText;
                    setText.DidFinish = true;
                }
            }

            foreach(var setSprite in animGroup.SetSpriteAnimations)
            {
                if (t >= setSprite.TimeSet && !setSprite.DidFinish)
                {
                    setSprite.Target.sprite = setSprite.TargetSprite;
                    setSprite.DidFinish = true;
                }
            }

			if (t > animGroup.TimeEnd)
			{
				_pendingAnimations.Dequeue();
			}
			
		}
		else
		{
			_willAcceptTransitions = true;
		}
	}
}
