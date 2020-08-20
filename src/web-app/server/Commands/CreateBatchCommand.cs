using System;
using System.ComponentModel.DataAnnotations;

namespace CodeJar.WebApp.Commands
{
    public class CreateBatchCommand
    {
        [Required]
        [StringLength(50)]
        public string BatchName { get; set; }

        [Required]
        [Range(1, 10000)]
        public int BatchSize { get; set; }

        [Required]
        [StringLength(50)]
        public string PromotionType { get; set; }

        [Required]
        public DateTime DateActive { get; set; }

        [Required]
        public DateTime DateExpires { get; set; }
    }
}