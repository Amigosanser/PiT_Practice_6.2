//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Courswork
{
    using System;
    using System.Collections.Generic;
    
    public partial class Achievements
    {
        public int AchievementID { get; set; }
        public int ChildID { get; set; }
        public string Description { get; set; }
        public System.DateTime Date { get; set; }
    
        public virtual Children Children { get; set; }
    }
}
