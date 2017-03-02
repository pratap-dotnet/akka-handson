using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace charting_actors
{
    public class ChartingActor : ReceiveActor
    {
        #region Messages
        public class InitializeChart
        {
            public InitializeChart(Dictionary<string,Series> initialSeries)
            {
                InitialSeries = initialSeries;
            }
            public Dictionary<string,Series> InitialSeries { get; private set; }
        }

        public class AddSeries
        {
            public AddSeries(Series series)
            {
                Series = series;
            }
            public Series Series { get; private set; }
        }

        public class RemoveSeries
        {
            public RemoveSeries(string series)
            {
            }
            public string Series { get; private set; }
        }

        #endregion

        public const int MaxPoints = 250;
        private int xPosCounter = 0;

        private readonly Chart _chart;
        private Dictionary<string, Series> _seriesIndex;
        
        public ChartingActor(Chart chart) 
            : this(chart, new Dictionary<string,Series>())
        {

        }

        public ChartingActor(Chart chart, Dictionary<string, Series> dictionary)
        {
            this._seriesIndex = dictionary;
            this._chart = chart;

            Receive<InitializeChart>(m => HandleInitialize(m));
            Receive<AddSeries>(m => HandleAddSeries(m));
            Receive<RemoveSeries>(m => HandleRemoveSeries(m));
            Receive<Actors.Metric>(m => HandleMetrics(m));
        }

        private void HandleRemoveSeries(RemoveSeries m)
        {
            if(!string.IsNullOrEmpty(m.Series) && _seriesIndex.ContainsKey(m.Series))
            {
                _chart.Series.Remove(_seriesIndex[m.Series]);
                _seriesIndex.Remove(m.Series);
                SetChartBoundaries();
            }
        }

        private void HandleAddSeries(AddSeries series)
        {
            if(!string.IsNullOrEmpty(series?.Series?.Name) &&
                !_seriesIndex.ContainsKey(series.Series.Name))
            {
                _seriesIndex.Add(series.Series.Name, series.Series);
                _chart.Series.Add(series.Series);
                SetChartBoundaries();
            }
        }

        private void HandleInitialize(InitializeChart m)
        {
            if(m.InitialSeries != null)
            {
                _seriesIndex = m.InitialSeries;
            }
            _chart.Series.Clear();

            var area = _chart.ChartAreas[0];
            area.AxisX.IntervalType = DateTimeIntervalType.Number;
            area.AxisY.IntervalType = DateTimeIntervalType.Number;

            SetChartBoundaries();

            if (_seriesIndex.Any())
            {
                foreach (var series in _seriesIndex)
                {
                    series.Value.Name = series.Key;
                    _chart.Series.Add(series.Value);
                }
            }

            SetChartBoundaries();
        }

        private void HandleMetrics(Actors.Metric metric)
        {
            if (!string.IsNullOrEmpty(metric.Series) &&
                _seriesIndex.ContainsKey(metric.Series))
            {
                var series = _seriesIndex[metric.Series];
                series.Points.AddXY(xPosCounter++, metric.CounterValue);
                while (series.Points.Count > MaxPoints) series.Points.RemoveAt(0);
                SetChartBoundaries();
            }
        }

        private void SetChartBoundaries()
        {
            double maxAxisX, maxAxisY, minAxisX, minAxisY = 0.0d;
            var allPoints = _seriesIndex.Values.SelectMany(series => series.Points).ToList();
            var yValues = allPoints.SelectMany(point => point.YValues).ToList();
            maxAxisX = xPosCounter;
            minAxisX = xPosCounter - MaxPoints;
            maxAxisY = yValues.Count > 0 ? Math.Ceiling(yValues.Max()) : 1.0d;
            minAxisY = yValues.Count > 0 ? Math.Floor(yValues.Min()) : 0.0d;
            if (allPoints.Count > 2)
            {
                var area = _chart.ChartAreas[0];
                area.AxisX.Minimum = minAxisX;
                area.AxisX.Maximum = maxAxisX;
                area.AxisY.Minimum = minAxisY;
                area.AxisY.Maximum = maxAxisY;
            }
        }


    }
}
