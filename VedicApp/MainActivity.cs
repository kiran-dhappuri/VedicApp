﻿using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using VedicApp.Views;
using VedicApp.Models;

namespace VedicApp
{
    [Activity(Label = "VedicApp", MainLauncher = true, Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        Button _btnHome;
        static ProgressBar _proBar;
        //private const string BaseUrl = "http://productwebapplication.azurewebsites.net/";
        //private const string BaseUrl = "http://vedicshop.azurewebsites.net/";
        private const string BaseUrl = "https://bestow.azurewebsites.net/";
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            LoadWebPage();

            _btnHome = FindViewById<Button>(Resource.Id.button1);
            _btnHome.SetBackgroundColor(Color.Black);
            _btnHome.SetTextColor(Color.WhiteSmoke);
            _btnHome.Click += BtnHome_Click;

            _proBar.IndeterminateDrawable.SetColorFilter(Android.Graphics.Color.Cyan, Android.Graphics.PorterDuff.Mode.Multiply);
        }
        private void BtnHome_Click(object sender, EventArgs e)
        {
            Console.WriteLine("clicked");
            LoadWebPage();
        }
        private void LoadWebPage()
        {
            var webView = FindViewById<WebView>(Resource.Id.webView);
            webView.Settings.JavaScriptEnabled = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            webView.SetWebViewClient(new HybridWebViewClient());

            // Render the view from the type generated from RazorView.cshtml
            var model = new Model1() { Text = "Text goes here" };
            var template = new RazorView() { Model = model };
            var page = template.GenerateString();

            // Load the rendered HTML into the view with a base URL 
            // that points to the root of the bundled Assets folder
            //webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
            webView.LoadUrl(BaseUrl);
            webView.Settings.JavaScriptEnabled = true;
            //webView.Settings.BuiltInZoomControls = true;
            webView.Settings.SetSupportZoom(true);
            webView.ScrollBarStyle = ScrollbarStyles.OutsideOverlay;
            webView.ScrollbarFadingEnabled = false;

            _proBar = FindViewById<ProgressBar>(Resource.Id.progressBar1);
        }

        private class HybridWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView webView, string url)
            {

                // If the URL is not our own custom scheme, just let the webView load the URL as usual
                var scheme = "hybrid:";

                if (!url.StartsWith(scheme))
                    return false;

                // This handler will treat everything between the protocol and "?"
                // as the method name.  The querystring has all of the parameters.
                var resources = url.Substring(scheme.Length).Split('?');
                var method = resources[0];
                var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);

                if (method == "UpdateLabel")
                {
                    var textbox = parameters["textbox"];

                    // Add some text to our string here so that we know something
                    // happened on the native part of the round trip.
                    var prepended = string.Format("C# says \"{0}\"", textbox);

                    // Build some javascript using the C#-modified result
                    var js = string.Format("SetLabelText('{0}');", prepended);

                    webView.LoadUrl("javascript:" + js);
                }

                return true;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                if(_proBar != null) _proBar.Visibility = ViewStates.Gone;
                Console.WriteLine("Finished loading");
                base.OnPageFinished(view, url);
            }
        }
    }
}

