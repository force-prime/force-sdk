using TMPro;
using UnityEngine;

public class HighScoreItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text placeLabel;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text scoreLabel;

    public void Attach(int place, string name, int score)
    {
        placeLabel.text = place.ToString();
        nameLabel.text = name;
        scoreLabel.text = score.ToString();
    }
}
