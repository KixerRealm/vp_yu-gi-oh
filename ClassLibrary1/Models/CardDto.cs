﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Middleware.Models
{
    public class CardDto
    {
        public string id { get; set; }
        public string cardId { get; set; }
        public string type { get; set; }//enum
        public string name { get; set; }
        public string description { get; set; }
        public string subType { get; set; }//enum
        public int? atk { get; set; }
        public int? def { get; set; }

        [JsonIgnore]
        public Image img { get; set; }

        [JsonIgnore]
        public int position { get; set; }

        public override string ToString()
        {
            return string.Format("{0}",name);
        }
        public CardDto Clone()
        {
            return new()
            {
                id = this.id,
                cardId = this.cardId,
                type = this.type,
                name = this.name,
                description = this.description,
                subType = this.subType,
                atk = this.atk,
                def = this.def,
                img =  this.img != null ? this.img.Clone() as Image : null
            };
        }

        public Image CloneImage()
        {
            return img != null ? new Bitmap(img.Clone() as Image) : null;
        }
       
    }
}
