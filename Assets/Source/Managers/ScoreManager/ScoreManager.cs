using System;
using DB.Data;
using DB.DependencyInjection.Attributes;
using DB.Managers.GridManager;
using UnityEngine;

namespace DB.Managers.ScoreManager
{
    public struct NeighborhoodScoreCalculationResult
    {
        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }
        public int TotalScore { get; private set; }
        
        public NeighborhoodScoreCalculationResult(int left, int right, int top, int bottom, int totalScore)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            TotalScore = totalScore;
        }

        public override string ToString()
        {
            return "ScoreCalculationResult: {" +
                   "Left: " + Left + ", " +
                   "Right: " + Right + ", " +
                   "Top: " + Top + ", " +
                   "Bottom: " + Bottom + ", " +
                   "Total: " + TotalScore + ", " +
                   "}";
        }
    }
    
    [Injectable]
    public class ScoreManager : MonoBehaviour
    {
        public Action<int> OnScoreChange;
        
        [SerializeField] BuildingsInteractionsMatrix _buildingsInteractionsMatrix;
        private int _currentScore = 0;

        public NeighborhoodScoreCalculationResult CalculateNeighborhoodScore(BuildingDefinition building,
            Neighborhood neighborhood)
        {
            int left = neighborhood.Left is null
                ? 0
                : _buildingsInteractionsMatrix.GetValue(building, neighborhood.Left);
            int right = neighborhood.Right is null
                ? 0
                : _buildingsInteractionsMatrix.GetValue(building, neighborhood.Right);
            int top = neighborhood.Top is null 
                ? 0 
                : _buildingsInteractionsMatrix.GetValue(building, neighborhood.Top);
            int bottom = neighborhood.Bottom is null
                ? 0
                : _buildingsInteractionsMatrix.GetValue(building, neighborhood.Bottom);
            int total = building.BaseScore + left + right + top + bottom;
            
            return new NeighborhoodScoreCalculationResult(left, right, top, bottom, total);
        }
        
        public void AddPointsToScore(int points)
        {
            _currentScore += points;
            OnScoreChange?.Invoke(_currentScore);
        }
    }
}

