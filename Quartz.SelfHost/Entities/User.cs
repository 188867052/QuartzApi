using System;

namespace Entities
{
    public partial class User
    {
        public int Id { get; set; }

        public string LoginName { get; set; }

        public string DisplayName { get; set; }

        public string Password { get; set; }

        public bool IsLocked { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public bool IsEnable { get; set; }

        public bool IsDeleted { get; set; }
    }
}
