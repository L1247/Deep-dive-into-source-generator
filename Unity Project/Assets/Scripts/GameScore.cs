using AutoProperty;

namespace MyNamespace
{
    [Singleton]
    public partial class GameScore
    {
        [AutoProp]
        private int score;

        public void AddScore(int score)
        {
            this.score += score;
        }
    }
}