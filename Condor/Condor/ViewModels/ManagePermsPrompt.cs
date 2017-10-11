/*
    This file is part of Condor.
    Condor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    Condor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with Condor.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Condor.ViewModels
{
    class ManagePermsPrompt : Prompt
    {
        public event EventHandler OnToggleCreate;
        public event EventHandler OnToggleMod;

        private Button m_btnToggleCreate;
        private Button m_btnToggleMod;
        public ManagePermsPrompt()
        {
            this.PromptTitle = "Member Permission";
            this.PositiveButtonVisible = false;
            this.NegativeButtonText = "Close";

            m_btnToggleCreate = new Button() { Text = "Toggle Create Project Permission" };
            m_btnToggleMod = new Button() { Text = "Toggle Moderator Permission" };

            m_btnToggleCreate.Clicked += new EventHandler((o, e) =>
            {
                if (OnToggleCreate != null)
                    OnToggleCreate(o, e);
                this.Dismiss();
            });
            m_btnToggleMod.Clicked += new EventHandler((o, e) =>
            {
                if (OnToggleMod != null)
                    OnToggleMod(o, e);
                this.Dismiss();
            });

            AddView(m_btnToggleCreate);
            AddView(m_btnToggleMod);
        }
    }
}
