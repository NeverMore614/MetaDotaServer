using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetaDotaServer.Entity
{
    [Table("Token")]
    public class Token
    {
        [Key]
        public string TokenStr { get; set; }

        public int Id { get; set; }
    }
}
