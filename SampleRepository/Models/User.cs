using System.Collections.Generic;

namespace SampleRepository.Models
{
    public class User : BaseEntity
    {
        public User()
        {
            Permissions = new List<Permission>();
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }

        public ICollection<Permission> Permissions { get; set; }
    }
}
