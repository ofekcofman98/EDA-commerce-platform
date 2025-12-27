using Shared.Contracts;
using Shared.Contracts.Orders;

namespace CartService.Generators.Random
{
    public class RandomOrderGenerator : BaseOrderGenerator
    {
        private static readonly System.Random Rnd = new System.Random();

        public override Order GenerateOrder(CreateOrderRequest i_Request, List<Item> i_Items)
        {
            Order order = base.GenerateOrder(i_Request, i_Items);

            order.CustomerId = randomCustomerId();
            order.Currency = randomCurrency();
            
            return order;
        }

        private string randomCustomerId()
        {
            return $"CUST-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        
        private string randomCurrency()
        {
            var array = Currencies.s_AllowedCurrencies.ToArray();
            int index = Rnd.Next(array.Length);
            return array[index];
        }

    }
}
