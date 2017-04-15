﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using Microsoft.Win32;

using Rock;

namespace Slingshot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        private Importer _importer = null;
        private Timer _timer = null;

        /// <summary>
        /// Gets or sets the rock URL.
        /// </summary>
        /// <value>
        /// The rock URL.
        /// </value>
        public string RockUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the rock user.
        /// </summary>
        /// <value>
        /// The name of the rock user.
        /// </value>
        public string RockUserName { get; set; }

        /// <summary>
        /// Gets or sets the rock password.
        /// </summary>
        /// <value>
        /// The rock password.
        /// </value>
        public string RockPassword { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectSlingshotFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectSlingshotFile_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".dplx";
            dlg.Filter = "Slingshot Files (.slingshot)|*.slingshot";

            if ( dlg.ShowDialog() == true )
            {
                tbSlingshotFileName.Text = dlg.FileName;
            }
        }

        private static Stopwatch _stopwatch = null;
        private object timerState = new object();

        /// <summary>
        /// Handles the Click event of the btnGo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnImport_Click( object sender, RoutedEventArgs e )
        {
            _importer = new Importer( tbSlingshotFileName.Text, this.RockUrl, this.RockUserName, this.RockPassword );

            btnImport.IsEnabled = false;
            btnImportPhotos.IsEnabled = false;
            _stopwatch = Stopwatch.StartNew();

            _timer = new Timer( 100 );
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += _importer.BackgroundWorker_DoImport;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the Elapsed event of the _timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void _timer_Elapsed( object sender, ElapsedEventArgs e )
        {
            this.Dispatcher.Invoke( () =>
            {
                string timerText = $"{Math.Round( _stopwatch.Elapsed.TotalSeconds, 1 )} seconds";
                lblTimer.Content = timerText;
            } );
        }

        /// <summary>
        /// Handles the ProgressChanged event of the BackgroundWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void BackgroundWorker_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            string resultText = string.Empty;
            if ( e.UserState is string )
            {
                resultText = e.UserState.ToString();
            }

            var resultsCopy = _importer.Results.ToArray();
            foreach ( var result in resultsCopy )
            {
                resultText += $"\n\n{result.Key}\n\n{result.Value}";
            }

            tbResults.Text = resultText.Trim();
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the BackgroundWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        private void BackgroundWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if ( e.Error != null )
            {
                if ( e.Error is SlingshotException )
                {
                    tbResults.Text = e.Error.Message + "\n\n" + tbResults.Text;
                }
                else
                {
                    tbResults.Text = e.Error.ToString() + "\n\n" + e.Error.StackTrace + "\n\n" + tbResults.Text;
                }
            }

            btnImport.IsEnabled = true;
            btnImportPhotos.IsEnabled = true;
            _timer.Stop();
            _stopwatch.Stop();
        }


        /// <summary>
        /// Handles the Click event of the btnImportPhotos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnImportPhotos_Click( object sender, RoutedEventArgs e )
        {
            _importer = new Importer( tbSlingshotFileName.Text, this.RockUrl, this.RockUserName, this.RockPassword );
            _importer.PhotoBatchSizeMB = tbPhotoBatchSize.Text.AsInteger();

            btnImportPhotos.IsEnabled = false;
            btnImport.IsEnabled = false;
            _stopwatch = Stopwatch.StartNew();

            _timer = new Timer( 100 );
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();

            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += _importer.BackgroundWorker_DoImportPhotos;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerAsync();
        }
    }
}
