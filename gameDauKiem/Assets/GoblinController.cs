using UnityEngine;

public class GoblinController : EnemyController
{
    protected override void OnMoveStateChanged(bool isMoving)
    {
        if (anim != null) anim.SetBool("IsRunning", isMoving);
    }

    protected override void OnAttackStarted()
    {
        if (anim == null) return;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttack", true);
    }

    protected override void OnAttackCancelled()
    {
        if (anim != null) anim.SetBool("IsAttack", false);
    }

    protected override void OnAttackEnded()
    {
        if (anim != null) anim.SetBool("IsAttack", false);
    }

    protected override void OnTakeDamage()
    {
        if (anim == null) return;
        anim.SetBool("IsHint", true);
        Invoke(nameof(EndHintAnimation), 0.3f);
    }

    void EndHintAnimation()
    {
        if (anim != null) anim.SetBool("IsHint", false);
    }
}

