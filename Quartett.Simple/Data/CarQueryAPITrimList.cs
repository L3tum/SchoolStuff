#region usings

using System.Collections.Generic;

#endregion

namespace Quartett.Simple.Data
{
	public class Trim
	{
		public string model_id { get; set; }
		public string model_make_id { get; set; }
		public string model_name { get; set; }
		public string model_trim { get; set; }
		public string model_year { get; set; }
		public string model_body { get; set; }
		public string model_engine_position { get; set; }
		public string model_engine_cc { get; set; }
		public string model_engine_cyl { get; set; }
		public string model_engine_type { get; set; }
		public string model_engine_valves_per_cyl { get; set; }
		public string model_engine_power_ps { get; set; }
		public string model_engine_power_rpm { get; set; }
		public string model_engine_torque_nm { get; set; }
		public string model_engine_torque_rpm { get; set; }
		public string model_engine_bore_mm { get; set; }
		public string model_engine_stroke_mm { get; set; }
		public string model_engine_compression { get; set; }
		public string model_engine_fuel { get; set; }
		public string model_top_speed_kph { get; set; }
		public string model_0_to_100_kph { get; set; }
		public string model_drive { get; set; }
		public string model_transmission_type { get; set; }
		public string model_seats { get; set; }
		public string model_doors { get; set; }
		public string model_weight_kg { get; set; }
		public string model_length_mm { get; set; }
		public string model_width_mm { get; set; }
		public string model_height_mm { get; set; }
		public string model_wheelbase_mm { get; set; }
		public string model_lkm_hwy { get; set; }
		public string model_lkm_mixed { get; set; }
		public string model_lkm_city { get; set; }
		public string model_fuel_cap_l { get; set; }
		public string model_sold_in_us { get; set; }
		public object model_co2 { get; set; }
		public string model_make_display { get; set; }
		public string make_display { get; set; }
		public string make_country { get; set; }
	}

	public class TrimList
	{
		public List<Trim> Trims { get; set; }
	}
}