namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CommnetReports
    {
        public int Id { get; set; }

        public int CommentId { get; set; }

        public int ReportUserId { get; set; }

        [StringLength(50)]
        public string Comment { get; set; }

        public DateTime? CreateTime { get; set; }

        public int? Member_Id { get; set; }

        public string Type { get; set; }

        public virtual Members Members { get; set; }

        public virtual StoreComments StoreComments { get; set; }
    }
}
