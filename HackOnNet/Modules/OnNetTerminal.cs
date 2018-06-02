using Hacknet;
using HackOnNet.GUI;
using HackOnNet.Modules.Tabs;
using HackOnNet.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackOnNet.Modules
{
    class OnNetTerminal : OnCoreModule
    {
        public static float PROMPT_OFFSET = 0f;

        public Tab LocalhostTab;
        public Tab ActiveTab;
        public List<Tab> TabList = new List<Tab>();
        public List<Tab> BottomTabList {
            get
            {
                return TabList.Where(x => x.IsBottom).ToList();
            }
        }
        public List<Tab> TopTabList
        {
            get
            {
                return TabList.Where(x => !x.IsBottom).ToList();
            }
        }

        public Tab FullTab;
        public Tab BottomTab;
        public Tab TopTab;

        public bool usingTabExecution = false;

        public bool preventingExecution = false;

        public bool executionPreventionIsInteruptable = false;

        private Color outlineColor = new Color(68, 68, 68);

        private Color backColor = new Color(8, 8, 8);

        private Color historyTextColor = new Color(220, 220, 220);

        private Color currentTextColor = Color.White;

        public OnNetTerminal(Rectangle location, UserScreen screen) : base(location, screen)
		{
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;
        }

        public override void LoadContent()
        {
            LocalhostTab = new Tab(this, "localhost", userScreen.username);
            var TestTab = new Tab(this, "2.2.2.2", userScreen.username);
            ActiveTab = LocalhostTab;
            FullTab = ActiveTab;
            BottomTab = FullTab;
            TabList.Add(ActiveTab);
            TabList.Add(TestTab);
            MoveTab(TestTab, TOP, true);
        }

        public bool IsSplitTerminal()
        {
            bool hasBottom = false;
            bool hasTop = false;
            foreach(Tab tab in TabList)
            {
                if (tab.IsBottom)
                    hasBottom = true;
                else
                    hasTop = true;
            }
            return hasTop == hasBottom;
        }

        public override void Update(float t)
        {
        }

        public void DrawFullTerminal(float t)
        {
            var tmpRect = this.bounds;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.moduleColorBacking);
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));
            tmpRect.Height = Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.moduleColorStrong);
            this.spriteBatch.DrawString(GuiData.detailfont, "TERMINAL - " + FullTab.target + " ; Tab " + (TabList.IndexOf(FullTab)+1) + "/" + TabList.Count, new Vector2((float)(tmpRect.X + 2), (float)(tmpRect.Y + 2)), this.userScreen.semiTransText);
            tmpRect = this.bounds;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));

            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            this.spriteBatch.Draw(Utils.white, this.bounds, this.userScreen.displayModuleExtraLayerBackingColor);

            int num = (int)((float)(this.bounds.Height - 12) / (tinyFontCharHeight + 1f));
            num -= 3;
            num = System.Math.Min(num, FullTab.history.Count);
            Vector2 input = new Vector2((float)(this.bounds.X + 4), (float)(this.bounds.Y + this.bounds.Height) - tinyFontCharHeight * 5f);
            if (num > 0)
            {
                for (int i = FullTab.history.Count; i > FullTab.history.Count - num; i--)
                {
                    try
                    {
                        this.spriteBatch.DrawFormatString(FullTab.history[i - 1], Utils.ClipVec2ForTextRendering(input));
                        input.Y -= tinyFontCharHeight + 1f;
                        if (input.Y < tmpRect.Y + 20)
                            break;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            FullTab.doGui(bounds, true);
        }


        public static bool BOTTOM = true;
        public static bool TOP = false;

        public void MoveTab(Tab tab, bool direction, bool changeActive)
        {
            if(direction == BOTTOM)
            {
                if(!tab.IsBottom)
                {
                    var idx = TopTabList.IndexOf(tab);
                    var nextTab = TopTabList[(idx == -1 ? 0 : ((idx + 1) % TopTabList.Count))];
                    if (nextTab == tab)
                        TopTab = null;
                    else
                        TopTab = nextTab;
                    tab.isBottom = true;
                    ActiveTab = tab;
                    BottomTab = tab;
                }
            }
            else if(direction == TOP)
            {
                if(tab.IsBottom)
                {
                    var idx = BottomTabList.IndexOf(tab);
                    var nextTab = BottomTabList[(idx == -1 ? 0 : ((idx + 1) % BottomTabList.Count))];
                    if (nextTab == tab)
                        BottomTab = null;
                    else
                        BottomTab = nextTab;
                    ActiveTab = tab;
                    tab.isBottom = false;
                    TopTab = tab;
                }
            }
        }

        public void DrawTopTerminal(float t)
        {
            var tmpRect = this.bounds;
            tmpRect.Height /= 2;
            tmpRect.Height -= 15;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.moduleColorBacking);
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));
            tmpRect.Height = Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect , 
                Color.Lerp(this.userScreen.moduleColorStrong, Color.LightBlue,
                (TopTab == ActiveTab ? 0.1f : 0f)));
            this.spriteBatch.DrawString(GuiData.detailfont, (TopTab == ActiveTab ? "[ACTIVE] " : "") + "TERMINAL - " + TopTab.target + " ; Tab " + (TopTabList.IndexOf(TopTab) + 1) + "/" + TopTabList.Count, new Vector2((float)(tmpRect.X + 2), (float)(tmpRect.Y + 2)), this.userScreen.semiTransText);
            tmpRect = this.bounds;
            tmpRect.Height /= 2;
            tmpRect.Height -= 15;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1, 
                Color.Lerp(this.userScreen.moduleColorSolid, Color.LightBlue,
                (TopTab == ActiveTab ? 0.45f : 0f)));

            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.displayModuleExtraLayerBackingColor);

            int num = (int)((float)(this.bounds.Height - 12) / (tinyFontCharHeight + 1f));
            num -= 3;
            num = System.Math.Min(num, TopTab.history.Count);
            Vector2 input = new Vector2((float)(tmpRect.X + 4), (float)(tmpRect.Y + tmpRect.Height) - tinyFontCharHeight * 5f);
            if (num > 0)
            {
                for (int i = TopTab.history.Count; i > TopTab.history.Count - num; i--)
                {
                    try
                    {
                        this.spriteBatch.DrawFormatString(TopTab.history[i - 1], Utils.ClipVec2ForTextRendering(input));
                        input.Y -= tinyFontCharHeight + 1f;
                        if (input.Y < tmpRect.Y + 20)
                            break;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            TopTab.doGui(tmpRect, ActiveTab == TopTab);
        }

        public void DrawBottomTerminal(float t)
        {
            var tmpRect = this.bounds;
            tmpRect.Height /= 2;
            tmpRect.Y += tmpRect.Height;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.moduleColorBacking);
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1, new Color?(this.userScreen.moduleColorSolid));
            tmpRect.Height = Module.PANEL_HEIGHT;
            this.spriteBatch.Draw(Utils.white, tmpRect,
                Color.Lerp(this.userScreen.moduleColorStrong, Color.LightBlue,
                (BottomTab == ActiveTab ? 0.1f : 0f)));
            this.spriteBatch.DrawString(GuiData.detailfont, (BottomTab == ActiveTab ? "[ACTIVE] " : "") + "TERMINAL - " + BottomTab.target + " ; Tab " + (BottomTabList.IndexOf(BottomTab) + 1) + "/" + BottomTabList.Count, new Vector2((float)(tmpRect.X + 2), (float)(tmpRect.Y + 2)), this.userScreen.semiTransText);
            tmpRect = this.bounds;
            tmpRect.Height /= 2;
            tmpRect.Y += tmpRect.Height;
            tmpRect.Y = tmpRect.Y - Module.PANEL_HEIGHT;
            tmpRect.Height = tmpRect.Height + Module.PANEL_HEIGHT;
            Hacknet.Gui.RenderedRectangle.doRectangleOutline(tmpRect.X, tmpRect.Y, tmpRect.Width, tmpRect.Height, 1,
                Color.Lerp(this.userScreen.moduleColorSolid, Color.LightBlue,
                (BottomTab == ActiveTab ? 0.45f : 0f)));

            float tinyFontCharHeight = GuiData.ActiveFontConfig.tinyFontCharHeight;
            this.spriteBatch.Draw(Utils.white, tmpRect, this.userScreen.displayModuleExtraLayerBackingColor);

            int num = (int)((float)(this.bounds.Height - 12) / (tinyFontCharHeight + 1f));
            num -= 3;
            num = System.Math.Min(num, BottomTab.history.Count);
            Vector2 input = new Vector2((float)(this.bounds.X + 4), (float)(this.bounds.Y + this.bounds.Height) - tinyFontCharHeight * 5f);
            if (num > 0)
            {
                for (int i = BottomTab.history.Count; i > BottomTab.history.Count - num; i--)
                {
                    try
                    {
                        this.spriteBatch.DrawFormatString(BottomTab.history[i - 1], Utils.ClipVec2ForTextRendering(input));
                        input.Y -= tinyFontCharHeight + 1f;
                        if (input.Y < tmpRect.Y + 20)
                            break;
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            BottomTab.doGui(tmpRect, ActiveTab == BottomTab);
        }

        public void ShiftDisplay(bool forwards)
        {
            if(IsSplitTerminal())
            {
                var TabList = ActiveTab.IsBottom ? BottomTabList : TopTabList;
                ActiveTab = TabList[(TabList.IndexOf(ActiveTab) + 1) % TabList.Count];
                if (ActiveTab.IsBottom)
                    BottomTab = ActiveTab;
                else
                    TopTab = ActiveTab;
            }
            else
            {
                ActiveTab = TabList[(TabList.IndexOf(ActiveTab) + 1) % TabList.Count];
                FullTab = ActiveTab;
            }
        }

        public bool WasPressed(Keys key)
        {
            return Hacknet.GuiData.getKeyboadState().IsKeyDown(key) && Hacknet.GuiData.getLastKeyboadState().IsKeyUp(key);
        }

        public override void Draw(float t)
        {
            if(Hacknet.GuiData.getKeyboadState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
            {
                if(WasPressed(Microsoft.Xna.Framework.Input.Keys.Up))
                {
                    MoveTab(ActiveTab, TOP, true);
                }
                else if (WasPressed(Microsoft.Xna.Framework.Input.Keys.Down))
                {
                    MoveTab(ActiveTab, BOTTOM, true);
                }
                else if (WasPressed(Microsoft.Xna.Framework.Input.Keys.S))
                {
                    if(IsSplitTerminal())
                    {
                        if (ActiveTab.IsBottom)
                            ActiveTab = TopTab;
                        else
                            ActiveTab = BottomTab;
                    }
                }
                else if (WasPressed(Microsoft.Xna.Framework.Input.Keys.Right))
                {
                    ShiftDisplay(true);
                }
                else if(WasPressed(Microsoft.Xna.Framework.Input.Keys.Left))
                {
                    ShiftDisplay(false);
                }
            }
            

            if(!IsSplitTerminal())
            {
                DrawFullTerminal(t);
            }
            else
            {
                DrawTopTerminal(t);
                DrawBottomTerminal(t);
            }
            
        }

        

        public void writeLine(string text)
        {
            /*text = Utils.SuperSmartTwimForWidth(text, this.bounds.Width - 6, GuiData.tinyfont);
            string[] array = text.Split(new char[]
            {
                '\n'
            });
            for (int i = 0; i < array.Length; i++)
            {
                this.history.Add(array[i]);
            }*/
        }

        public void write(string text)
        {
            /*if (this.history.Count <= 0 || GuiData.tinyfont.MeasureString(this.history[this.history.Count - 1] + text).X > (float)(this.bounds.Width - 6))
            {
                this.writeLine(text);
            }
            else
            {
                System.Collections.Generic.List<string> list;
                int index;
                (list = this.history)[index = this.history.Count - 1] = list[index] + text;
            }*/
        }

        public void clearCurrentLine()
        {
            /*this.currentLine = "";
            Hacknet.Gui.TextBox.cursorPosition = 0;
            Hacknet.Gui.TextBox.textDrawOffsetPosition = 0;*/
        }

        public void reset()
        {
            /*this.history.Clear();
            this.clearCurrentLine();*/
        }

        public int commandsRun()
        {
            //return this.runCommands.Count;
            return 0;
        }

        public string getLastRunCommand()
        {
            return "";
            //return this.lastRunCommand;
        }
    }
}
