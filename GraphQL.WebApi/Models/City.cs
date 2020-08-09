using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQL.WebApi.Models
{
    [Table("city", Schema = "business")]
    public partial class City: BaseEntity
    {
        public string name { get; set; }
        public int? population { get; set; }
        public int country_id { get; set; }

        [ForeignKey("country_id")]
        public Country Country { get; set; }
    }
}
