﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TASCompAssistant
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// Should this be readonly??
		private readonly ObservableCollection<Competitor> Competitors = new ObservableCollection<Competitor>();
		private Graph Graph;


		/*	TODO:
				- Error handle & chack that textboxes contain numbers ONLY
				- Add DQ Reasons
				- Add check for Competitors for objects with equivilant Username values, to avoid duplicates
					- On event there is duplicate upon entering via left feild, initiate a yes/no prompt
					  to determine if you should overwrite the values previously submitted for that username
				- Look into using CollectionViewSource rather than ObservableCollection
				- When doubleclicking a checkbox in the datagrid to edit the value, unles you click away, it doesn't commit the edit.
				  can we make it so that upon the value change of the text box, the commit occures?
		*/

		public MainWindow()
		{
			InitializeComponent();

			// Set up datagrid ItemsSource
			var competitors = new ListCollectionView(Competitors);
			competitors.GroupDescriptions.Add(new PropertyGroupDescription("Qualification"));
			Datagrid_Competition.ItemsSource = competitors;
			Datagrid_Score.ItemsSource = Competitors;
		}

		private void MenuItem_File_Exit_Click(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}

		private void btn_AddCompetitor_Click(object sender, RoutedEventArgs e)
		{
			try // Do real error handling when you figure out how to
			{
				var competitor = new Competitor()
				{
					Username = txtbox_Username.Text,
					VIStart = Convert.ToInt32(txtbox_VIStart.Text),
					VIEnd = Convert.ToInt32(txtbox_VIEnd.Text),
					Rerecords = Convert.ToInt32(txtbox_Rerecords.Text),
					DQ = chkbox_DQ.IsChecked.Value,
					DQReason = "coming soon...", // Fix this later
				};

				Competitors.Add(competitor);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"There was an error:\n{ex.Message}");
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string msg = string.Empty;

			foreach (var item in Competitors)
			{
				msg += $"{item.Username}   DQ: {item.DQ}" + Environment.NewLine;
			}

			MessageBox.Show(msg);
		}

		private void Chkbox_DQ_ValueChanged(object sender, RoutedEventArgs e)
		{
			stackPanel_DQReason.IsEnabled = chkbox_DQ.IsChecked.Value;
		}

		private void Chkbox_DQ_Other_ValueChanged(object sender, RoutedEventArgs e)
		{
			txtBox_DQ_Other.IsEnabled = Chkbox_DQ_Other.IsChecked.Value;
		}

		private void TestData_Add(object sender, RoutedEventArgs e)
		{
			Competitors.Clear();

			var r = new Random();
			for (int i = 1; i <= 20; i++)
			{
				var start = r.Next(0, 1000);
				Competitors.Add(new Competitor()
				{
					Username = $"User {i}",
					VIStart = start,
					VIEnd = start + r.Next(0, 1000)
				});
			}

			string output = JsonConvert.SerializeObject(Competitors);
			Clipboard.SetText(output);
		}

		private void SortCompetition(object sender, RoutedEventArgs e)
		{
			// THIS CODE IS BAD BUT IT WORKS >:(

			// Set all DQ's place to total competitors (aka, last place)
			foreach (var item in Competitors)
			{
				if (item.DQ)
				{
					item.Place = Competitors.Count;
				}
			}

			// Sort by time in seconds
			var Sorted = new ObservableCollection<Competitor>(Competitors.OrderBy(i => i.TimeInSeconds));

			// Set first place that isn't a dq
			int place = 0;
			foreach (var item in Sorted)
			{
				place++;
				if (!item.DQ)
				{
					item.Place = 1;
					break;
				}
			}

			// Set the rest of the competitor's place, excluding DQ's competitors
			for (int i = 1; i < Sorted.Count; i++)
			{
				if (!Sorted[i].DQ)
				{
					if (Sorted[i].TimeInSeconds > Sorted[i - 1].TimeInSeconds)
					{
						Sorted[i].Place = ++place;
					}
					else
					{
						Sorted[i].Place = place;
					}
				}
			}

			// The following is bad. Look into this => https://stackoverflow.com/questions/19112922/sort-observablecollectionstring-through-c-sharp
			// This is here because otherwise the event to update UI doesn't trigger. Idk why.
			// Once a better datatype is chosen, the whole sorting method can be altered to be simpler, and more efficient. See above link.
			Competitors.Clear();
			foreach (var item in Sorted)
			{
				Competitors.Add(item);
			}
		}

		// Add DQ Grouping to Datagrid
		private void Chkbox_SplitDQView_Checked(object sender, RoutedEventArgs e)
		{

		}

		// Remove DQ Grouping to Datagrid
		private void Chkbox_SplitDQView_Unchecked(object sender, RoutedEventArgs e)
		{

		}

		private void TestGraph(object sender, RoutedEventArgs e)
		{
			var data = new List<double>();

			foreach (var item in Competitors)
			{
				data.Add(item.VIs);
			}

			Graph = new Graph(data);

			StatisticsGraph.Series = Graph.SeriesCollection;
		}
	}
}
