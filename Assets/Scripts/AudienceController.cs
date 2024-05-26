using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AudienceController : MonoBehaviour
{
    #region Constants

    private static readonly int Fidget = Animator.StringToHash("fidget");
    private static readonly int Cross = Animator.StringToHash("cross");
    private static readonly int IsHandRaised = Animator.StringToHash("is_hand_raised");

    #endregion

    #region Components

    [SerializeField] private Animator animator;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        CallRaiseHand();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #region Trigger animations

    public void CallFidget()
        {
            animator.SetTrigger(Fidget);
        }
    
    public void CallCross()
    {
        animator.SetTrigger(Cross);
    }
    
    public void CallRaiseHand()
    {
        animator.SetBool(IsHandRaised, true);
    }
    
    public void CallLowerHand(ActivateEventArgs args)
    {
        animator.SetBool(IsHandRaised, false);
    }

    #endregion
    
}