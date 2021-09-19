using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

using Shamisen.Optimization;

namespace Shamisen.Tests.AndroidNet6
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            CounterLabel.Text = $"Armv8 Intrinsics: {IntrinsicsUtils.ArmIntrinsics}";

            SemanticScreenReader.Announce(CounterLabel.Text);
        }
    }
}
