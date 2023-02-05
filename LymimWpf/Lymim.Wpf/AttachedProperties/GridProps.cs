using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;

namespace Lymim.Wpf.AttachProperties
{
    public class GridProps
    {
        public static string GetColumns(DependencyObject obj) => (string)obj.GetValue(ColumnsProperty);

        public static void SetColumns(DependencyObject obj, string value) => obj.SetValue(ColumnsProperty, value);

        /// <summary>
        /// Columns samples: 1|*|2*|Auto
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(string), typeof(GridProps),
                new PropertyMetadata(null, OnColumnsPropertyChanged));

        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetRowOrColumnDefinitions(
                d, e.NewValue,
                grid => grid.ColumnDefinitions.Clear(),
                (grid, gridLength) => grid.ColumnDefinitions.Add(new ColumnDefinition { Width = gridLength }));
        }

        public static string GetRows(DependencyObject obj) => (string)obj.GetValue(RowsProperty);

        public static void SetRows(DependencyObject obj, string value) => obj.SetValue(RowsProperty, value);

        /// <summary>
        /// Rows samples: 1|*|2*|Auto
        /// </summary>
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.RegisterAttached("Rows", typeof(string), typeof(GridProps),
                new PropertyMetadata(null, OnRowsPropertyChanged));

        private static void OnRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetRowOrColumnDefinitions(
                d, e.NewValue,
                grid => grid.RowDefinitions.Clear(),
                (grid, gridLength) => grid.RowDefinitions.Add(new RowDefinition { Height = gridLength }));
        }

        private static void SetRowOrColumnDefinitions(DependencyObject obj, object newValue, Action<Grid> ClearDefs, Action<Grid, GridLength> AddDef)
        {
            if (!(obj is Grid grid))
                return;

            var lengthes = ParseLengthes(newValue.ToString());
            if (lengthes == null)
                return;

            ClearDefs(grid);
            foreach (var length in lengthes)
            {
                AddDef(grid, length);
            }
        }

        private static IEnumerable<GridLength> ParseLengthes(string lengthData)
        {
            var matchToConstructorList = new TupleList<string, RegexOptions, Func<string, GridLength>>
            {
                { @"\d+(\.\d+)?", RegexOptions.None, str=> new GridLength(double.Parse(str), GridUnitType.Pixel) },
                { @"\d+(\.\d+)?\*", RegexOptions.None, str=> new GridLength(double.Parse(str.Substring(0, str.Length - 1)), GridUnitType.Star) },
                { @"\*", RegexOptions.None, str=> new GridLength(1, GridUnitType.Star) },
                { @"Auto", RegexOptions.IgnoreCase, str=> new GridLength(1, GridUnitType.Auto) },
            };

            GridLength? ParseGridLength(string lengthString)
            {
                var matchToConstructor = matchToConstructorList.FirstOrDefault(mtc => Regex.IsMatch(lengthString, mtc.Item1, mtc.Item2));
                return matchToConstructor != null ? matchToConstructor.Item3(lengthString) : default(GridLength?);
            }

            var lengthStrings = lengthData.Split('|');
            var gridLengthes = lengthStrings.Select(lengthString => ParseGridLength(lengthString));

            if (gridLengthes.Any(gridLength => gridLength == null))
                return null;

            return gridLengthes.Select(length => (GridLength)length);
        }
    }

    public class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
    {
        public void Add(T1 item, T2 item2, T3 item3)
        {
            Add(new Tuple<T1, T2, T3>(item, item2, item3));
        }
    }
}
