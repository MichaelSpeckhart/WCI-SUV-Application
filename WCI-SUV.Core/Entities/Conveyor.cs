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
    [Table("Conveyor")]
    public class Conveyor : AuditableEntity
    {
        [Key]
        [Required]
        [Column("Account Number")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Int32 AccountNumber  { get; set; }

        [Required]
        [Column("Ticket Number")]
        public Int32 TicketNumber   { get; set; }

        [Required]
        [Column("Slot Number")]
        public Int32 SlotNumber     { get; set; }

        [Required]
        [Column("Garment Number")]
        public Int32 GarmentNumber  { get; set; }

        [Required]
        [Column("Employee Number")]
        public Int32 EmployeeNumber { get; set; }

        [Column("Ticket Size")]
        public long TicketSize      { get; set; }

    }
}
