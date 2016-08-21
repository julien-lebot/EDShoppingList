using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDShoppingList
{
    public class JsonSystem
    {
        [PrimaryKey]
        public int id { get; set; }
        public int edsm_id { get; set; }
        public string name { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public string faction { get; set; }
        public object population { get; set; }
        public string government { get; set; }
        public string allegiance { get; set; }
        public string state { get; set; }
        public string security { get; set; }
        public string primary_economy { get; set; }
        public string power { get; set; }
        public string power_state { get; set; }
        public int? needs_permit { get; set; }
        public int updated_at { get; set; }
        public string simbad_ref { get; set; }
        public int is_populated { get; set; }
        public string reserve_type { get; set; }
    }
}
