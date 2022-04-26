using System;

namespace CRMP_Auto_Calc.Models
{
    [Serializable]
    class Pattern
    {
        public string pattern = "";
        public bool ignoreCase = false;

        public string answer = "";
        public int sendMode = 0;
        public int answerDelay = 0;
    }
}
