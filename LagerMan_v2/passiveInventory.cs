//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LagerMan_v2
{
    using System;
    using System.Collections.Generic;
    
    public partial class passiveInventory
    {
        public int id { get; set; }
        public int supplier { get; set; }
        public Nullable<int> product { get; set; }
    
        public virtual productCatalog productCatalog { get; set; }
        public virtual suppliers suppliers { get; set; }
    }
}
