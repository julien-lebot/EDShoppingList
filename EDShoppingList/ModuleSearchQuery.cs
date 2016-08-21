using ReactiveUI;

namespace EDShoppingList
{
    public class ModuleSearchQuery : ReactiveObject
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public int? Class { get; set; }
        public string Rating { get; set; }
    }
}
