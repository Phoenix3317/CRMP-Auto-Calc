using CRMP_Auto_Calc.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Threading;

namespace CRMP_Auto_Calc
{
    class Chat : IDisposable
    {
        public bool chatOpened { get; private set; }
        public bool isWork { get; private set; }
        public bool floodProtection { get; set; }

        public delegate void NewMessage(ChatLine line);
        public event NewMessage OnNewMessage;

        public delegate void ChatStateChanged(bool isOpen);
        public event ChatStateChanged OnChatStateChanged;

        private string chatlog;
        private DateTime lastMsgTime;
        private Task globalHookTask;

        public Chat(string chatlog)
        {
            if (!File.Exists(chatlog)) new FileNotFoundException("chatlog не найден");
            this.chatlog = chatlog;
            this.isWork = false;
            this.floodProtection = false;

            globalHookTask = new Task(() =>
            {
                IKeyboardEvents hook = Hook.GlobalEvents();
                hook.KeyUp += hook_KeyUp;
                Application.Run();
                MessageBox.Show("HW");
            });

            globalHookTask.Start();
        }

        private void hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isWork) return;

            if (e.KeyCode == Keys.F6)
            {
                chatOpened = !chatOpened;
                OnChatStateChanged(chatOpened);
            }
            else if (chatOpened && e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                chatOpened = false;
                OnChatStateChanged(chatOpened);
                if (e.KeyCode == Keys.Enter) lastMsgTime = DateTime.Now;
            }
        }

        public void SendMsg(string msg, int senderMode)
        {
            if(floodProtection)
            {
                TimeSpan t = DateTime.Now - lastMsgTime;
                if (t.TotalSeconds < 1.5) Thread.Sleep(1000);
            }

            if (chatOpened)
            {
                switch (senderMode)
                {
                    case 0: return;
                    case 1: Send($"^(a) {msg} {{ENTER}}"); break;
                    case 2: Send($"^(a) {msg} {{ENTER}} {{F6}}"); break;
                }
            }
            else Send($"{{F6}} ^(a) {msg} {{ENTER}}");
        }

        public void Send(string keys)
        {
            lastMsgTime = DateTime.Now;
            SendKeys.SendWait(keys);
        }

        public void Start()
        {
            isWork = true;
            chatOpened = false;
            using (StreamReader reader = new StreamReader(File.Open(chatlog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding(1251)))
            {
                reader.BaseStream.Position = reader.BaseStream.Length;
                while(isWork)
                {
                    string line = reader.ReadLine();
                    if (line == null || line == "") continue;

                    OnNewMessage(new ChatLine(line));
                }
            }
        }

        public void StartAsync() => new Task(() => Start()).Start();

        public void Stop() => isWork = false;

        public void Dispose()
        {
            isWork = false;
            Application.Exit();
            ((IDisposable)globalHookTask).Dispose();
        }
    }
}
