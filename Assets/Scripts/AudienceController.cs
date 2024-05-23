using UnityEngine;

public class AudienceController : MonoBehaviour
{
    #region Constants

    private static readonly int Fidget = Animator.StringToHash("fidget");

    #endregion

    #region Components

    [SerializeField] private Animator animator;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        animator.SetTrigger(Fidget);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}