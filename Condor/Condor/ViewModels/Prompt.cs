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

using Condor.Views;

namespace Condor.ViewModels
{
    class Prompt : ContentPage
    {
        public delegate void PromptClosedEventListener();
        public event PromptClosedEventListener OnPromptSaved;
        public event PromptClosedEventListener OnPromptDismissed;

        private MasterDetailPage m_mdp;

        private StackLayout m_layout;
        private Label m_title;
        private Grid m_btnsLayout;
        private Button m_btnSave;
        private Button m_btnCancel;

        public Prompt()
        {
            this.OnPromptSaved = null;
            this.OnPromptDismissed = null;

            m_layout = new StackLayout() { VerticalOptions = LayoutOptions.Center,Padding=20 };
            m_title = new Label() { HorizontalTextAlignment = TextAlignment.Center, FontSize = 14 };

            m_btnSave = new Button();
            m_btnCancel = new Button();

            m_btnsLayout = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitionCollection() {
                    new ColumnDefinition() { Width=GridLength.Auto },
                    new ColumnDefinition() { Width=GridLength.Star },
                    new ColumnDefinition() { Width=GridLength.Auto },
                }
            };

            m_btnsLayout.Children.Add(m_btnSave, 2, 0);
            m_btnsLayout.Children.Add(m_btnCancel, 0, 0);

            m_layout.Children.Add(m_title);
            m_layout.Children.Add(m_btnsLayout);

            PositiveButtonText = "Save";
            NegativeButtonText = "Cancel";

            this.Content = m_layout;
        }

        public void AddView(View v)
        {
            m_layout.Children.Insert(m_layout.Children.Count - 1, v);
        }

        public string PromptTitle
        {
            get
            {
                return m_title.Text;
            }
            set
            {
                m_title.Text = value;
                this.Title = value;
            }
        }

        public string PositiveButtonText
        {
            get
            {
                return m_btnSave.Text;
            }
            set
            {
                m_btnSave.Text = value;
            }
        }
        public string NegativeButtonText
        {
            get
            {
                return m_btnCancel.Text;
            }
            set
            {
                m_btnCancel.Text = value;
            }
        }

        public bool PositiveButtonVisible
        {
            get
            {
                return m_btnSave.IsVisible;
            }
            set
            {
                m_btnSave.IsVisible = value;
            }
        }
        public bool NegativeButtonVisible
        {
            get
            {
                return m_btnCancel.IsVisible;
            }
            set
            {
                m_btnCancel.IsVisible = value;
            }
        }

        public void Show(MasterDetailPage mdp)
        {
            m_mdp = mdp;

            m_btnSave.Clicked += new EventHandler((o, e) =>
            {
                Dismiss();
                if (this.OnPromptSaved != null)
                    this.OnPromptSaved();
            });
            m_btnCancel.Clicked += new EventHandler((o, e) =>
            {
                Dismiss();
                if (this.OnPromptDismissed != null)
                    this.OnPromptDismissed();
            });

            ((NavigationPage)mdp.Detail).PushAsync(this);
        }

        public void Dismiss()
        {
            ((NavigationPage)m_mdp.Detail).PopAsync();
        }
    }
}
