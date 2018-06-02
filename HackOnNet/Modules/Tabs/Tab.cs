using Hacknet;
using Hacknet.Gui;
using HackOnNet.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules.Tabs
{
    class Tab
    {
        public string target = "localhost";

        public bool isBottom = true;

        public bool IsBottom => isBottom;


        public System.Collections.Generic.List<string> history;

        public System.Collections.Generic.List<string> runCommands = new System.Collections.Generic.List<string>();

        public int commandHistoryOffset;

        public string currentLine;

        public string lastRunCommand;

        public string prompt;
        public string user;

        public OnNetTerminal parent;

        int FrameCounter = 0;

        bool WasActivated = false;
        int cursorPosition = 0;
        int drawOffsetPosition = 0;
        Keys lastHeldKey;
        float keyRepeatDelay = 0.44f;


        public Tab(OnNetTerminal parent, string target, string username)
        {
            this.target = target;
            this.parent = parent;
            this.history = new System.Collections.Generic.List<string>(512);
            this.runCommands = new System.Collections.Generic.List<string>(512);
            this.commandHistoryOffset = 0;
            this.currentLine = "";
            this.lastRunCommand = "";
            this.user = username;
            this.prompt = "{{blue}}" + user + "{{white}}$ ";
            Hacknet.GuiData.getFilteredKeys();
        }

        public void doGui(Rectangle bounds, bool active)
        {
            SpriteFont tinyfont = GuiData.tinyfont;
            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            int num = -4;
            int num2 = (int)((float)(bounds.Y + bounds.Height - 16) - tinyFontCharHeight - (float)num);
            int i = (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X;
            if (bounds.Width > 0)
            {
                while (i >= (int)((double)bounds.Width * 0.7))
                {
                    this.prompt = this.prompt.Substring(1);
                    i = (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X;
                }
            }
            parent.spriteBatch.DrawFormatString(this.prompt, new Vector2((float)(bounds.X + 3), (float)num2));
            if (Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK())
            {
                num -= 4;
            }
            //num2 += num;

            Hacknet.Gui.TextBox.LINE_HEIGHT = (int)(tinyFontCharHeight + 15f);
            Hacknet.GuiData.spriteBatch.DrawString(GuiData.tinyfont, this.currentLine, Utils.ClipVec2ForTextRendering(new Vector2((float)
                bounds.X + 3 + (int)Terminal.PROMPT_OFFSET + (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X
                , (float)num2)), Color.White);
            if (active)
            {
                this.currentLine = DoTerminal(bounds.X + 3 + (int)Terminal.PROMPT_OFFSET + (int)tinyfont.MeasureString(this.prompt.RemoveFormatting()).X, num2, bounds.Width - i - 4, bounds.Height, 1, this.currentLine);
                if (this.WasActivated)
                {
                    this.ExecuteLine();
                }
                if (Hacknet.Gui.TextBox.UpWasPresed)
                {
                    if (this.runCommands.Count > 0)
                    {
                        this.commandHistoryOffset++;
                        if (this.commandHistoryOffset > this.runCommands.Count)
                        {
                            this.commandHistoryOffset = this.runCommands.Count;
                        }
                        this.currentLine = this.runCommands[this.runCommands.Count - this.commandHistoryOffset];
                        this.cursorPosition = this.currentLine.Length;
                    }
                }
                if (Hacknet.Gui.TextBox.DownWasPresed)
                {
                    if (this.commandHistoryOffset > 0)
                    {
                        this.commandHistoryOffset--;
                        if (this.commandHistoryOffset < 0)
                        {
                            this.commandHistoryOffset = 0;
                        }
                        if (this.commandHistoryOffset <= 0)
                        {
                            this.currentLine = "";
                        }
                        else
                        {
                            this.currentLine = this.runCommands[this.runCommands.Count - this.commandHistoryOffset];
                        }
                        this.cursorPosition = this.currentLine.Length;
                    }
                }
            }
            /*if (Hacknet.Gui.TextBox.TabWasPresed)
            {
                if (this.usingTabExecution)
                {
                    this.ExecuteLine();
                }
                else
                {
                    this.doTabComplete();
                }
            }*/
        }

        public string DoTerminal(int x, int y, int width, int selectionHeight, int lines, string str)
        {
            var font = Hacknet.GuiData.tinyfont;

            Rectangle tmpRect = Hacknet.GuiData.tmpRect;
            tmpRect.X = x;
            tmpRect.Y = y;
            tmpRect.Width = width;
            tmpRect.Height = 0;
            var cursorPosition = str.Length;
            FrameCounter++;

            string filteredStringInput = getFilteredStringInput(str, Hacknet.GuiData.getKeyboadState(), Hacknet.GuiData.getLastKeyboadState());

            if (Hacknet.GuiData.getKeyboadState().IsKeyDown(Keys.Enter) && !Hacknet.GuiData.getLastKeyboadState().IsKeyDown(Keys.Enter))
            {
                this.WasActivated = true;
                this.cursorPosition = 0;
                this.drawOffsetPosition = 0;
            }
            tmpRect.Height = lines * TextBox.LINE_HEIGHT;
            tmpRect.X = x;
            tmpRect.Y = y;
            tmpRect.Width = width;
            tmpRect.Height = 10;
            tmpRect.X += 2;
            tmpRect.Y += 2;
            tmpRect.Width -= 4;
            tmpRect.Height -= 4;
            float num2 = ((float)TextBox.LINE_HEIGHT - font.MeasureString(filteredStringInput).Y) / 2f;
            string text = filteredStringInput;
            int num3 = 0;
            int num4 = 0;
            string text2 = text;
            while (font.MeasureString(text2).X > (float)(width - 5))
            {
                num3++;
                int num5 = text.Length - num4 - (num3 - num4);
                if (num5 < 0)
                {
                    break;
                }
                text2 = text.Substring(num4, num5);
            }
            if (this.cursorPosition < this.drawOffsetPosition)
            {
                this.drawOffsetPosition = System.Math.Max(0, this.drawOffsetPosition - 1);
            }
            while (this.cursorPosition > this.drawOffsetPosition + (text.Length - num3))
            {
                this.drawOffsetPosition++;
            }
            if (text.Length <= num3 || this.drawOffsetPosition < 0)
            {
                if (this.drawOffsetPosition <= text.Length - num3)
                {
                    this.drawOffsetPosition = text.Length - num3;
                }
                else
                {
                    this.drawOffsetPosition = 0;
                }
            }
            else if (this.drawOffsetPosition > num3)
            {
                num3 = this.drawOffsetPosition;
            }
            if (num3 > text.Length)
            {
                num3 = text.Length - 1;
            }
            if (this.drawOffsetPosition >= text.Length)
            {
                this.drawOffsetPosition = 0;
            }
            text = text.Substring(this.drawOffsetPosition, text.Length - num3);

            if (filteredStringInput != "")
            {
                int num6 = System.Math.Min(this.cursorPosition - this.drawOffsetPosition, text.Length);
                if (num6 <= 0)
                {
                    num6 = 1;
                }
                if (text.Length == 0)
                {
                    tmpRect.X = x;
                }
                else
                {
                    tmpRect.X = (int)((float)x + font.MeasureString(text.Substring(0, num6)).X) + 3;
                }
            }
            else
            {
                tmpRect.X = x + 3;
            }
            tmpRect.Y = y + 2;
            tmpRect.Width = 1;
            tmpRect.Height = TextBox.LINE_HEIGHT - 4;
            if (Hacknet.Localization.LocaleActivator.ActiveLocaleIsCJK())
            {
                tmpRect.Y += 4;
            }
            Hacknet.GuiData.spriteBatch.Draw(Hacknet.Utils.white, tmpRect, (FrameCounter % 60 < 40) ? Color.White : Color.Gray);
            return filteredStringInput;
        }

        private string getFilteredStringInput(string s, KeyboardState input, KeyboardState lastInput)
        {
            char[] filteredKeys = Hacknet.GuiData.getFilteredKeys();
            for (int i = 0; i < filteredKeys.Length; i++)
            {
                char c = filteredKeys[i];
                string str = s.Substring(0, this.cursorPosition) + c;
                s = str + s.Substring(this.cursorPosition);
                this.cursorPosition++;
            }
            Keys[] pressedKeys = input.GetPressedKeys();
            if (pressedKeys.Length == 1 && lastInput.IsKeyDown(pressedKeys[0]))
            {
                if (pressedKeys[0] == this.lastHeldKey && TextBox.IsSpecialKey(pressedKeys[0]))
                {
                    this.keyRepeatDelay -= Hacknet.GuiData.lastTimeStep;
                    if (this.keyRepeatDelay <= 0f)
                    {
                        s = this.forceHandleKeyPress(s, pressedKeys[0], input, lastInput);
                        this.keyRepeatDelay = 0.04f;
                    }
                }
                else
                {
                    this.lastHeldKey = pressedKeys[0];
                    this.keyRepeatDelay = 0.44f;
                }
            }
            else
            {
                for (int i = 0; i < pressedKeys.Length; i++)
                {
                    if (!lastInput.IsKeyDown(pressedKeys[i]))
                    {
                        if (TextBox.IsSpecialKey(pressedKeys[i]))
                        {
                            Keys keys = pressedKeys[i];
                            switch (keys)
                            {
                                case Keys.Back:
                                    goto IL_1DF;
                                case Keys.Tab:
                                    TextBox.TabWasPresed = true;
                                    break;
                                default:
                                    switch (keys)
                                    {
                                        case Keys.End:
                                            this.cursorPosition = (this.cursorPosition = s.Length);
                                            break;
                                        case Keys.Home:
                                            this.cursorPosition = 0;
                                            break;
                                        case Keys.Left:
                                            this.cursorPosition--;
                                            if (this.cursorPosition < 0)
                                            {
                                                this.cursorPosition = 0;
                                            }
                                            break;
                                        case Keys.Up:
                                            TextBox.UpWasPresed = true;
                                            break;
                                        case Keys.Right:
                                            this.cursorPosition++;
                                            if (this.cursorPosition > s.Length)
                                            {
                                                this.cursorPosition = s.Length;
                                            }
                                            break;
                                        case Keys.Down:
                                            TextBox.DownWasPresed = true;
                                            break;
                                        case Keys.Select:
                                        case Keys.Print:
                                        case Keys.Execute:
                                        case Keys.PrintScreen:
                                        case Keys.Insert:
                                            break;
                                        case Keys.Delete:
                                            if (s.Length > 0 && this.cursorPosition < s.Length)
                                            {
                                                string str = s.Substring(0, this.cursorPosition);
                                                s = str + s.Substring(this.cursorPosition + 1);
                                            }
                                            break;
                                        default:
                                            if (keys == Keys.OemClear)
                                            {
                                                goto IL_1DF;
                                            }
                                            break;
                                    }
                                    break;
                            }
                            goto IL_2CC;
                            IL_1DF:
                            if (s.Length > 0 && this.cursorPosition > 0)
                            {
                                string str = s.Substring(0, this.cursorPosition - 1);
                                s = str + s.Substring(this.cursorPosition);
                                this.cursorPosition--;
                            }
                        }
                        IL_2CC:;
                    }
                }
            }
            return s;
        }

        private string forceHandleKeyPress(string s, Keys key, KeyboardState input, KeyboardState lastInput)
        {
            if (!TextBox.IsSpecialKey(key))
            {
                string str = TextBox.ConvertKeyToChar(key, input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.CapsLock) || input.IsKeyDown(Keys.RightAlt));
                string str2 = s.Substring(0, TextBox.cursorPosition) + str;
                s = str2 + s.Substring(TextBox.cursorPosition);
                TextBox.cursorPosition++;
            }
            else
            {
                if (key <= Keys.Down)
                {
                    switch (key)
                    {
                        case Keys.Back:
                            break;
                        case Keys.Tab:
                            TextBox.TabWasPresed = true;
                            goto IL_186;
                        default:
                            switch (key)
                            {
                                case Keys.Left:
                                    this.cursorPosition--;
                                    if (this.cursorPosition < 0)
                                    {
                                        this.cursorPosition = 0;
                                    }
                                    goto IL_186;
                                case Keys.Up:
                                    TextBox.UpWasPresed = true;
                                    goto IL_186;
                                case Keys.Right:
                                    this.cursorPosition++;
                                    if (this.cursorPosition > s.Length)
                                    {
                                        this.cursorPosition = s.Length;
                                    }
                                    goto IL_186;
                                case Keys.Down:
                                    TextBox.DownWasPresed = true;
                                    goto IL_186;
                                default:
                                    goto IL_184;
                            }
                            break;
                    }
                }
                else if (key != Keys.Delete && key != Keys.OemClear)
                {
                    goto IL_184;
                }
                if (s.Length > 0 && this.cursorPosition > 0)
                {
                    string str2 = s.Substring(0, this.cursorPosition - 1);
                    s = str2 + s.Substring(this.cursorPosition);
                    this.cursorPosition--;
                }
                IL_184:
                IL_186:;
            }
            return s;
        }

        public void ExecuteLine()
        {
            this.WasActivated = false;
            string text = this.currentLine;
            if (Hacknet.Gui.TextBox.MaskingText)
            {
                text = "";
                for (int i = 0; i < this.currentLine.Length; i++)
                {
                    text += "*";
                }
            }
            this.history.Add(this.prompt + text);
            this.lastRunCommand = this.currentLine;
            this.runCommands.Add(this.currentLine);

            parent.userScreen.Execute(this.currentLine);
            this.currentLine = "";
            cursorPosition = 0;
            this.drawOffsetPosition = 0;
        }
    }
}
