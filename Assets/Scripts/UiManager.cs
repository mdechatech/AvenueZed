using UnityEngine;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public TMP_Text CarGoalText;
    public TMP_Text TimeGoalText;
    public TMP_Text FerriedText;
    public TMP_Text TripTimeText;
    public TMP_Text MoneyText;

    public TMP_Text LevelText;

    [Space]
    public GameObject WinScreen;
    public Button NextLevelButton;

    public GameManager Game { get { return GameManager.Instance; } }

    public void FlashMoney()
    {
        MoneyText.DOKill();
        MoneyText.color = Color.white;
        MoneyText.DOColor(Color.red, 0.5f)
            .SetEase(Ease.Flash, 20);
    }

    private void Update()
    {
        CarGoalText.text = string.Format("Ferry {0} CARS", Game.CarGoal);
        TimeGoalText.text = string.Format("Avg {0:F0}s TRIP TIME", Game.TimeGoal);
        FerriedText.text = string.Format("{0}/<size=50%>{1}</size>",
            Game.CarsFerried, Game.CarGoal);
        TripTimeText.text = string.Format("{0:F1}s/<size=50%>{1}s</size>",
            Game.AverageTripTime, Game.TimeGoal);
        MoneyText.text = string.Format("${0}", Game.Money);

        LevelText.text = string.Format("Level {0}", Game.LevelIndex + 1);
    }
}
