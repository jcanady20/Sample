using System;

namespace SampleRepository.Models
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Created = DateTime.Now;
        }
        public string Id { get; set; }
        public DateTime Created { get; set; }
    }
}
