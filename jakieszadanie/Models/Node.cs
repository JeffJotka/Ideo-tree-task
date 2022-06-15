using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace jakieszadanie.Models
{
    public class Node
    {
        [Key]
        [Display(Name = "Ajdi")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Parent")]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual Node? Parent { get; set; }

        public virtual ICollection<Node>? Children { get; set; }
    }
    public class DelAndMove
    {
        [Key]
        [Required()]
        public int NodeId { get; set; }

        [Required()]
        public int TargetId { get; set; }
    }
}

