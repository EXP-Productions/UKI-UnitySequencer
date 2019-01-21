using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class UkiStateDB
{
    public static Dictionary<int, Dictionary<int, int>> _StateDB = new Dictionary<int, Dictionary<int, int>>();

    public static void RegisterActuator(Actuator actuator)
    {
        if (!_StateDB.ContainsKey((int)actuator._ActuatorIndex))
        {
            Dictionary<int, int> registerDict = new Dictionary<int, int>();

            foreach (ModBusRegisters register in (ModBusRegisters[])Enum.GetValues(typeof(ModBusRegisters)))
            {
                registerDict.Add((int)register, 0);
            }
            _StateDB.Add((int)actuator._ActuatorIndex, registerDict);
        }
        else
        {
            UnityEngine.Debug.LogError("Registering the same actuator twice, are you sure you've assigned scene actuators correctly");
        }
    }

}

