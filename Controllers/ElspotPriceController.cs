using System.Globalization;
using Elpriser.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;

namespace Elpriser.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ElspotPriceController : ControllerBase
	{
		private readonly ILogger<ElspotPriceController> _logger;
		public ElspotPriceController(ILogger<ElspotPriceController> logger)
		{
			_logger = logger;
		}

		[HttpGet("getstatus")]
		public async Task<ElspotResponseModel> GetStatus()
		{
			var client = new RestClient("https://api.energidataservice.dk/dataset/");

			var currentDate = DateTime.Now;

			var currentTime = DateTime.Parse(currentDate.ToString("yyyy-MM-dd HH:00:00"));

			var startDate = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day);
			var endDate = startDate.AddDays(2);

			var query = $"elspotprices?start={startDate:yyyy-MM-dd}&end={endDate:yyyy-MM-dd}";

			RestRequest request = new RestRequest(query + "&filter={\"PriceArea\":\"DK1\"}");
			var result = await client.GetAsync<Elspotprices>(request);

			if (result != null && result.records.Any())
			{
				var _start = startDate.AddDays(1);

				var recordsToday = result.records.Where(c => c.HourDK < _start).OrderBy(c => c.HourDK).ToList();
				var recordsTomorrow = result.records.Where(c => c.HourDK >= _start).OrderBy(c => c.HourDK).ToList();
				
				var todayMin = result.records.Where(c => c.HourUTC < _start).MinBy(c => c.SpotPriceEUR);
				var todayMax = result.records.Where(c => c.HourUTC < _start).MaxBy(c => c.SpotPriceEUR);

				var tomorrowMin = result.records.Where(c => c.HourUTC >= _start).MinBy(c => c.SpotPriceEUR);
				var tomorrowMax = result.records.Where(c => c.HourUTC >= _start).MaxBy(c => c.SpotPriceEUR);

				var current = result.records.FirstOrDefault(c => c.HourDK == currentTime);

				var ci = new CultureInfo("da-DK");

				var todaySpotPriceDKKCalculated = new List<float>();
				var todaySpotPriceBackgroundColor = new List<string>();

				var calcFactorNormal = 0.235f;
				var calcFactorHotTime = 0.347f;

				foreach (var item in recordsToday.ToList())
				{
					var calcResult = (float)Math.Round(item.SpotPriceDKK * calcFactorNormal, 0);
					
					if (item.HourDK.Month is >= 1 and < 4)
					{
						if (item.HourDK.Hour is >= 17 and < 20)
						{
							calcResult = (float)Math.Round(item.SpotPriceDKK * calcFactorHotTime, 0);
						}
					}

					todaySpotPriceDKKCalculated.Add(calcResult);

					if (calcResult < 150)
					{
						todaySpotPriceBackgroundColor.Add("green");
					}
					else if (calcResult >= 150 && calcResult < 300)
					{
						todaySpotPriceBackgroundColor.Add("yellow");
					}
					else
					{
						todaySpotPriceBackgroundColor.Add("red");
					}
				}

				var chartDataToday = new ElspotChartModel
				{
					Labels = recordsToday.Select(c => c.HourDK.ToString("HH:mm")).ToList(),
					Datasets = new List<ElspotChartDataModel>()
					{
						new()
						{
							Data = todaySpotPriceDKKCalculated,
							Label = "EL spot priser",
							BackgroundColor = todaySpotPriceBackgroundColor
						}
					}
				};

				var tomorrowSpotPriceDKKCalculated = new List<float>();
				var tomorrowSpotPriceBackgroundColor = new List<string>();

				foreach (var item in recordsTomorrow.ToList())
				{
					var calcResult = (float)Math.Round(item.SpotPriceDKK * calcFactorNormal, 0);
					
					if (item.HourDK.Month is >= 1 and < 4)
					{
						if (item.HourDK.Hour is >= 17 and < 20)
						{
							calcResult = (float)Math.Round(item.SpotPriceDKK * calcFactorHotTime, 0);
						}
					}

					tomorrowSpotPriceDKKCalculated.Add(calcResult);

					if (calcResult < 150)
					{
						tomorrowSpotPriceBackgroundColor.Add("green");
					}
					else if (calcResult >= 150 && calcResult < 300)
					{
						tomorrowSpotPriceBackgroundColor.Add("yellow");
					}
					else
					{
						tomorrowSpotPriceBackgroundColor.Add("red");
					}
				}

				var chartDataTomorrow = new ElspotChartModel
				{
					Labels = recordsTomorrow.Select(c => c.HourDK.ToString("HH:mm")).ToList(),
					Datasets = new List<ElspotChartDataModel>()
					{
						new()
						{
							Data = tomorrowSpotPriceDKKCalculated,
							Label = "EL spot priser",
							BackgroundColor = tomorrowSpotPriceBackgroundColor
						}
					}
				};

				var currentPrice = "-";

				if (current != null)
				{
					currentPrice = Math.Round(current.SpotPriceDKK * calcFactorNormal, 0).ToString("N2");
					
					if (current.HourDK.Month is >= 1 and < 4)
					{
						if (current.HourDK.Hour is >= 17 and < 20)
						{
							currentPrice = Math.Round(current.SpotPriceDKK * calcFactorHotTime, 0).ToString("N2");
						}
					}
				}

				
				return new ElspotResponseModel
				{
					ChartDataToday = chartDataToday,
					ChartDataTomorrow = chartDataTomorrow,
					TodayDay = startDate.ToString("D", ci),
					TodayBadTime = string.Concat(todayMax.HourDK.AddHours(-1).ToString("HH:00"), " - ",
						todayMax.HourDK.AddHours(1).ToString("HH:59")),
					TodayBadPrice = (todayMax.SpotPriceDKK * 0.128f).ToString("N2"),
					TodayBestTime = string.Concat(todayMin.HourDK.AddHours(-1).ToString("HH:00"), " - ",
						todayMin.HourDK.AddHours(1).ToString("HH:59")),
					TodayBestPrice = (todayMin.SpotPriceDKK * 0.128f).ToString("N2"),
					TomorrowDay = endDate.AddDays(-1).ToString("D", ci),
					TomorrowBadTime = tomorrowMax != null
						? string.Concat(tomorrowMax.HourDK.AddHours(-1).ToString("HH:00"), " - ",
							tomorrowMax.HourDK.AddHours(1).ToString("HH:59"))
						: "-",
					TomorrowBadPrice = tomorrowMax != null ? (tomorrowMax.SpotPriceDKK * 0.128f).ToString("N2") : "-",
					TomorrowBestTime = tomorrowMax != null
						? string.Concat(tomorrowMin.HourDK.AddHours(-1).ToString("HH:00"), " - ",
							tomorrowMin.HourDK.AddHours(1).ToString("HH:59"))
						: "-",
					TomorrowBestPrice = tomorrowMax != null ? (tomorrowMin.SpotPriceDKK * 0.128f).ToString("N2") : "-",
					CurrentPrice = currentPrice
				};
			}

			return null;
		}
	}
}
