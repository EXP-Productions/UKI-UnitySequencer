using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    // Default Value for property
    private float _ActuatorArrivalRange = 40f;

    // Options will be grouped by category
    [NumberRange(0, 100)]   
    [Category("Animation")]
    public float ActuatorArrivalRange
    {
        get { return _ActuatorArrivalRange; }
        set { _ActuatorArrivalRange = value; }
    }
    
    public void PrintActuatorPositions()
    {
        UKI_PoseManager.Instance.PrintAllActuatorRanges();

    }



    /*
    private float _myRangeProperty = 0f;

    // The NumberRange attribute will ensure that the value never leaves the range 0-10
    [NumberRange(0, 10)]
    [Category("My Category")]
    public float MyRangeProperty
    {
        get { return _myRangeProperty; }
        set { _myRangeProperty = value; }
    }
    */

}
