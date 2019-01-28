using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActuatorTester : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Calibrate");

        //Invoke("SendTestMsg", 10f);
        //Invoke("SendTestMsg2", 20f);
    }

    IEnumerator Calibrate()
    {
        while(UkiCommunicationsManager.Instance._EStopping)
        {
            yield return new WaitForSeconds(1.0f);
        }

        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)UkiTestActuatorAssignments.RightWingRaise, -55);
        yield return new WaitForSeconds(10f);
        print("done sending calibration messages");
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
