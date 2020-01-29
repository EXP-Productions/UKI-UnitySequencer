using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UKIEnums
{
    public enum State
    {
        Paused,
        Calibrating,
        CalibratedToZero,
        Animating,
        NoiseMovement,
    }
}

public enum UkiLegs
{
    Undefined,
    RightRear,
    RightMid,
    RightFront,
    LeftRear,
    LeftMid,
    LeftFront
}

public enum UkiWings
{
    LeftWing,
    RightWing,
}


public enum UkiTestActuatorAssignments
{
    Undefined = -1,
    Global = 0,
    RightWingRaise = 6,
    TestBox = 24,
    TestBox2 = 31
}


public enum UkiActuatorAssignments
{
    Undefined = -1,
    Global = 0,
    RightRearKnee = 5,
    RightMidHip = 7,
    RightMidKnee = 8,
    RightMidAnkle = 9,
    RightFrontHip = 10,
    RightFrontKnee = 11,
    RightFrontAnkle = 12,
    LeftRearHip = 13,
    LeftRearKnee = 14,
    LeftRearAnkle = 15,
    LeftMidHip = 16,
    LeftMidKnee = 17,
    LeftMidAnkle = 18,
    LeftFrontHip = 19,
    LeftFrontKnee = 20,
    LeftFrontAnkle = 21,
    LeftWingRotate = 22,
    LeftWingRaise = 23,
    RightWingRotate = 25,
    RightWingRaise = 26,
    Arse = 27,
    Pincer1 = 28,
    Pincer2 = 29,
    RightRearAnkle = 30,
    RightRearHip = 31,
}
