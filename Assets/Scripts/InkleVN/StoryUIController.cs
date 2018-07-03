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
	
	// Animation for Text component (fade in/out)
	private class TextAnimation
	{
		public float TimeStart;
		public float TimeEnd;

		public AnimationCurve Curve;
		public Text Target;

		// Opacity values for text (since we're only doing that)
		public float InitialAlpha;
		public float TargetAlpha;

		public string TargetText;

		public bool DidFinish;

		// Convenience constructor
		public TextAnimation(Text target, float timeStart, float duration, AnimationCurve curve,
			float targetAlpha, string targetText = null)
		{
			Target = target;
			TimeStart = timeStart;
			TimeEnd = timeStart + duration;
			Curve = curve;
			//InitialAlpha = target.color.a;
			// hack: initial alpha is always 1-targetAlpha
			InitialAlpha = 1.0f - targetAlpha;
			TargetAlpha = targetAlpha;
			TargetText = targetText;
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
		public List<TextAnimation> TextAnimations;

		public AnimGroup()
		{
			TimeEnd = TimeStart = Time.time;
			RectAnimations = new List<RectAnimation>();
			TextAnimations = new List<TextAnimation>();
		}

		public AnimGroup AddAnimation(RectAnimation ra)
		{
			TimeStart = Mathf.Min(TimeStart, ra.TimeStart);
			TimeEnd = Mathf.Max(TimeEnd, ra.TimeEnd);
			RectAnimations.Add(ra);
			return this;
		}

		public AnimGroup AddAnimation(TextAnimation ta)
		{
			TimeStart = Mathf.Min(TimeStart, ta.TimeStart);
			TimeEnd = Mathf.Max(TimeEnd, ta.TimeEnd);
			TextAnimations.Add(ta);
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
	
	/**
	 * Transition scene to another state
	 */
	public bool Transition(SceneTransitionRequest str)
	{
		if (!_willAcceptTransitions) return false;
		// Empty speaker - transition to description phrase or background
		if (str.TransitionSpeaker == null)
		{
			if (str.TransitionBackground != null)
			{
				// TODO: animate background transition
				Background.sprite = _backgroundPool.GetBackgroundSprite(str.TransitionBackground);
			}
			else
			{
				_pendingAnimations.Enqueue(new AnimGroup()
					.AddAnimation(new TextAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
					.AddAnimation(new RectAnimation(PhraseBackground.rectTransform, Time.time + FadeOutDuration, TransitionDuration, TransitionCurve, DescriptionBackgroundAnchor.rectTransform))
					.AddAnimation(new TextAnimation(PhraseText, Time.time + FadeOutDuration + TransitionDuration, FadeInDuration, FadeInCurve, 1.0f, str.TransitionPhrase))
				);
			}
		}
		else
		{
			ActorImage.sprite = _actorPool.GetActorSprite(str.TransitionSpeaker, str.TransitionSpeakerEmotion);
			if (str.TransitionChoices == null)
			{
				if (str.TransitionSpeaker.Contains("Player"))
				{
					
					_pendingAnimations.Enqueue(new AnimGroup()
						.AddAnimation(new TextAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
						.AddAnimation(new RectAnimation(PhraseBackground.rectTransform, Time.time + FadeOutDuration, TransitionDuration, TransitionCurve, PlayerPhraseBackgroundAnchor.rectTransform))
						.AddAnimation(new TextAnimation(PhraseText, Time.time + FadeOutDuration + TransitionDuration, FadeInDuration, FadeInCurve, 1.0f, str.TransitionPhrase))
					);	
				}
				else
				{
					_pendingAnimations.Enqueue(new AnimGroup()
						.AddAnimation(new TextAnimation(PhraseText, Time.time, FadeOutDuration, FadeOutCurve, 0.0f))
						.AddAnimation(new RectAnimation(PhraseBackground.rectTransform, Time.time + FadeOutDuration, TransitionDuration, TransitionCurve, NPCPhraseBackgroundAnchor.rectTransform))
						.AddAnimation(new TextAnimation(PhraseText, Time.time + FadeOutDuration + TransitionDuration, FadeInDuration, FadeInCurve, 1.0f, str.TransitionPhrase))
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
			foreach (var textAnim in animGroup.TextAnimations)
			{
				if (t < textAnim.TimeStart || 
				    textAnim.DidFinish) continue;
				if (textAnim.TargetText != null)
				{
					textAnim.Target.text = textAnim.TargetText;
				}
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
