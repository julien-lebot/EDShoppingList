using ServiceStack.DataAnnotations;

namespace EDShoppingList
{
    public class JsonCategory
    {
        [PrimaryKey]
        public int id { get; set; }
        public int category_id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
    }
}
