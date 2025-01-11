namespace Project6w6wH.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Replies
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Replies()
        {
            Notifies = new HashSet<Notifies>();
            ReplyLikes = new HashSet<ReplyLikes>();
        }

        public int Id { get; set; }

        public int CommentId { get; set; }

        public int ReplyUserId { get; set; }

        public string ReplyContent { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }

        public string ReplyOnlyCode { get; set; }

        public virtual Members Members { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notifies> Notifies { get; set; }

        public virtual StoreComments StoreComments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReplyLikes> ReplyLikes { get; set; }
    }
}
