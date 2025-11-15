using Shared.Contracts;

namespace OrderService
{
    public class ShippingCostService
    {
        public const int k_Percent = 2;

        public static decimal CalculateShippingCost(Order i_Order)
        {
            return (i_Order.TotalAmount * k_Percent) / 100; 
        }
    }
}
