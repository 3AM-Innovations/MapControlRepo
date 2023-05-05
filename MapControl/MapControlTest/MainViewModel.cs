using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;

namespace MapControlTest
{
	public class MainViewModel : INotifyPropertyChanged
	{
		public MainViewModel()
		{
			RandomlyMove = true;
			RandomlyMoveInterval = (int)TimeSpan.FromSeconds(1).TotalMilliseconds;
			RandomlyMoveVisibility = Visibility.Collapsed;
			RandomlyResize = true;
		}

		public bool RandomlyMove { get; set; }

		public int RandomlyMoveInterval { get; }

		public int RandomlyResizeInterval { get; set; }

		public int RandomlyMoveElapsed { get; set; }

		public int RandomlyResizeElapsed { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public double Heading { get; set; }

		public double Pitch { get; set; }

		public double Zoom { get; set; }

		public Visibility RandomlyMoveVisibility { get; set; }

		public bool RandomlyResize { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}