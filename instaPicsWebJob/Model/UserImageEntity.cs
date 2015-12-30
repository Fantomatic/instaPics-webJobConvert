using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace instaPicsWebJob.Model
{
    public class UserImageEntity : TableEntity
    {
        public string user { get; set; }

        public string imgOriginal { get; set; }

        public string imgOriginalThumb { get; set; }

        public string imgBN { get; set; }

        public string imgBNThumb { get; set; }
    }
}
