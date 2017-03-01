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
        #endregion

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
        }
        
        private void HandleAddSeries(AddSeries series)
        {
            if(!string.IsNullOrEmpty(series?.Series?.Name) &&
                !_seriesIndex.ContainsKey(series.Series.Name))
            {
                _seriesIndex.Add(series.Series.Name, series.Series);
                _chart.Series.Add(series.Series);
            }
        }

        private void HandleInitialize(InitializeChart m)
        {
            if(m.InitialSeries != null)
            {
                _seriesIndex = m.InitialSeries;
            }
            _chart.Series.Clear();
            if (_seriesIndex.Any())
            {
                foreach (var series in _seriesIndex)
                {
                    series.Value.Name = series.Key;
                    _chart.Series.Add(series.Value);
                }
            }
        }
    }
}
