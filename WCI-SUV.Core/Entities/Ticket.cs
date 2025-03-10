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
    [Table("Ticket")]
    public class Ticket : AuditableEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("Ticket Number")]
        public Int32 TicketNumber   { get; set; }

        [Required]
        [Column("Ticket Size")]
        public long TicketSize      { get; set; }

        [Required]
        [Column("Slot Number")]
        public Int32 SlotNumber     { get; set; }   

    }
}
