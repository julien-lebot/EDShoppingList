using ReactiveUI;

namespace EDShoppingList
{
    public class ModuleSearchResponse : ReactiveObject
    {
        public string System { get; set; }
        public string Station { get; set; }
        public int? DistanceToStar { get; set; }
        public string Module { get; set; }
        public string Category { get; set; }
        public int Class { get; set; }
        public string Rating { get; set; }
        public int? Price { get; set; }
        public double DistanceToSystem { get; set; }
    }
}
