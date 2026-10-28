using System.ComponentModel.DataAnnotations;

namespace CadetTest.Entities
{
    public class Consent
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Recipient { get; set; }
        public string Status { get; set; }
        public string RecipientType { get; set; }
    }
}
