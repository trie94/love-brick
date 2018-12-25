namespace Love.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SlotHelper : MonoBehaviour
    {
        public SlotBehavior[] slotBehaviors;

        void Awake()
        {
            slotBehaviors = GetComponentsInChildren<SlotBehavior>();
        }
    }
}
