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
using System.Net;
using Condor.Models.API.CommandModels;
using Newtonsoft.Json.Linq;
using ModernHttpClient;
using System.Net.Http;
namespace Condor.Helpers
{
    class CondorAPI
    {
        private static CondorAPI m_theAPI = null;
        private string m_addr;
        private CondorHandlerThread m_thread;
        private string m_tok;

        public static CondorAPI getInstance()
        {
            if (m_theAPI == null)
                m_theAPI = new CondorAPI();
            return m_theAPI;
        }

        private CondorAPI()
        {
            
        }

        public void Initialize(string addr)
        {
            m_addr = addr;
            m_thread = new CondorHandlerThread(m_addr);
            m_thread.Start();
        }

        public void Execute(APICommand cmd)
        {
            cmd.Token = this.Token;
            m_thread.Enqueue(cmd);
        }

        public string Token
        {
            get
            {
                return m_tok;
            }
            set
            {
                m_tok = value;
            }
        }
    }
}
