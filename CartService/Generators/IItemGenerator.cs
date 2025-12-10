using Shared.Contracts;

namespace CartService.Generators
{
    public interface IItemGenerator
    {
        List<Item> GenerateItems(int i_NumOfItems);
    }
}
