using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RadioCab.Validations
{
         public class SalaryRangeAttribute : ValidationAttribute
        {
            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                if (value == null)
                    return ValidationResult.Success; 

                var salary = value.ToString()!.Trim();

              
                var regex = new Regex(@"^\d+(\s*-\s*\d+)?$");

                if (!regex.IsMatch(salary))
                {
                    return new ValidationResult("Salary must be a positive number or range (e.g., 5000-10000).");
                }

                var parts = salary.Split('-');
                if (parts.Length == 2)
                {
                    int first = int.Parse(parts[0].Trim());
                    int second = int.Parse(parts[1].Trim());

                    if (first > second)
                    {
                        return new ValidationResult("In salary range, the first number must be less than or equal to the second number.");
                    }
                }

                return ValidationResult.Success;
            }
        }
    }

