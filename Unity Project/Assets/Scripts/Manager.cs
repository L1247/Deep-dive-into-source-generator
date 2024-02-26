#region

using MyNamespace;
using UnityEngine;

#endregion

namespace DefulatNameSpace
{
    public class Manager : MonoBehaviour
    {
    #region Unity events

        private void Awake()
        {
            GameScore.Instance.AddScore(10);
            Debug.Log($"{GameScore.Instance.Score}");
            // NewBehaviourScript.Instance.Test();
        }

    #endregion
    }
}