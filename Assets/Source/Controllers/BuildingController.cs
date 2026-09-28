using DB.Data;
using DB.Managers.GridManager;
using DB.Managers.ScoreManager;
using TMPro;
using UnityEngine;

namespace DB.Controllers
{
    public class BuildingController : MonoBehaviour
    {
        [SerializeField] private TextMeshPro leftScoreText;
        [SerializeField] private TextMeshPro rightScoreText;
        [SerializeField] private TextMeshPro topScoreText;
        [SerializeField] private TextMeshPro bottomScoreText;
        [SerializeField] private TextMeshPro finalScoreText;

        public void UpdateNeighbourhoodScore(NeighborhoodScoreCalculationResult scoreCalculationResult)
        {
            UpdateScore(scoreCalculationResult.Left, leftScoreText);
            UpdateScore(scoreCalculationResult.Right, rightScoreText);
            UpdateScore(scoreCalculationResult.Top, topScoreText);
            UpdateScore(scoreCalculationResult.Bottom, bottomScoreText);
            finalScoreText.text = scoreCalculationResult.TotalScore.ToString();
            finalScoreText.enabled = true;
        }

        public void ClearDisplayedScores()
        {
            UpdateScore(0, leftScoreText);
            UpdateScore(0, rightScoreText);
            UpdateScore(0, topScoreText);
            UpdateScore(0, bottomScoreText);
            finalScoreText.text = "";
            finalScoreText.enabled = false;
        }

        private void UpdateScore(int score, TextMeshPro text)
        {
            if (score == 0)
            {
                text.text = "";
                text.enabled = false;
            }
            else
            {
                text.text = score.ToString();
                text.enabled = true;
            }
        }
    }
}

