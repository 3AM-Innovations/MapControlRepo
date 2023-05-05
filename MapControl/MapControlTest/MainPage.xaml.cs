using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MapControlTest
{
	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly BackgroundWorker _worker;
		private Size _displaySize;

		public MainPage()
		{
			InitializeComponent();

			ViewModel = new MainViewModel();
			DataContext = ViewModel;

			_worker = new BackgroundWorker();
			_worker.DoWork += WorkerOnDoWork;
			_worker.WorkerSupportsCancellation = true;

			Loaded += OnLoaded;
		}

		public MainViewModel ViewModel { get; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var v = DisplayInformation.GetForCurrentView();
			_displaySize = new Size(v.ScreenWidthInRawPixels, v.ScreenHeightInRawPixels);

			TestButton_OnClick(TestButton, new RoutedEventArgs());
			ViewModel.RandomlyMove = true;
			_worker.RunWorkerAsync();
		}

		private async void WorkerOnDoWork(object sender, DoWorkEventArgs e)
		{
			var r = new Random();
			var w = Stopwatch.StartNew();
			var rw = Stopwatch.StartNew();
			var cw = Stopwatch.StartNew();

			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				() =>
				{
					ViewModel.RandomlyResizeInterval = r.Next(1, 10) * 1000;
				});
			
			while (!_worker.CancellationPending)
			{
				if (cw.Elapsed.TotalSeconds >= 60)
				{
					await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
						() => TestButton_OnClick(TestButton, new RoutedEventArgs())
					);

					cw.Restart();
				}

				if (rw.Elapsed.TotalMilliseconds >= ViewModel.RandomlyResizeInterval)
				{
					if (ViewModel.RandomlyResize)
					{
						await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
							() =>
							{
								var width = r.Next(500, (int)_displaySize.Width);
								var height = r.Next(500, (int)_displaySize.Height);

								ApplicationView
									.GetForCurrentView()
									.TryResizeView(new Size(width: width, height: height));

								ViewModel.RandomlyResizeInterval = r.Next(1, 10) * 1000;
								ViewModel.RandomlyResizeElapsed = 0;
							}
						);
					}

					rw.Restart();
				}

				if (!ViewModel.RandomlyMove || !w.IsRunning || w.Elapsed.TotalMilliseconds < ViewModel.RandomlyMoveInterval)
				{
					Thread.Sleep(500);

					await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
						() =>
						{
							ViewModel.RandomlyResizeElapsed = (int)rw.Elapsed.TotalMilliseconds;
							ViewModel.RandomlyMoveElapsed = (int)w.Elapsed.TotalMilliseconds;
						}
					);

					continue;
				}

				await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
				{
					w.Reset();

					ViewModel.RandomlyMoveElapsed = 0;
					ViewModel.RandomlyMoveVisibility = Visibility.Collapsed;

					var location = Map.Center.Position;
					location.Latitude += (r.Next(0, 1000) / 100000.0) * (r.Next(0, 100) % 2 == 0 ? 1 : -1);
					location.Longitude += (r.Next(0, 1000) / 100000.0) * (r.Next(0, 100) % 2 == 0 ? 1 : -1);

					var zoom = r.Next(17, 19) + r.NextDouble();
					var heading = r.Next(0, 360);
					var pitch = r.Next(60, 90);
					var point = new Geopoint(location, AltitudeReferenceSystem.Terrain);

					await Map.TrySetViewAsync(point, zoom, heading, pitch, MapAnimationKind.Default);
					w.Restart();

					ViewModel.RandomlyMoveVisibility = Visibility.Visible;
				});
			}
		}

		private void TestButton_OnClick(object sender, RoutedEventArgs e)
		{
			Map.Style = MapStyle.Aerial3DWithRoads;
			Map.Center = new Geopoint(
				new BasicGeoposition
				{
					Altitude = 0,
					Latitude = -37.8155,
					Longitude = 144.965
				}, AltitudeReferenceSystem.Terrain
			);
			Map.ZoomLevel = 16;
		}

		private void Workspaces_OnClick(object sender, RoutedEventArgs e)
		{
			if (WorkspacesGrid.ColumnDefinitions.Count >= 2)
			{
				WorkspacesGrid.ColumnDefinitions.RemoveAt(1);
				WorkspacesGrid.RowDefinitions.RemoveAt(1);
			}
			else
			{
				// Update Grid
				var newCol = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
				var newRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };

				WorkspacesGrid.ColumnDefinitions.Add(newCol);
				WorkspacesGrid.RowDefinitions.Add(newRow);
			}
		}

		private void Map_OnCenterChanged(MapControl sender, object args)
		{
			var location = sender.Center.Position;
			ViewModel.Latitude = location.Latitude;
			ViewModel.Longitude = location.Longitude;
			ViewModel.Heading = sender.Heading;
			ViewModel.Pitch = sender.Pitch;
			ViewModel.Zoom = sender.ZoomLevel;
		}
	}
}