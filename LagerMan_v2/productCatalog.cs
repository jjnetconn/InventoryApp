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
    
    public partial class productCatalog
    {
        public productCatalog()
        {
            this.activeInventory = new HashSet<activeInventory>();
            this.inverters = new HashSet<inverters>();
            this.panels = new HashSet<panels>();
            this.passiveInventory = new HashSet<passiveInventory>();
        }
    
        public int id { get; set; }
        public int supplier { get; set; }
        public string prCname { get; set; }
        public bool prActive { get; set; }
        public Nullable<int> prNumber { get; set; }
        public Nullable<int> prGroup { get; set; }
        public string prShortName { get; set; }
    
        public virtual ICollection<activeInventory> activeInventory { get; set; }
        public virtual ICollection<inverters> inverters { get; set; }
        public virtual ICollection<panels> panels { get; set; }
        public virtual ICollection<passiveInventory> passiveInventory { get; set; }
        public virtual suppliers suppliers { get; set; }
    }
}