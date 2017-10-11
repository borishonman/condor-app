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
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Condor.Helpers;

namespace Condor.iOS
{
    class Config : IConfig
    {
        private NSUserDefaults m_defaults;

        public Config()
        {
            m_defaults = NSUserDefaults.StandardUserDefaults;
        }

        public IConfig GetConfig()
        {
            return new Config();
        }

        public string Address
        {
            get
            {
                return m_defaults.StringForKey("addr");
            }
            set
            {
                m_defaults.SetString(value, "addr");
            }
        }

        public string Token
        {
            get
            {
                return m_defaults.StringForKey("token");
            }
            set
            {
                m_defaults.SetString(value, "token");
            }
        }
    }
}