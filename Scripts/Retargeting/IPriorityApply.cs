using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASAPToolkit.Unity.Retargeting {
	// Interface that allows us to manage multiple scripts that may have an effect
	// onto a skeleton (in Apply()) and call them based on a priority level.
	// Scripts with higher priority will be called before scripts with lower priority.
	public interface IPriorityApply {
		void Apply();
		int GetPriority();
	}
}
