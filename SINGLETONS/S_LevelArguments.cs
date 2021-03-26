using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_LevelArguments : Singleton<S_LevelArguments>
{
    //TODO: make these level classes scriptable objects for singelton in scene? [SerializeField]
    //TODO: integrate a sort of default and testing section for these so we can easily test level for real
    private PrologueLevel _prologueLvl = new PrologueLevel();
    public PrologueLevel PrologueLvl { get => _prologueLvl; set => _prologueLvl = value; }
    public class PrologueLevel
    {
        public bool TROLLY_POWERED_ON = false;
        public bool TROLLY_MOVING = false;
    }
}
