using Shared.Contracts;

namespace CartService.Validator.Validators.OrderValidators
{
    public class UniqueItemIdValidator : BaseValidator<Order>
    {
        public override ValidationResult Handle(Order i_Order)
        {
            if (i_Order == null)
            {
                return ValidationResult.Failure("No object found");
            }

            else if (i_Order.Items == null || !i_Order.Items.Any())
            {
                return ValidationResult.Failure("No items found");
            }

            IEnumerable<string> allItemIds = i_Order.Items.Select(item => item.itemId).ToList();
            IEnumerable<string> distinctItemIds = allItemIds.Distinct();

            if (distinctItemIds.Count() != allItemIds.Count())
            {
                return ValidationResult.Failure("Item IDs must be unique within a single order");
            }

            return base.Handle(i_Order);
        }
    }
}
