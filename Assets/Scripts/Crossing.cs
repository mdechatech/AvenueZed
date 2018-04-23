using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine.UI;

public class Crossing : MonoBehaviour
{
    public SpriteRenderer HorArrow;
    public SpriteRenderer VertArrow;
    public Image BackgroundImage;
    public Image DownTimerImage;
    public Sprite DownSprite;
    public Sprite UpSprite;
    public TMP_Text ShortcutText;

    [Space]
    public KeyCode Shortcut;
    public float CollideDowntime;
    public float BreachDowntime;

    public Button ToggleButton;

    public bool AllowingVertical;

    public GameObject ExplosionPrefab { get { return GameManager.Instance.ExplosionPrefab; } }

    public bool IsDown;
    private float DownTimer;

    private static readonly KeyCode[,] _shortcuts = new[,]
    {
        {KeyCode.Q, KeyCode.W, KeyCode.E},
        {KeyCode.A, KeyCode.S, KeyCode.D},
        {KeyCode.Z, KeyCode.X, KeyCode.C}
    };

    public void Initialize(CrossingData data)
    {
        GameManager.Instance.Crossings.Add(this);

        transform.position = data.Position;
        HorArrow.transform.eulerAngles = new Vector3(
            HorArrow.transform.eulerAngles.x,
            HorArrow.transform.eulerAngles.y,
            data.Row.Angle);
        
        VertArrow.transform.eulerAngles = new Vector3(
            VertArrow.transform.eulerAngles.x,
            VertArrow.transform.eulerAngles.y,
            data.Column.Angle);

        BackgroundImage.sprite = UpSprite;

        if (data.Column.Index < _shortcuts.GetLength(0) &&
            data.Row.Index < _shortcuts.GetLength(1))
        {
            Shortcut = _shortcuts[data.Row.Index, data.Column.Index];
            ShortcutText.text = Shortcut.ToString();
        }

        transform.DOScale(0, 0.5f)
            .From()
            .SetDelay(Random.Range(0.2f, 0.4f))
            .SetEase(Ease.OutCubic);

        Observable.EveryUpdate()
            .Where(_ => !IsDown && !AllowingVertical && Input.GetKeyDown(Shortcut))
            .Subscribe(_ => AllowVertical())
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => !IsDown && AllowingVertical && Input.GetKeyUp(Shortcut))
            .Subscribe(_ => DisallowVertical())
            .AddTo(this);

        DisallowVertical();
    }

    private void Awake()
    {
        ToggleButton.onClick.AddListener(() =>
        {
            if (!IsDown)
                ToggleVertical();
        });
    }

    private void Update()
    {
        if (IsDown)
        {
            if ((DownTimer -= Time.deltaTime) < 0)
            {
                Fix();
            }
            else
            {
                DownTimerImage.fillAmount = 1 - (DownTimer / CollideDowntime);
            }
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.Crossings.Remove(this);
    }

    public void Collide(bool collision = true)
    {
        DOTween.Kill(this);

        HorArrow.DOFade(0, 0.1f).SetId(this);
        VertArrow.DOFade(0, 0.1f).SetId(this);
        BackgroundImage.sprite = DownSprite;

        if (collision)
            Instantiate(ExplosionPrefab, transform.position, Quaternion.identity);

        AllowingVertical = false;
        IsDown = true;
        DownTimer = Mathf.Max(DownTimer, collision ? CollideDowntime : BreachDowntime);
    }

    public void Fix()
    {
        DOTween.Kill(this);

        DisallowVertical();
        BackgroundImage.sprite = UpSprite;
        DownTimerImage.fillAmount = 0;
        IsDown = false;
        DownTimer = 0;
    }

    public void AllowVertical()
    {
        DOTween.Kill(this);
        AllowingVertical = true;
        HorArrow.DOFade(1, 0.1f).SetId(this);
        VertArrow.DOFade(1, 0.1f).SetId(this);
    }

    public void DisallowVertical()
    {
        DOTween.Kill(this);
        AllowingVertical = false;
        HorArrow.DOFade(1, 0.1f).SetId(this);
        VertArrow.DOFade(0, 0.1f).SetId(this);
    }

    public void ToggleVertical()
    {
        if (AllowingVertical)
            DisallowVertical();
        else
            AllowVertical();
    }
}
