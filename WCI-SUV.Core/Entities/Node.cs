using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WCI_SUV.Core.Models;

namespace WCI_SUV.Core.Entities
{
    [Table("Nodes")]
    public class Node : AuditableEntity
    {
        [Key]
        [Required]
        [Column("Name")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }

        [Required]
        [Column("Namespace")]
        public Int16 Namespace { get; set; }

        [Required]
        [Column("Register")]
        public uint Register { get; set; }

        [Required]
        [Column("Type")]
        public string Type { get; set; }

        // Audit properties
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


    }
}
