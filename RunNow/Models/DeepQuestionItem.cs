
﻿using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunNow.Models
{
    public partial class DeepQuestionItem : ObservableObject
    {
        public string Id { get; set; }
        public string QuestionText { get; set; }
        public string Category { get; set; }

        [ObservableProperty]
        private int? selectedValue;
    }

}

