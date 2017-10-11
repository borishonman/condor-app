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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Condor.Helpers;
using Condor.ViewModels;
using Condor.Models.API.CommandModels.Permission.Commands;

namespace Condor.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MemberPermsPage : ContentPage
	{
        class MemberInfo
        {
            private string m_user;
            private bool m_create;
            private bool m_mod;

            public string User { get { return m_user; } set { m_user = value; } }
            public bool CanCreateProjects { get { return m_create; } set { m_create = value; } }
            public bool IsModerator { get { return m_mod; } set { m_mod = value; } }
        }
		public MemberPermsPage (MasterDetailPage mdp)
		{
			InitializeComponent ();

            ReloadDisplay();

            tbl_member_perms.ItemSelected += new EventHandler<SelectedItemChangedEventArgs>((o, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                tbl_member_perms.SelectedItem = null;

                MemberInfo i = e.SelectedItem as MemberInfo;

                ManagePermsPrompt p = new ManagePermsPrompt();
                p.OnToggleCreate += new EventHandler((o2, e2) =>
                {
                    ToggleCreatePermissionCommand cmd = new ToggleCreatePermissionCommand() { Member = i.User };
                    cmd.OnResponseReceived += new Models.API.CommandModels.APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update member info
                        i.CanCreateProjects = (bool)response.GetValue("haspermission");
                        ReloadDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.OnToggleMod += new EventHandler((o2, e2) =>
                {
                    ToggleIsModCommand cmd = new ToggleIsModCommand() { Member = i.User };
                    cmd.OnResponseReceived += new Models.API.CommandModels.APICommand.ResponseReceivedHandler((response) =>
                    {
                        if ((string)response.GetValue("result") != "success")
                        {
                            DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                            return;
                        }
                        //update member info
                        i.IsModerator = (bool)response.GetValue("haspermission");
                        ReloadDisplay();
                    });
                    CondorAPI.getInstance().Execute(cmd);
                });
                p.Show(mdp);
            });
		}

        private void ReloadDisplay()
        {
            ObservableCollection<MemberInfo> memberPerms = new ObservableCollection<MemberInfo>();

            GetPermTableCommand cmd = new GetPermTableCommand();
            cmd.OnResponseReceived += new Models.API.CommandModels.APICommand.ResponseReceivedHandler((response) =>
            {
                if ((string)response.GetValue("result") != "success")
                {
                    DependencyService.Get<INotification>().ShortAlert("ERROR: " + (string)response.GetValue("msg"));
                    return;
                }
                JArray members = (JArray)response.GetValue("members");
                foreach (JToken jt in members)
                {
                    JObject j = jt as JObject;
                    MemberInfo newInfo = new MemberInfo();
                    newInfo.User = (string)j.GetValue("userid");
                    newInfo.CanCreateProjects = (bool)j.GetValue("cancreateprojects");
                    newInfo.IsModerator = (bool)j.GetValue("ismoderator");
                    memberPerms.Add(newInfo);
                }
            });
            CondorAPI.getInstance().Execute(cmd);

            tbl_member_perms.ItemsSource = memberPerms;
        }
	}
}