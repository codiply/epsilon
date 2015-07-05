using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epsilon.Logic.Entities
{
    public class CoinAccountTransactionType : BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual string Description { get; set; }

        [NotMapped]
        public virtual CoinAccountTransactionTypeId? IdAsEnum
        {
            get
            {
                return EnumsHelper.CoinAccountTransactionTypeId.Parse(Id);
            }
        }
    }
}
