using Epsilon.Logic.Constants.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Dtos
{
    public class UiAlert
    {
        public UiAlertType Type { get; set; }
        public string Message { get; set; }
    }
}
