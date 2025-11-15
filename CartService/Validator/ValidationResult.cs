namespace CartService.Validator
{
    public class ValidationResult
    {
        public bool isValid {  get; set; }
        public string? ErrorMessage { get; set; }

        public static ValidationResult Success() =>  new ValidationResult { isValid = true };
        public static ValidationResult Failure(string i_Message) =>
            new ValidationResult { isValid = false, ErrorMessage = i_Message };
    }
}
