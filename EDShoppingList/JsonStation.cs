using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace EDShoppingList
{
    public class JsonStation
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public int system_id { get; set; }
        public string max_landing_pad_size { get; set; }
        public int? distance_to_star { get; set; }
        public string faction { get; set; }
        public string government { get; set; }
        public string allegiance { get; set; }
        public string state { get; set; }
        public int? type_id { get; set; }
        public string type { get; set; }
        public int? has_blackmarket { get; set; }
        public int? has_market { get; set; }
        public int? has_refuel { get; set; }
        public int? has_repair { get; set; }
        public int? has_rearm { get; set; }
        public int? has_outfitting { get; set; }
        public int? has_shipyard { get; set; }
        public int? has_docking { get; set; }
        public int? has_commodities { get; set; }
        public List<object> import_commodities { get; set; }
        public List<object> export_commodities { get; set; }
        public List<object> prohibited_commodities { get; set; }
        public List<string> economies { get; set; }
        public int updated_at { get; set; }
        public int? shipyard_updated_at { get; set; }
        public int? outfitting_updated_at { get; set; }
        public int? market_updated_at { get; set; }
        public int is_planetary { get; set; }
        public List<string> selling_ships { get; set; }
        [Ignore]
        public List<int> selling_modules { get; set; }
    }
}
