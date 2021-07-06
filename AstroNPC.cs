using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class AstroNPC : A_Interactive
{
    enum STANCE { STAND, SIT, CROUCH}
    [SerializeField]
    private SpriteRenderer spriteComp;
    //[SerializeField]
    //private STANCE stance = STANCE.STAND;
    [SerializeField]
    private bool startFacingRight = true;
    [SerializeField]
    private Transform animatedTextParent = null;
    [SerializeField]
    private AnimatedText.ATDetails firstInteractionText = default;
    [SerializeField]
    private List<AnimatedText.ATDetails> randomInteractionText = new List<AnimatedText.ATDetails>();

    private bool firstInteraction = true;
    private AnimatedText at;
    private Coroutine returnToDefaultDirectionCR;

    private ContactFilter2D cf;

    private void Start()
    {
        //I know kinda jank but whatever, don't have a utilities class/singleton...should probably make one
        AstroPlayer. InitContactFilterSettings(cf, gameObject);
        MoveNPCToFloor();
        FaceRight(startFacingRight);
    }

    private void MoveNPCToFloor()
    {
        //the NPC collider is really meant to be used for interactive detection with astro
        //cheap trick to make sure when moving to the ground we don't bump into things on the side from the long colldier
        CapsuleCollider2D coll = GetComponent<CapsuleCollider2D>();
        Vector2 ogDimensions = coll.size;
        CapsuleDirection2D ogOrientation = coll.direction;

        coll.size = new Vector2(1, ogDimensions.y);
        coll.direction = CapsuleDirection2D.Vertical;

        RaycastHit2D closestGroundSpot = AstroPlayer.GetClosestGroundCollision(GetComponent<Rigidbody2D>(), cf);
        float yPos = (GetComponent<CapsuleCollider2D>().size.y * transform.localScale.y / 2f) + closestGroundSpot.point.y;
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

        //restore collider original specs
        coll.size = ogDimensions;
        coll.direction = ogOrientation;
    }

    protected override void OnAstroEnter(GameObject astroGO)
    {
        if (returnToDefaultDirectionCR != null)
        {
            StopCoroutine(returnToDefaultDirectionCR);
            returnToDefaultDirectionCR = null;
        }

        Transform atParent = animatedTextParent != null ? animatedTextParent : transform;

        if (firstInteraction && firstInteractionText.ShouldAnimate())
        {
            firstInteraction = false;
            at = S_AnimatedTextBuilder.Current.StartNewTextAnimation(firstInteractionText, atParent, at);
        }
        else
        {
            AnimatedText.ATDetails randomATD = randomInteractionText[Random.Range(0, randomInteractionText.Count)];
            at = S_AnimatedTextBuilder.Current.StartNewTextAnimation(randomATD, atParent, at);
        }
    }

    private void FaceRight(bool faceRight)
    {
        if (spriteComp == null)
        {
            Debug.LogErrorFormat("SpriteRenderer component not set in the LabAstro: {0}", name);
            return;
        }

        spriteComp.flipX = !faceRight;
    }

    protected override void OnAstroExit(GameObject astroGO)
    {
        if (at != null)
        {
            at.DeanimateText();
        }
        returnToDefaultDirectionCR = StartCoroutine(ReturnToDefaultDirectionCR());
    }

    private IEnumerator ReturnToDefaultDirectionCR()
    {
        yield return new WaitForSeconds(3);
        FaceRight(startFacingRight);
        returnToDefaultDirectionCR = null;
    }

    private void FixedUpdate()
    {
        if (!AstroInArea)
        {
            return;
        }

        FaceRight(AstroGO.transform.position.x > transform.position.x);
    }

    public override void OnAstroFocus()
    {
    }

    public override void OnInteract()
    {
    }

    public override void OnReleaseInteract()
    {
    }
}
