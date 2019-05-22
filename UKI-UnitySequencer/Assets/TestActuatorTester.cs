using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActuatorTester : MonoBehaviour
{
    bool calibrated = false;
    bool sendMsg1 = false;
    public UkiActuatorAssignments _Actuator;

    private void Update()
    {
        if(calibrated)
        {
            if(!sendMsg1)
            {
                SendTestMsg2();
                sendMsg1 = true;
                Invoke("SendTestMsg", 10f);
            }
        }
    }


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
            print("waiting for estop");
            yield return new WaitForSeconds(1.0f);
        }
        print(" sending calibration messages");
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)UkiActuatorAssignments.LeftWingRaise, -30);
        UkiCommunicationsManager.Instance.SendCalibrationMessage((int)UkiActuatorAssignments.LeftMidAnkle, -30);
        yield return new WaitForSeconds(20f);
        print("done sending calibration messages");
        calibrated = true;
    }

    void SendTestMsg()
    {
        print("Sending Message 1");
        
        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiActuatorAssignments.LeftWingRaise, 30, ModBusRegisters.MB_GOTO_SPEED_SETPOINT);
        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiActuatorAssignments.LeftWingRaise, 300, ModBusRegisters.MB_GOTO_POSITION);
    }


    void SendTestMsg2()
    {
        print("Sending Message 2");

        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiActuatorAssignments.LeftMidAnkle, 30, ModBusRegisters.MB_GOTO_SPEED_SETPOINT);
        UkiCommunicationsManager.Instance.SendActuatorMessage((int)UkiActuatorAssignments.LeftMidAnkle, 200, ModBusRegisters.MB_GOTO_POSITION);
    }

}
