using UnityEngine;
using HutongGames.PlayMaker;

namespace BreakableWallRandomizer.Fsm
{
    internal class SetTriggerCollider : FsmStateAction
    {
        private Collider2D thisCollider;
        public override void OnEnter()
        {
            thisCollider = Owner.GetComponent<BoxCollider2D>();
            thisCollider.isTrigger = false;
            Rigidbody2D rb = Owner.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = Owner.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            Finish();
        }
    }
}