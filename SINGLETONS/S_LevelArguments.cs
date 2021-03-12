using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_LevelArguments : Singleton<S_LevelArguments>
{
    [SerializeField]
    private PrologueLevel _prologueLvl;
    public PrologueLevel PrologueLvl { get => _prologueLvl; set => _prologueLvl = value; }
    public class PrologueLevel
    {
        public bool TROLLY_POWERED_ON;
    }
}
