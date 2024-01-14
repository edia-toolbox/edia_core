using eDIA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using UXF;
public class Break : EBlock {
    void Awake() {
	  trialSteps.Add(BreakStep1);
    }

    void BreakStep1() {

	  List<string> _messageToShow = Session.instance.CurrentBlock.settings.GetStringList("_info");

	  foreach (var s in _messageToShow) {
		this.Add2Console("Info:" + s);
	  }

	  Experiment.Instance.WaitOnProceed();
    }

    public override void OnBlockStart() {
	  base.OnBlockStart();

	  List<string> _messageToShow = Session.instance.CurrentBlock.settings.GetStringList("_start");

	  foreach (var s in _messageToShow) {
		this.Add2Console("Info:" + s);
	  }
    }

    public override void OnBlockEnd() {
	  base.OnBlockEnd();

	  List<string> _messageToShow = Session.instance.CurrentBlock.settings.GetStringList("_end");

	  foreach (var s in _messageToShow) {
		this.Add2Console("Info:" + s);
	  }
    }

}
