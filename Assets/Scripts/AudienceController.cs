using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = UnityEngine.Random;

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

    #region Unity

    private void Start()
    {
        // Start non-verbal movements.
        StartCoroutine(CallFidgetOrCrossRandomly());
    }

    #endregion


    #region Trigger animations

    private IEnumerator CallFidgetOrCrossRandomly()
    {
        while (true)
        {
            // Randomly wait between 10 and 30 seconds.
            yield return new WaitForSeconds(Random.Range(10, 31));

            // Skip if hand is raised.
            if (animator.GetBool(IsHandRaised))
            {
                continue;
            }

            // Randomly choose between fidget and cross.
            if (Random.value < 0.5f)
            {
                CallFidget();
            }
            else
            {
                CallCross();
            }
        }
    }

    private void CallFidget()
    {
        animator.SetTrigger(Fidget);
    }

    private void CallCross()
    {
        animator.SetTrigger(Cross);
    }

    public void CallRaiseHand()
    {
        animator.SetBool(IsHandRaised, true);
    }

    public void CallLowerHand()
    {
        animator.SetBool(IsHandRaised, false);
    }

    #endregion
}