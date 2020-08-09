using System.ComponentModel.DataAnnotations.Schema;

namespace GraphQL.WebApi.Models
{
    [Table("country", Schema = "business")]
    public partial class Country : BaseEntity
    {
        public string name { get; set; }
        public string continent { get; set; }
    }
}
