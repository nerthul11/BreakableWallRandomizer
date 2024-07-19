using HutongGames.PlayMaker;

namespace BreakableWallRandomizer.Fsm
{
    internal class CustomFsmBooleanCheck : FsmStateAction
    {
        private bool value;
        private string trueEvent;
        private string falseEvent;

        public CustomFsmBooleanCheck(bool _value, string _trueEvent, string _falseEvent) 
        {
            value = _value;
            trueEvent = _trueEvent;
            falseEvent = _falseEvent;
        }
        public override void OnEnter()
        {
            if (value)
            {
                Fsm.Event(trueEvent);
            }
            else
            {
                Fsm.Event(falseEvent);
            }
            Finish();
        }
    }
}