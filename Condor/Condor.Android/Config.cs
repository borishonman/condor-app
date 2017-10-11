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

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android;

using Condor.Helpers;
using Android.Content.Res;

namespace Condor.Droid
{
    class Config : IConfig
    {
        private ISharedPreferences m_prefs;
        private ISharedPreferencesEditor m_prefsEditor;

        public Config()
        {
            Context ctx = Android.App.Application.Context;
            m_prefs = ctx.GetSharedPreferences(ctx.Resources.GetString(Resource.String.app_config_file), FileCreationMode.Private);
            m_prefsEditor = m_prefs.Edit();
        }

        public string Address
        {
            get
            {
                return m_prefs.GetString("addr", "");
            }
            set
            {
                m_prefsEditor.PutString("addr", value);
                m_prefsEditor.Commit();
            }
        }

        public string Token
        {
            get
            {
                return m_prefs.GetString("token", "");
            }
            set
            {
                m_prefsEditor.PutString("token", value);
                m_prefsEditor.Commit();
            }
        }
    }
}