﻿#region XbimHeader

// The eXtensible Building Information Modelling (xBIM) Toolkit
// Solution:    XbimComplete
// Project:     XbimXplorer
// Filename:    App.xaml.cs
// Published:   01, 2012
// Last Edited: 9:05 AM on 20 12 2011
// (See accompanying copyright.rtf)

#endregion

#region Directives

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using log4net;
using Squirrel;
using Xbim.IO.Esent;

#endregion

namespace XbimXplorer
{
    /// <summary>
    ///   Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        private static readonly ILog Log = LogManager.GetLogger("Xbim.WinUI");

        private static async void update()
        {
            if (!File.Exists("..\\update.exe"))
                return;
            try
            {
                using (var mgr = new UpdateManager("http://www.overarching.it/dload/XbimXplorer"))
                {
                    await mgr.UpdateApp();
                    mgr.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error in UpdateManager", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {

            // evaluate special parameters before loading MainWindow
            var blockUpdate = false;
            foreach (var thisArg in e.Args)
            {
                if (string.Compare("/noupdate", thisArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    blockUpdate = true;
                }
            }
            if (!blockUpdate)
                update();
            

            // evaluate special parameters before loading MainWindow
            var blockPlugin = false;
            foreach (var thisArg in e.Args)
            {
                if (string.Compare("/noplugins", thisArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    blockPlugin = true;
                }
            }

            var mainView = new XplorerMainWindow(blockPlugin);
            mainView.Show();
            mainView.DrawingControl.ViewHome();
            var bOneModelLoaded = false;
            for (var i = 0; i< e.Args.Length; i++)
            {
                var thisArg = e.Args[i];
                if (string.Compare("/AccessMode", thisArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var stringMode = e.Args[++i];
                    XbimDBAccess acce;
                    if (Enum.TryParse(stringMode, out acce))
                    {
                        mainView.FileAccessMode = acce;
                    }
                }
                else if (string.Compare("/plugin", thisArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var pluginName = e.Args[++i];
                    if (File.Exists(pluginName))
                    {
                        var fi = new FileInfo(pluginName);
                        var di = fi.Directory;
                        mainView.LoadPlugin(di, true, fi.Name);
                        continue;
                    }
                    if (Directory.Exists(pluginName) )
                    {
                        var di = new DirectoryInfo(pluginName);
                        mainView.LoadPlugin(di, true);
                        continue;
                    }
                    Clipboard.SetText(pluginName);
                    MessageBox.Show(pluginName + " not found. The full file name has been copied to clipboard.", "Plugin not found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (string.Compare("/select", thisArg, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var selLabel = e.Args[++i];
                    Debug.Write("Select " + selLabel + "... ");
                    mainView.LoadingComplete += delegate
                    {
                        int iSel;
                        if (!int.TryParse(selLabel, out iSel))
                            return;
                        if (mainView.Model == null)
                            return;
                        if (mainView.Model.Instances[iSel] == null)
                            return;
                        mainView.SelectedItem = mainView.Model.Instances[iSel];    
                    };
                }
                else if (File.Exists(thisArg) && bOneModelLoaded == false)
                {
                    // does not support the load of two models
                    bOneModelLoaded = true;
                    mainView.LoadAnyModel(thisArg);
                }
            }
        }
    }
}