using System.ComponentModel.DataAnnotations;

namespace Manager.Models.ModelValidation
{
    public static class ValidationExtensions
    {
        public static bool TryValidate(object model, out Dictionary<string, string[]> errors)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            errors = results
                .SelectMany(r => r.MemberNames.DefaultIfEmpty(string.Empty)
                    .Select(m => (Member: m, Message: r.ErrorMessage ?? string.Empty)))
                .GroupBy(t => t.Member)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => t.Message).Where(m => !string.IsNullOrWhiteSpace(m)).ToArray()
                );
            return isValid;
        }
    }
}
