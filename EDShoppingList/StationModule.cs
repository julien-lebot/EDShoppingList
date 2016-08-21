using ServiceStack.DataAnnotations;

namespace EDShoppingList
{
    public class StationModule
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int StationId { get; set; }
        public int ModuleId { get; set; }
    }
}
