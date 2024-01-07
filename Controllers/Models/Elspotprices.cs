namespace Elpriser.Controllers.Models
{
	public class Elspotprices
	{
		public int total { get; set; }
		public Record[] records { get; set; }
	}
	
	public class Record
	{
		public DateTime HourUTC { get; set; }
		public DateTime HourDK { get; set; }
		public float SpotPriceDKK { get; set; }
		public float SpotPriceEUR { get; set; }
	}

}
