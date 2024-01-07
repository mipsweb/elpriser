namespace Elpriser.Controllers.Models
{
    public class ElspotResponseModel
    {
        public string TodayDay { get; set; }
        public string TodayBestTime { get; set; }
        public string TodayBadTime { get; set; }
        public string TomorrowDay { get; set; }
        public string TomorrowBestTime { get; set; }
        public string TomorrowBadTime { get; set; }
        public string TodayBadPrice { get; set; }
        public string TodayBestPrice { get; set; }
        public string TomorrowBadPrice { get; set; }
        public string TomorrowBestPrice { get; set; }
        public string CurrentPrice { get; set; }
        public ElspotChartModel ChartDataToday { get; set; }
        public ElspotChartModel ChartDataTomorrow { get; set; }
    }

    public class ElspotChartModel
    {
        public IList<string> Labels { get; set; }
        public IList<ElspotChartDataModel> Datasets { get; set; }
    }

    public class ElspotChartDataModel
    {
        public IList<float> Data { get; set; }
        public string Label { get; set; }
        public List<string> BackgroundColor { get; set; }
    }
}
