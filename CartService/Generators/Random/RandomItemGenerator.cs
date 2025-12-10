using Shared.Contracts;

namespace CartService.Generators.Random
{
    public class RandomItemGenerator : IItemGenerator
    {
        private static readonly System.Random Rnd = new System.Random();

        public List<Item> GenerateItems(int i_NumOfItems)
        {
            List<Item> items = new List<Item>();

            for (int i = 0; i < i_NumOfItems; i++)
            {
                items.Add(new Item 
                {
                    itemId = randomItemId(),
                    price = randomPrice(),
                    quantity = randomQuantity(),
                });
            }

            return items;
        }

        private string randomItemId()
        {
            int randomNum = Rnd.Next(1000, 9999);
            return $"ITEM-{randomNum}";
        }

        private decimal randomPrice()
        {
            decimal price = (decimal)(Rnd.NextDouble() * 495 + 5);
            return Math.Round(price, 2);
        }

        private int randomQuantity()
        {
            return Rnd.Next(1, 10);
        }
    }
}
