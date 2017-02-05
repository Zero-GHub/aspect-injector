﻿using Mono.Cecil;

namespace AspectInjector.Core.Advice.Advices
{
    public class AroundAdvice : AdviceBase
    {
        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }
    }
}