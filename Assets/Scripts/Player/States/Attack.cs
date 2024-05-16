using UnityEngine;

public class Attack : State {

    private PlayerController controller;

    public int stage = 1;
    private float stateTime;

    public Attack(PlayerController controller) : base ("Attack") {
        this.controller = controller;
    }

    public override void Enter() {
        base.Enter();

        // ERROR: Invalid stage
        if (stage <= 0 || stage > controller.attackStages) {
            controller.stateMachine.ChangeState(controller.idleState);
            return;
        }

        //Reset variables
        stateTime = 0;

        //Set animator trigger
        controller.thisAnimator.SetTrigger("tAttack" + stage);
    }
    public override void Exit() {
        base.Exit();
    }
    public override void Update() {
        base.Update();

        // Switch to Attack (again, yes)
        if (controller.AttemptToAttack()) {
            return;
        }

        //Update StateTime
        stateTime += Time.deltaTime;

        // Exit after time
        if(IsStageExpired()) {
            controller.stateMachine.ChangeState(controller.idleState);
            return;
        }
    }
    public override void LateUpdate() {
        base.LateUpdate();
    }
    public override void FixedUpdate() {
        base.FixedUpdate();
    }

    public bool CanSwitchStages() {
        // Get attack variables
        var isLastState = stage == controller.attackStages;
        var stageDuration = controller.attackStageDurations[stage - 1];
        var stageMaxIntervals = isLastState ? 0 : controller.attackStageMaxIntervals[stage - 1];
        var maxStageDuration = stageDuration + stageMaxIntervals;

        // Reply
        return !isLastState && stateTime >= stageDuration && stateTime <= maxStageDuration;
    }

    public bool IsStageExpired() {
        // Get attack variables
        var isLastState = stage == controller.attackStages;
        var stageDuration = controller.attackStageDurations[stage - 1];
        var stageMaxIntervals = isLastState ? 0 : controller.attackStageMaxIntervals[stage - 1];
        var maxStageDuration = stageDuration + stageMaxIntervals;

        // Reply
        return stateTime > maxStageDuration;
    }

}