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

using Condor.Helpers;
using Condor.Droid;
using Android.Content.PM;

[assembly: Xamarin.Forms.Dependency(typeof(UpdaterImplAndroid))]
namespace Condor.Droid
{
    class UpdaterImplAndroid : IUpdaterImpl
    {
        public string GetFileType()
        {
            return "apk";
        }
        public int GetCurrentVersion()
        {
            PackageInfo pinfo = Android.App.Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, PackageInfoFlags.MetaData);
            return pinfo.VersionCode;
        }
    }
}