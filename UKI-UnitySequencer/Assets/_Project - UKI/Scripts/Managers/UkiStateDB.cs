using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Holds the states for all the actuiators
// Gets updated in the UKI comms manager and allows us to query the state of each of the actuators
public static class UkiStateDB
{
    public static Dictionary<UkiActuatorAssignments, Dictionary<ModBusRegisters, int>> _StateDB = new Dictionary<UkiActuatorAssignments, Dictionary<ModBusRegisters, int>>();
    
    public static void RegisterActuator(UkiActuatorAssignments actuator)
    {
        if (!_StateDB.ContainsKey(actuator))
        {
            Dictionary<ModBusRegisters, int> registerDict = new Dictionary<ModBusRegisters, int>();

            foreach (ModBusRegisters register in (ModBusRegisters[])Enum.GetValues(typeof(ModBusRegisters)))
            {
                registerDict.Add(register, 0);
            }
            _StateDB.Add(actuator, registerDict);
        }
        else
        {
            UnityEngine.Debug.LogError(actuator.ToString() +   "   ERROR: Registering the same actuator twice, are you sure you've assigned scene actuators correctly");
        }
    }

}

