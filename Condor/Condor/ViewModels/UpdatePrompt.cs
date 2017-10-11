using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;

namespace Condor.ViewModels
{
    class UpdatePrompt : Prompt
    {
        private Label m_body;
        private Label m_version;

        public UpdatePrompt()
        {
            this.PositiveButtonText = "Go to Download";
            this.NegativeButtonText = "Ignore";
            this.PromptTitle = "Update Available!";

            m_body = new Label() { Margin = 20 };
            m_version = new Label() { Margin = 10 };
            AddView(m_version);
            AddView(m_body);
        }

        public string Text
        {
            get
            {
                return m_body.Text;
            }
            set
            {
                m_body.Text = value;
            }
        }

        public string Version
        {
            set
            {
                m_version.Text = "Version " + value + " available for download";
            }
        }
    }
}
