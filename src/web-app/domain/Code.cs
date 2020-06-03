﻿using System;
using System.Collections.Generic;
using System.Text;


namespace CodeJar.Domain
{
    public class Code
    {
        public int Id {get; set;}
        public int BatchId {get; set;}
        public string StringValue { get; set; }
        public int SeedValue {get; set;}
        public byte State { get; set; }
       
    }
}
