using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class S_Global : Singleton<S_Global>
{
    //TODO: make these level classes scriptable objects for singelton in scene? [SerializeField]
    //TODO: integrate a sort of default and testing section for these so we can easily test level for real
    [SerializeField]
    private PrologueLevel _prologueLvl = new PrologueLevel();
    public PrologueLevel PrologueLvl { get => _prologueLvl; set => _prologueLvl = value; }

    

    [Serializable]
    public class PrologueLevel
    {
        [SerializeField]
        private VarContainer<bool> trollyPoweredOn = new VarContainer<bool>(false);
        public bool TROLLY_POWERED_ON { get => trollyPoweredOn.CurrVar; set { trollyPoweredOn.CurrVar = value; } }

        [SerializeField]
        private VarContainer<bool> trollyMoving = new VarContainer<bool>(false);
        public bool TROLLY_MOVING { get => trollyMoving.CurrVar; set { trollyMoving.CurrVar = value; } }

        [SerializeField]
        private VarContainer<bool> mindLabPowerOn = new VarContainer<bool>(false);
        public bool MIND_LAB_POWER_ON { get => mindLabPowerOn.CurrVar; set { mindLabPowerOn.CurrVar = value; } }
        [SerializeField]
        private VarContainer<bool> mindLabMachineOn = new VarContainer<bool>(false);
        public bool MIND_LAB_MACHINE_ON { get => mindLabMachineOn.CurrVar; set { mindLabMachineOn.CurrVar = value; } }
        [SerializeField]
        private VarContainer<bool> mindLabChairActivated = new VarContainer<bool>(false);
        public bool MIND_LAB_CHAIR_ACTIVATED { get => mindLabChairActivated.CurrVar; set { mindLabChairActivated.CurrVar = value; } }

        [SerializeField]
        private VarContainer<bool> elevatorRoomUnlocked = new VarContainer<bool>(false);
        public bool ELEVATOR_ROOM_UNLOCKED { get => elevatorRoomUnlocked.CurrVar; set { elevatorRoomUnlocked.CurrVar = value; } }
        [SerializeField]
        private VarContainer<bool> elevatorRoomElevatorPoweredOn = new VarContainer<bool>(false);
        public bool ELEVATOR_ROOM_ELEVATOR_POWERED_ON { get => elevatorRoomElevatorPoweredOn.CurrVar; set { elevatorRoomElevatorPoweredOn.CurrVar = value; } }
        [SerializeField]
        private VarContainer<bool> maeLogsFound = new VarContainer<bool>(false);
        public bool MAE_LOGS_FOUND { get => maeLogsFound.CurrVar; set { maeLogsFound.CurrVar = value; } }

        [SerializeField]
        private VarContainer<bool> brainLabActivated = new VarContainer<bool>(false);
        public bool BRAIN_LAB_ACTIVATED { get => brainLabActivated.CurrVar; set { brainLabActivated.CurrVar = value; } }

        [SerializeField]
        private VarContainer<int> brainCoresActive = new VarContainer<int>(1);
        public int BRAIN_CORES_ACTIVE
        {
            get => brainCoresActive.CurrVar;
            set
            {
                brainCoresActive.CurrVar = value;
            }
        }

    }

    private static event System.Action ResetVarContainersToDefaultValues = delegate { };
    private static event System.Action EnableInspectorLevelVariablesChanged = delegate { };

    private void Awake()
    {
        S_DeveloperTools.Current.EnableInspectorLevelVariablesChanged -= S_DeveloperTools_EnableInspectorLevelVariablesChanged;
        S_DeveloperTools.Current.EnableInspectorLevelVariablesChanged += S_DeveloperTools_EnableInspectorLevelVariablesChanged;


        if (!S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL() || !S_DeveloperTools.Current.EnableInspectorLevelVariables)
        {
            ResetVarContainersToDefaultValues();
        }

        S_DeveloperTools_EnableInspectorLevelVariablesChanged();
    }

    //hack to get around trying to subscribe to the S_DeveloperTools event int the constructor of the VarContainer class
    private void S_DeveloperTools_EnableInspectorLevelVariablesChanged()
    {
        EnableInspectorLevelVariablesChanged();
    }


    [Serializable]
    public class VarContainer<T>
    {
        public VarContainer(T defaultVal)
        {
            DefaultVal = defaultVal;
            ResetVarContainersToDefaultValues -= ResetToDefaultValue;
            ResetVarContainersToDefaultValues += ResetToDefaultValue;

            EnableInspectorLevelVariablesChanged -= S_DeveloperTools_EnableInspectorLevelVariablesChanged;
            EnableInspectorLevelVariablesChanged += S_DeveloperTools_EnableInspectorLevelVariablesChanged;
        }

        [SerializeField, GetSet("InspectorVar")]
        private T inspectorVar;
        //only public here so that the GetSet field works
        public T InspectorVar
        {
            get => inspectorVar;
            set
            {
                if (DEVTOOLS_useInspectorVals)
                {
                    inspectorVar = value;
                    currVar = value;
                }
                else
                {
                    inspectorVar = currVar;
                }
            }
        }

        private T currVar;
        public T CurrVar
        {
            get => currVar;
            set
            {
                currVar = value;
                inspectorVar = value;
            }
        }
        public T DefaultVal { get; private set; }

        //default true so we can edit bools before playing (and they get reset off if dev tools options off).
        private bool DEVTOOLS_useInspectorVals = true;
        public void S_DeveloperTools_EnableInspectorLevelVariablesChanged()
        {
            DEVTOOLS_useInspectorVals = S_DeveloperTools.Current.DevToolsEnabled_TIME_TRAVEL() ? S_DeveloperTools.Current.EnableInspectorLevelVariables : false;
            if (DEVTOOLS_useInspectorVals)
            {
                currVar = inspectorVar;
            }
        }


        public void ResetToDefaultValue()
        {
            inspectorVar = DefaultVal;
            CurrVar = DefaultVal;
        }
    }
}
