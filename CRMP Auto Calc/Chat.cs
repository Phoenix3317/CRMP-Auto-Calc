﻿using CRMP_Auto_Calc.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;
using System.Threading;
using System.Collections.Generic;

namespace CRMP_Auto_Calc
{
    class Chat : IDisposable
    {
        public bool ChatOpened { get; private set; }
        public bool IsWork { get; private set; }
        public bool FloodProtection { get; set; }

        public delegate void NewMessage(ChatLine line);
        public event NewMessage OnNewMessage;

        public delegate void KeyPressed(KeyEventArgs e);
        public event KeyPressed OnKeyPressed;

        public delegate void ChatStateChanged(bool isOpen);
        public event ChatStateChanged OnChatStateChanged;

        private readonly string chatlog;
        private DateTime lastMsgTime;
        private Task globalHookTask;

        public Chat(string chatlog)
        {
            if (!File.Exists(chatlog)) new FileNotFoundException("chatlog не найден");
            this.chatlog = chatlog;
            this.IsWork = false;
            this.FloodProtection = false;

            globalHookTask = new Task(() =>
            {
                IKeyboardEvents hook = Hook.GlobalEvents();
                hook.KeyUp += Hook_KeyUp;
                Application.Run();
            });

            globalHookTask.Start();
        }

        private void Hook_KeyUp(object sender, KeyEventArgs e)
        {
            if (!IsWork) return;

            if (e.KeyCode == Keys.F6) SetChatState(!ChatOpened);
            else if (e.KeyCode == Keys.R && e.Control) SetChatState(false);
            else if (ChatOpened && e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                SetChatState(false);
                if (e.KeyCode == Keys.Enter) lastMsgTime = DateTime.Now;
            }
            else OnKeyPressed(e);
        }

        public void SetChatState(bool isOpen)
        {
            ChatOpened = isOpen;
            OnChatStateChanged(ChatOpened);
        }

        public void SendMsg(string msg, int senderMode)
        {
            if(FloodProtection)
            {
                TimeSpan t = DateTime.Now - lastMsgTime;
                if (t.TotalSeconds < 1.5) Thread.Sleep(1000);
            }

            if (ChatOpened)
            {
                switch (senderMode)
                {
                    case 0: return;
                    case 1: Send($"{{HOME}}+({{END}}){msg}{{ENTER}}"); break;
                    case 2: Send($"{{HOME}}+({{END}}){msg}{{ENTER}}{{F6}}"); break;
                }
            }
            else Send($"{{F6}}{{HOME}}+({{END}}){msg}{{ENTER}}");
        }

        public void Send(string keys)
        {
            lastMsgTime = DateTime.Now;
            SendKeys.SendWait(keys);
        }

        public void Send(List<string> keys, int sleep = 0) => keys.ForEach(key => {
            Send(key);
            Thread.Sleep(sleep);
        });

        public void Start()
        {
            IsWork = true;
            ChatOpened = false;
            using (StreamReader reader = new StreamReader(File.Open(chatlog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding(1251)))
            {
                reader.BaseStream.Position = reader.BaseStream.Length;
                while (IsWork)
                {
                    string line = reader.ReadLine();
                    if (line == null || line == "") continue;

                    OnNewMessage(new ChatLine(line));
                }
            }
        }

        public void StartAsync() => new Task(() => Start()).Start();

        public void Stop() => IsWork = false;

        public void Dispose()
        {
            IsWork = false;
            Application.Exit();
            ((IDisposable)globalHookTask).Dispose();
        }
    }
}
