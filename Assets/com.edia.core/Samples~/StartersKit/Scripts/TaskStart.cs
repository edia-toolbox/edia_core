using Edia;
using UnityEngine;
using UXF;

namespace StartersKit {

    public class TaskStart : XBlock {

        float UserResponseTime = 0f;
        float StepStartTime = 0;

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                Each step is a standalone part of the experiment task. 
                I.e. Show a message to the user, show a fixation cross, wait for user input, etc.
                In order to use the steps, they need to be defined in the trial sequence.

                A common approach within a trial is as follows:
            */

            AddToTrialSequence(PrepareStimuli); // Sett up environment / Generate Stimuli / etc
            AddToTrialSequence(WaitOnUserInput); // Input from the user
            AddToTrialSequence(ValidateUserInput); // Check input and log results
            AddToTrialSequence(CleanUpScene); // Clean up
        }

#region TASK STEPS

        /*
            The framework provides a set of tools to make it easier to implement the task.
            For example:
            * XRManager.Instance.xxxxx => All things XR related
            * Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

            See documentation for more details.
        */

        /// <summary>Present Cube</summary>
        void PrepareStimuli() {
            // Enable the pause button on the control panel in case the participant wants to pause the experiment.
            Experiment.Instance.EnablePauseButton(true);

            // Disable XR interaction from the user
            XRManager.Instance.EnableAllInteraction(false);

            /*
                Continue to the next step:
                Set statemachine in 'wait' mode: Experiment.Instance.WaitOnProceed();
            */

            Experiment.Instance.WaitOnProceed();

            /*
            Then, either:
                * Directly: Experiment.Instance.Proceed();
                * Delayed: Experiment.Instance.ProceedWithDelay (seconds as float)
            */

            Experiment.Instance.ProceedWithDelay(Session.instance.CurrentBlock.settings.GetFloat("timer_showcube"));
        }

        /// <summary>Move cube, wait on user input</summary>
        void WaitOnUserInput() {
            // Enable interaction from the user. The system will automaticly enable the Ray Interaction for the active hands set in the settings.
            XRManager.Instance.EnableRayInteraction(true);

            StepStartTime = Time.time;

            // Show message to user and allow proceeding to NextStep by pressing the button.
            Experiment.Instance.ShowMessageToUser("Click button below to continue");

            // Tell the system to wait on button press. Which will also enable the button on the controlpanel to overrule the user
            Experiment.Instance.WaitOnProceed();
        }

        /// <summary>User clicked button</summary>
        void ValidateUserInput() {
            UserResponseTime = Time.time - StepStartTime;

            // Add result to log
            Experiment.Instance.AddToTrialResults("UserResponseTime", UserResponseTime.ToString());
        }

        /// <summary>Clean up</summary>
        void CleanUpScene() {
            XRManager.Instance.EnableRayInteraction(false);
        }

#endregion
#region ------------------------------ STATEMACHINE OVERRIDES

        /*
            Opional overrides
        */

        public override void OnBlockStart() {
        }

        public override void OnStartTrial() {
        }

        public override void OnEndTrial() {
        }

        public override void OnBetweenSteps() {
        }

        public override void OnBlockOutro() {
        }

        public override void OnBlockEnd() {
        }

#endregion
    }

}