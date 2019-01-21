using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActuatorTester : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {

        Invoke("SendTestMsg", 10f);
        Invoke("SendTestMsg2", 20f);
    }

 
    void SendTestMsg()
    {
        print("Sending Message 1");

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 100, ModBusRegisters.MB_MOTOR_ACCEL);

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 55, ModBusRegisters.MB_GOTO_SPEED_SETPOINT);

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 1000, ModBusRegisters.MB_GOTO_POSITION);
    }


    void SendTestMsg2()
    {
        print("Sending Message 2");

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 100, ModBusRegisters.MB_MOTOR_ACCEL);

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 55, ModBusRegisters.MB_GOTO_SPEED_SETPOINT);

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiTestActuatorAssignments.RightWingRaise, 500, ModBusRegisters.MB_GOTO_POSITION);
    }

}
