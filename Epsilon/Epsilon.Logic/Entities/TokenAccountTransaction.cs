using Epsilon.Logic.JsonModels;
using System;

namespace Epsilon.Logic.Entities
{
    public class TokenAccountTransaction
    {
        public virtual long Id { get; set; }
        public virtual Guid UniqueId { get; set; }
        public virtual string AccountId { get; set; }
        public virtual string RewardTypeKey { get; set; }
        public virtual Decimal Amount { get; set; }
        public virtual int Quantity { get; set; }
        public virtual DateTimeOffset MadeOn { get; set; }

        public virtual Guid? InternalReference { get; set; }
        public virtual string ExternalReference { get; set; }

        public virtual TokenAccount Account { get; set; }
        public virtual TokenRewardType RewardType { get; set; }

        public MyTokenTransactionsItem ToItem()
        {
            return new MyTokenTransactionsItem
            {
                uniqueId = UniqueId,
                rewardTypeKey = RewardTypeKey,
                amount = Amount,
                quantity = Quantity,
                madeOn = MadeOn
            };
        }
    }
}
