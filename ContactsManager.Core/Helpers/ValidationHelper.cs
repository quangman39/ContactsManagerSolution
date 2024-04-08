using System.ComponentModel.DataAnnotations;

namespace Services.Helpers
{
    public class ValidationHelper
    {
        public static void ModelValition(object obj)
        {
            //Model validations 
            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, results, true);


            //Validate Name of PersonAdd
            if (!isValid)
            {
                throw new ArgumentException(results.FirstOrDefault()?.ErrorMessage);

            }
        }

    }
}
