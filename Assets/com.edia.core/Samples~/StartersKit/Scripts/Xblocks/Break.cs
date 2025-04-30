using Edia;

namespace StartersKit {

    /// <summary>
    /// A break block is also a XBlock, as it is part of the total experiment sequence.
    /// Other examples of Xblocks could be: Questionaire, VR Practice, Mini game, etc.
    /// </summary>
    public class Break : XBlock {

        private void Awake() {
            /*
                Each trial exists out of a sequence of steps.
                In this case only a message shown to the user to take a break.
                
                As this is a XBlock, start and end are logged to disk automatically.
            */

            AddToTrialSequence(DisplayMessage); // Sett up environment / Generate Stimuli / etc
        }

#region TASK STEPS

        /// <summary>Present Cube</summary>
        void DisplayMessage() {

            Experiment.Instance.ShowMessageToUser("Take a short brake");
            Experiment.Instance.WaitOnProceed();
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