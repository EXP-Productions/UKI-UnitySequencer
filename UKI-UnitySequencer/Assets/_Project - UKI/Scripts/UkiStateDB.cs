using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class UkiStateDB
{
    public static Dictionary<UkiActuatorAssignments, Dictionary<ModBusRegisters, int>> _StateDB = new Dictionary<UkiActuatorAssignments, Dictionary<ModBusRegisters, int>>();

    public static void RegisterActuator(Actuator actuator)
    {
        if (!_StateDB.ContainsKey(actuator._ActuatorIndex))
        {
            Dictionary<ModBusRegisters, int> registerDict = new Dictionary<ModBusRegisters, int>();

            foreach (ModBusRegisters register in (ModBusRegisters[])Enum.GetValues(typeof(ModBusRegisters)))
            {
                registerDict.Add(register, 0);
            }
            _StateDB.Add(actuator._ActuatorIndex, registerDict);
        }
        else
        {
            UnityEngine.Debug.LogError("Registering the same actuator twice, are you sure you've assigned scene actuators correctly");
        }
    }

}

