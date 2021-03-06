﻿using PrecizeSoft.IO.Contracts.ConversionStatistics;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrecizeSoft.IO.Wcf.DataContracts.ConversionStatistics.V1
{
    [DataContract(Namespace = "http://io.precizesoft.com/ConversionStatistics/V1/")]
    public class StatByHour: IStatByHour
    {
        [DataMember]
        public int Hour { get; set; }

        [DataMember]
        public int TotalCount { get; set; }

        [DataMember]
        public long FileSizeSum { get; set; }

        [DataMember]
        public long ResultFileSizeSum { get; set; }

        [DataMember]
        public long TotalFileSizeSum { get; set; }
    }
}
