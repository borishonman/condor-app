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

using Foundation;
using UIKit;
using Condor.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(NotificationIOS))]
namespace Condor.iOS
{
    class NotificationIOS : INotification
    {
        class Notification
        {
            public UIAlertController alert;
            public NSTimer alertDelay;
            public void destroy()
            {
                UIApplication.SharedApplication.InvokeOnMainThread(() =>
                {
                    if (alert != null)
                    {
                        alert.DismissViewController(true, null);
                    }
                    if (alertDelay != null)
                    {
                        alertDelay.Dispose();
                    }
                });
            }
        };
        const double LONG_DELAY = 2.0;
        const double SHORT_DELAY = 1.0;

        public void LongAlert(string message)
        {
            ShowAlert(message, LONG_DELAY);
        }
        public void ShortAlert(string message)
        {
            ShowAlert(message, SHORT_DELAY);
        }

        void ShowAlert(string message, double seconds)
        {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>
            {
                Notification newNot = new Notification();
                newNot.alertDelay = NSTimer.CreateScheduledTimer(seconds, (obj) =>
                {
                    newNot.destroy();
                });
                newNot.alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
                UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(newNot.alert, true, null);
            });
        }
    }
}