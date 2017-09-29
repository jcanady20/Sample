﻿using System.Collections.Generic;

namespace Mongo.Context.Mapping
{
    public class MongoIndex
    {
        public MongoIndex()
        {
            Keys = new List<string>();
            TimeToLive = -1;
        }
        public ICollection<string> Keys { get; set; }
        public bool Desending { get; set; }
        public bool Unique { get; set; }
        public double TimeToLive { get; set; }
    }
}
