using Edia;
using UnityEngine;
using UXF;

namespace StartersKit {

    public class TaskStart : XBlock {
        /*
            Task related parameters
        */

        float UserResponseTime = 0f;
        float StepStartTime = 0;

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                In order to use them, we need to add the methods of this task to the trial sequence.

                An common approach of these steps within a trial are as follows:
            */

            AddToTrialSequence(TaskStep1); // In many cases setting up the environment for the task, i.e. Generating Stimuli
            AddToTrialSequence(TaskStep2); // Input from the user
            AddToTrialSequence(TaskStep3); // Check input and log
            AddToTrialSequence(TaskStep4); // Clean up
        }

#region ------------------------------ TASK STEPS

        /*
            Methods that define the steps taken in this trial.

            Main scripts to call:
            * XRManager.Instance.xxxxx => All things XR related
            * Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

        */

        /// <summary>Present Cube</summary>
        void TaskStep1() {
            // Enable the pause button on the control panel
            Experiment.Instance.EnablePauseButton(true);

            // Disable XR interaction from the user
            XRManager.Instance.EnableRayInteraction(false);

            /*
                Continue to the next step:
                1) Set statemachine in 'wait' mode: Experiment.Instance.WaitOnProceed();

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
        void TaskStep2() {
            // Enable interaction from the user. The system will automaticly enable the Ray Interaction for the active hands set in the settings.
            XRManager.Instance.EnableRayInteraction(true);

            StepStartTime = Time.time;

            // Show message to user and allow proceeding to NextStep by pressing the button.
            Experiment.Instance.ShowMessageToUser("Click button below to continue");

            // Tell the system to wait on button press. Which will also enable the button on the controlpanel to overrule the user
            Experiment.Instance.WaitOnProceed();
        }

        /// <summary>User clicked button</summary>
        void TaskStep3() {
            UserResponseTime = Time.time - StepStartTime;

            // Add result to log
            Experiment.Instance.AddToTrialResults("UserResponseTime", UserResponseTime.ToString());
        }

        /// <summary>Clean up</summary>
        void TaskStep4() {
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