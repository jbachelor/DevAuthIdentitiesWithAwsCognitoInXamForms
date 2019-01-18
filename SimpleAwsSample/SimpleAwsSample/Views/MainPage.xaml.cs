using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleAwsSample.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void OnStatusLabelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Label.Height))
            {
                #region hack

                // silly hack to get the dang label to scroll ALL the way to the bottom.
                // Comment out the delay below, and you'll notice that the scrollView will
                // scroll *close* to the bottom, but a few lines will be left still below what
                // is visible.
                await Task.Delay(3); 

                #endregion hack

                await StatusScrollView.ScrollToAsync(StatusTextLabel, ScrollToPosition.End, true);
            }
        }
    }
}