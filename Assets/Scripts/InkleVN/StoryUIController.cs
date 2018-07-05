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

    public delegate void OnChoice(int choiceIndex);

    private OnChoice _choiceHandler;

    private Text _shadowText;

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

        public AnimGroup()
        {
            TimeEnd = TimeStart = Time.time;
            RectAnimations = new List<RectAnimation>();
            FadeAnimations = new List<FadeAnimation>();
            SetTextAnimations = new List<SetTextAnimation>();
            SetSpriteAnimations = new List<SetSpriteAnimation>();
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
    }

    private Queue<AnimGroup> _pendingAnimations;

    void Awake()
    {
        _willAcceptTransitions = true;
        _actorPool = new ActorPool(false);
        _backgroundPool = new BackgroundPool(false);
        _pendingAnimations = new Queue<AnimGroup>();
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
        return LayoutUtility.GetPreferredHeight(_shadowText.rectTransform);
    }

    private void TransitionBackground(SceneTransitionRequest str)
    {
        var animGroup = new AnimGroup();
        BackgroundTransition.sprite = _backgroundPool.GetBackgroundSprite(str.TransitionBackground);
        animGroup.AddAnimation(new FadeAnimation(BackgroundTransition, Time.time, FadeInDuration, FadeInCurve, 1.0f))
            .AddAnimation(new SetSpriteAnimation(Background, Time.time + FadeInDuration, BackgroundTransition.sprite))
            .AddAnimation(new FadeAnimation(BackgroundTransition, Time.time + FadeInDuration, 0.001f, FadeOutCurve, 0.0f));
        _pendingAnimations.Enqueue(animGroup);
    }

    private void TransitionToDescription(SceneTransitionRequest str)
    {
        var animGroup = new AnimGroup();
        var textFadeOut = new FadeAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f);
        animGroup.AddAnimation(textFadeOut);

        var lastAnimFinish = textFadeOut.TimeEnd;

        var defaultTextBoxSize = new Vector2(DescriptionBackgroundAnchor.rectTransform.rect.width - 30,
                                            DescriptionBackgroundAnchor.rectTransform.rect.height - 30);

        var requiredHeight = GetDesiredTextHeight(PhraseText, str.TransitionPhrase, defaultTextBoxSize);

        if (PhraseBackground.rectTransform.anchoredPosition != DescriptionBackgroundAnchor.rectTransform.anchoredPosition ||
            requiredHeight != PhraseText.rectTransform.rect.height)
        {
            var textBoxResize = new RectAnimation(PhraseBackground.rectTransform, textFadeOut.TimeEnd, TransitionDuration, TransitionCurve, DescriptionBackgroundAnchor.rectTransform);
            // Fixup for correct size
            textBoxResize.TargetSize.y = requiredHeight + 30;
            animGroup.AddAnimation(textBoxResize);
            lastAnimFinish = textBoxResize.TimeEnd;
        }

        var textChange = new SetTextAnimation(PhraseText, lastAnimFinish, str.TransitionPhrase);
        animGroup.AddAnimation(textChange);
        var textFadeIn = new FadeAnimation(PhraseText, lastAnimFinish, FadeInDuration, FadeInCurve, 1.0f);
        animGroup.AddAnimation(textFadeIn);
        _pendingAnimations.Enqueue(animGroup);
    }


    
	
	/**
	 * Transition scene to another state
	 */
	public bool Transition(SceneTransitionRequest str)
	{
		if (!_willAcceptTransitions) return false;
        if (str.TransitionBackground != null)
        {
            TransitionBackground(str);
        }
        if (str.TransitionSpeaker == null)
		{
            TransitionToDescription(str);
		}
		else
		{
			ActorImage.sprite = _actorPool.GetActorSprite(str.TransitionSpeaker, str.TransitionSpeakerEmotion);
			if (str.TransitionChoices == null)
			{
				if (str.TransitionSpeaker.Contains("Player"))
				{
					
					_pendingAnimations.Enqueue(new AnimGroup()
						.AddAnimation(new FadeAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
						.AddAnimation(new RectAnimation(PhraseBackground.rectTransform, Time.time + FadeOutDuration, TransitionDuration, TransitionCurve, PlayerPhraseBackgroundAnchor.rectTransform))
						.AddAnimation(new FadeAnimation(PhraseText, Time.time + FadeOutDuration + TransitionDuration, FadeInDuration, FadeInCurve, 1.0f))
					);	
				}
				else
				{
					_pendingAnimations.Enqueue(new AnimGroup()
						.AddAnimation(new FadeAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
						.AddAnimation(new RectAnimation(PhraseBackground.rectTransform, Time.time + FadeOutDuration, TransitionDuration, TransitionCurve, NPCPhraseBackgroundAnchor.rectTransform))
						.AddAnimation(new FadeAnimation(PhraseText, Time.time + FadeOutDuration + TransitionDuration, FadeInDuration, FadeInCurve, 1.0f))
					);
				}
			}
		}

		return true;
	}

	private void GetPhraseSize(string text)
	{
		
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
