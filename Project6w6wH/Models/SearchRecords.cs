namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class SearchRecords
    {
        public int Id { get; set; }

        public string Label { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
