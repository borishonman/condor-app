using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using Condor.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(UpdaterImpliOS))]
namespace Condor.iOS
{
    class UpdaterImpliOS
    {
        public string GetFileType()
        {
            return "ipa";
        }
        public int GetCurrentVersion()
        {
            return 999;
        }
    }
}