using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDShoppingList
{
    public class JsonModule
    {
        [PrimaryKey]
        public int id { get; set; }
        public int group_id { get; set; }
        public int @class { get; set; }
        public string rating { get; set; }
        public int? price { get; set; }
        public string weapon_mode { get; set; }
        public int? missile_type { get; set; }
        public string name { get; set; }
        public int? belongs_to { get; set; }
        public int? ed_id { get; set; }
        public string ship { get; set; }
        public JsonCategory group { get; set; }
        public double? mass { get; set; }
        public int? dps { get; set; }
        public double? power { get; set; }
        public int? damage { get; set; }
        public int? ammo { get; set; }
        public double? range_km { get; set; }
        public string efficiency { get; set; }
        public double? power_produced { get; set; }
        public int? duration { get; set; }
        public int? cells { get; set; }
        public string recharge_rating { get; set; }
        public int? capacity { get; set; }
        public int? count { get; set; }
        public int? range_ls { get; set; }
        public int? rate { get; set; }
        public int? bins { get; set; }
        public int? additional_armour { get; set; }
        public int? vehicle_count { get; set; }
    }
}
