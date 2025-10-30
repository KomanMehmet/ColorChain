using _Project.Scripts.Core.Enums;
using UnityEngine;

namespace _Project.Scripts.Data
{
    [CreateAssetMenu(fileName = "BallData", menuName = "ColorChain/Data/Ball Data", order = 0)]
    public class BallData : ScriptableObject
    {
        [Header("Visual Settings")]
        [SerializeField] private BallColor ballColor;
        [SerializeField] private Color color = UnityEngine.Color.white;
        [SerializeField] private Sprite ballSprite;

        [Header("Gameplay Settings")]
        [SerializeField] private int pointValue = 10;
        [Range(0f, 1f)]
        [SerializeField] private float spawnWeight = 1f;
        
        public BallColor Color => ballColor;
        public Color BallColor => color;
        public Sprite BallSprite => ballSprite;
        public int PointValue => pointValue;
        public float SpawnWeight => spawnWeight;
        
        private void OnValidate()
        {
            if (pointValue < 0)
            {
                pointValue = 0;
                Debug.LogWarning($"[BallData] Point value cannot be negative! Set to 0.");
            }
            
            spawnWeight = Mathf.Clamp01(spawnWeight);
        }
        
        public override string ToString()
        {
            return $"BallData [{ballColor}] - Points: {pointValue}, Weight: {spawnWeight:F2}";
        }
    }
}