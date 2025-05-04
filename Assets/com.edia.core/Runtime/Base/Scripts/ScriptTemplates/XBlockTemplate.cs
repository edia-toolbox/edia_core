using Edia;
using UnityEngine;

public class XBlockTemplate : XBlock {

    /*
     * Xblock order of execution:
     * 1. OnBlockStart
     * 2. OnTrialStart
     * 3. first step method
     * 4. InBetweenSteps
     * 5. next step method
     * 6. ...
     * 7. OnTrialEnd
     * 8. OnBlockEnd
     * 9. OnBlockOutro
     */

    public class TaskStart : XBlock {

        private int _selectedStimulus = 0;

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                Each step is a standalone part of the experiment task.
                I.e. Show a message to the user, show a fixation cross, wait for user input, etc.
                In order to use the steps, they need to be defined in the trial sequence.

                A common approach within a trial is as follows:
            */

            AddToTrialSequence(SetupAndShowStimuli); //      #1 Set up environment / Generate Stimuli / etc
            AddToTrialSequence(UserInput); //     #2 Input from the user
            AddToTrialSequence(ValidateAndLogUserInput); //   #3 Check input and log results
        }

#region TASK STEPS

        /*
            The framework provides a set of tools to make it easier to implement the task.
            For example:
            * XRManager.Instance.xxxxx => All things XR related
            * Experiment.instance.xxxxx => All things related to the progress of the experiment, logging data, etc

            See documentation for more details.
        */

        // -------------------------------------------------------------------------------------------
        // #1
        /// <summary>Prepare and show stimuli</summary>
        void SetupAndShowStimuli() {
            /*
                Some examples:
                - Choose a random stimulus from a list of stimuli.
                - Generate a random number between 0 and 100.
                - Choose a color and assign it to the stimuli materials.
                - ...
            */

            /*
             * A step always ends with either a 'PROCEED' or 'WAITONPROCEED'
             */
        }

        // -------------------------------------------------------------------------------------------
        // #2
        /// <summary>Wait on user input</summary>
        void UserInput() {
            /*
             * Enable user interaction.
             * There are two available ways of interacting:
             *   1. Pointing a ray. This works for near and far interactions. ( XRManager.Instance.EnableRayInteraction )
             *   2. Poke / touch. This works for near interaction. ( XRManager.Instance.EnablePokeInteraction() )
             * Both work with controllers and hands.
             * See Edia documentation for more details.
             */

            XRManager.Instance.EnableRayInteraction(true); // This enabled a XR ray to interact FAR and NEAR, including grabbing

            // Tell the system to wait on button press. This will also enable the 'proceed' button on the experimenter controllerpanel 
            Experiment.Instance.WaitOnProceed();
        }

        // Call from button
        public void UserInputDone(int stimulusIndex) {
            _selectedStimulus = stimulusIndex;

            /*
             * !! If the experimenter uses the 'proceed' button on the controllerpanel, this method is skipped.
             * Therefore be very aware of where to put essential parts of code.
             */

            // As we already have put the system in 'wait' mode in the previous step, we can proceed.
            Experiment.Instance.Proceed();
        }

        // -------------------------------------------------------------------------------------------
        // #3
        /// <summary>Validate and log data</summary>
        void ValidateAndLogUserInput() {
            // Add results and relevant data to trial results
            Experiment.Instance.AddToTrialResults("selection", _selectedStimulus.ToString());

            /*
             * Continue to next step
             * The message panel has a 'proceed' button by default.
             */
            Experiment.Instance.ShowMessageToUser("Press 'proceed' to continue.");
            Experiment.Instance.WaitOnProceed();
        }

#endregion
#region STATEMACHINE OVERRIDES

        public override void OnBlockStart() {
            /*
             * Shows message to the user if there is a value defined in the configs with the key "_intro"
             */
        }

        public override void OnStartTrial() {
        }

        public override void OnEndTrial() {
        }

        public override void OnBetweenSteps() {
        }

        public override void OnBlockEnd() {
        }

        public override void OnBlockOutro() {
            /*
             * Shows message to the user if there is a value defined in the configs with the key "_outro"
             */
        }

#endregion
    }
}