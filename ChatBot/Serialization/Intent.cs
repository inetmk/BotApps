using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatBot.Serialization
{
    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }
}