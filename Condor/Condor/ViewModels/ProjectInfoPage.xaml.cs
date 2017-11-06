using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Condor.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ProjectInfoPage : ContentPage
	{
        public Label Description;

		public ProjectInfoPage ()
		{
			InitializeComponent ();

            Description = txt_project_description;
		}
	}
}