//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace iDental
{
    using System;
    using System.Collections.Generic;
    
    public partial class Images
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Images()
        {
            this.Templates_Images = new HashSet<Templates_Images>();
        }
    
        public int Image_ID { get; set; }
        public string Image_Path { get; set; }
        public string Image_FileName { get; set; }
        public string Image_Extension { get; set; }
        public bool Image_IsEnable { get; set; }
        public int Registration_ID { get; set; }
        public System.DateTime CreateDate { get; set; }
    
        public virtual Registrations Registrations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Templates_Images> Templates_Images { get; set; }
    }
}
