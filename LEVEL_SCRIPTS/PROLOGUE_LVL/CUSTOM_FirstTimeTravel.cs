using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Image))]
public class CUSTOM_FirstTimeTravel : MonoBehaviour
{
    [SerializeField]
    private AstroCamera astroCam;
    [SerializeField]
    private float timeTillBlack;
    [SerializeField]
    private float timeSpentInBlack;
    private Animator animator;
    private Image image;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
        image = GetComponent<Image>();
        image.enabled = true;

        SO_RA_FirstTimeTravel.OnFirstTimeTravel -= SO_RA_FirstTimeTravel_OnFirstTimeTravel;
        SO_RA_FirstTimeTravel.OnFirstTimeTravel += SO_RA_FirstTimeTravel_OnFirstTimeTravel;
    }

    private void SO_RA_FirstTimeTravel_OnFirstTimeTravel()
    {
        S_AstroInputManager.Current.ControlsEnabled = false;
        //Hack to start playing it
        animator.enabled = true;
        StartCoroutine(delayTimeTravel(timeTillBlack + timeSpentInBlack));
        StartCoroutine(delayEndOfEffect(timeTillBlack + astroCam.TTFadeOut + timeSpentInBlack));
    }

    private IEnumerator delayTimeTravel(float delay)
    {
        yield return new WaitForSeconds(delay);
        S_TimeTravel.Current.InitTimeTravel(S_TimeTravel.TIME_PERIOD.PAST);
    }

    private IEnumerator delayEndOfEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        image.enabled = false;
    }
}
