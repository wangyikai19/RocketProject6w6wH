namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CollectStores
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int MemberId { get; set; }

        public DateTime? CreateTime { get; set; }

        public virtual Members Members { get; set; }

        public virtual Stores Stores { get; set; }
    }
}
