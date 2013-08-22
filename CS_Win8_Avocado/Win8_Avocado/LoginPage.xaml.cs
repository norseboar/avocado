using Win8_Avocado.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Win8_Avocado
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LoginPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public LoginPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("emailInput"))
            {
                emailInput.Text = roamingSettings.Values["emailInput"].ToString();
            }
            if (roamingSettings.Values.ContainsKey("passwordInput"))
            {
                passwordInput.Password = roamingSettings.Values["passwordInput"].ToString();
            }
            if (roamingSettings.Values.ContainsKey("rememberPasswordInput"))
            {
                rememberPasswordInput.IsChecked = (bool?)roamingSettings.Values["rememberPasswordInput"];
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void EmailInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["emailInput"] = emailInput.Text;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (rememberPasswordInput.IsChecked == true)
            {
                storePassword();
            }
        }
        private void RememberPasswordInput_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["rememberPasswordInput"] = rememberPasswordInput.IsChecked;

            // If a user checks the box after inputting password, still remember it
            if (rememberPasswordInput.IsChecked == true)
            {
                storePassword();
            }
            else
            {
                // If a user unchecks the box, immediately forget the password
                roamingSettings.Values["passwordInput"] = "";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                var args = new Dictionary<string, string>();
                args.Add(Avocado.EMAIL, emailInput.Text);
                args.Add(Avocado.PASSWORD, passwordInput.Password);
                this.Frame.Navigate(typeof(ConversationPage), args);
            }
        }

        // Special method to save password so it's encrypted
        private void storePassword()
        {
            ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
            // TODO: Encrypt password
            roamingSettings.Values["passwordInput"] = passwordInput.Password;
        }
    }
}
