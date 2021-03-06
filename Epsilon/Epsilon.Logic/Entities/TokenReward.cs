﻿using Epsilon.Logic.Constants.Enums;
using Epsilon.Logic.Helpers;
using Epsilon.Logic.JsonModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Epsilon.Logic.Entities
{
    public class TokenReward
    {
        public virtual int SchemeId { get; set; }
        public virtual string TypeKey { get; set; }
        public virtual decimal Value { get; set; }

        public virtual TokenRewardScheme Scheme { get; set; }
        public virtual TokenRewardType Type { get; set; }

        [NotMapped]
        public virtual TokenRewardKey? TypeKeyAsEnum
        {
            get
            {
                return EnumsHelper.TokenRewardKey.Parse(TypeKey);
            }
        }

        [NotMapped]
        public virtual decimal AbsValue
        {
            get
            {
                return Math.Abs(Value);
            }
        }

        public TokenRewardTypeValue ToTokeRewardTypeValue()
        {
            return new TokenRewardTypeValue
            {
                key = TypeKey,
                value = Value
            };
        }
    }
}
