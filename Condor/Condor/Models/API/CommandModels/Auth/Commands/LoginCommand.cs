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

namespace Condor.Models.API.CommandModels.Auth.Commands
{
    class LoginCommand : AuthAPICommand
    {
        public LoginCommand()
        {
            this.Function = "login";
            this.Token = "";
        }

        public string Username
        {
            get
            {
                return m_data["username"];
            }
            set
            {
                m_data["username"] = value;
            }
        }
        public string Password
        {
            get
            {
                return m_data["password"];
            }
            set
            {
                m_data["password"] = value;
            }
        }
    }
}
