using System;

namespace CRMP_Auto_Calc.Models
{
    [Serializable]
    class Settings
    {
        public string gameName = "grand_theft_auto_san_andreas.dll";
        public string chatlogPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA San Andreas User Files\CR-MP\GenerationC\chatlog.txt";
        public string chatlogCopyPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\GTA San Andreas User Files\CR-MP\GenerationC\chatlog.copy.txt";

        public bool copyChatlog = true;
        public bool floodProtection = false;
        public bool usePatterns = false;
        public bool waitGame = true;
        public bool onlyPatterns = false;

        /// <summary>
        /// 0 - copy answer to clipboard
        /// 1 - send answer to chat
        /// </summary>
        public int senderType = 1;
        /// <summary>
        /// 0 - Do nothing
        /// 1 - Send answer and close chat
        /// 2 - Send answer and open chat
        /// </summary>
        public int chatOpened = 2;
        public int answerDelay = 700;
    }
}
