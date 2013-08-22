using Win8_Avocado.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Win8_Avocado
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ConversationPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        // Avocado-specific variables
        private string authToken;
        private Dictionary<string, BitmapImage> icons = new Dictionary<string,BitmapImage>();
        private long lastActivityTime = 0;

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


        public ConversationPage()
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            var args = (Dictionary<string, string>) e.Parameter;
            string email;
            string password;
            args.TryGetValue(Avocado.EMAIL, out email);
            args.TryGetValue(Avocado.PASSWORD, out password);
            authToken = await Avocado.Login(email, password);
            await LoadCoupleData();
            await LoadActivities();
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async(source) =>
            {
                // 
                // Update the UI thread by using the UI core dispatcher.
                // 
                await Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    async() =>
                    {
                        await LoadActivities();
                    });

            }, TimeSpan.FromSeconds(5));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Avocado.Logout(authToken);
            this.Frame.Navigate(typeof(LoginPage));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Avocado.SendMessage(authToken, messageInput.Text);
            messageInput.Text = "";
        }

        /// <summary>
        /// Loads a list of the latest 100 avocado activities. Currently only supports messages
        /// </summary>
        private async Task<Boolean> LoadActivities()
        {
            var activitiesArray = await Avocado.GetActivities(authToken, lastActivityTime);
            foreach (var activity in activitiesArray)
            {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;
                var tb = new TextBlock();
                tb.TextWrapping = TextWrapping.WrapWholeWords;
                tb.VerticalAlignment = VerticalAlignment.Center;

                if (activity.ValueType != JsonValueType.Object)
                {
                    throw new Exception();
                }
                try
                {
                    var activityObj = activity.GetObject();

                    // For debugging only
                    System.Diagnostics.Debug.WriteLine(activityObj.Stringify());
                    
                    // Record timestamp
                    var timestamp = (long)activityObj.GetNamedNumber("timeCreated", 0.0);
                    if (timestamp > lastActivityTime)
                    {
                        lastActivityTime = timestamp;
                    }

                    // Get user and image
                    var userId = activityObj.GetNamedString("userId");
                    BitmapImage original;
                    icons.TryGetValue(userId, out original);
                    var img = new Image();
                    img.Width = 50;
                    img.Height = 50;
                    img.Source = original;
                    img.Margin = new Thickness(0, 5, 20, 0);
                    sp.Children.Add(img);

                    if (!activityObj.GetNamedString("type").Equals("message"))
                    {
                        // TODO: Gray, italicized
                        tb.Text = "Only messages are currently supported by this client";
                    }
                    else
                    {
                        tb.Text = activity.GetObject().GetNamedObject("data").GetNamedString("text");
                    }
                }
                catch
                {
                    // TODO: Gray, italicized
                    tb.Text = "Error retrieving activity";
                }
                sp.Children.Add(tb);
                activityList.Children.Add(sp);
                activityScrollViewer.Measure(activityScrollViewer.RenderSize);
                activityScrollViewer.ChangeView(0, activityScrollViewer.ScrollableHeight, 1);
            }
            return true;
        }

        /// <summary>
        /// Fills the user's and partner's id fields, as well as image fields so it doesn't need to be perpetually reloaded
        /// Private because this should only be called on login
        /// </summary>
        /// <returns></returns>
        private async Task<Boolean> LoadCoupleData()
        {
            var userArray = await Avocado.GetUsers(authToken);
            // Parse Json
            var user1 = userArray.GetObjectAt(0);
            var user2 = userArray.GetObjectAt(1);
            var img1 = FetchUserImage(user1);
            var img2 = FetchUserImage(user2);

            var id1 = user1.GetNamedString("id");
            var id2 = user2.GetNamedString("id");
            icons.Add(id1, img1);
            icons.Add(id2, img2);
            return true;
        }

        private BitmapImage FetchUserImage(JsonObject user)
        {
            var imgSrc = user.GetNamedString("avatarUrl");
            return new BitmapImage(new Uri(imgSrc));
        }
    }
}
