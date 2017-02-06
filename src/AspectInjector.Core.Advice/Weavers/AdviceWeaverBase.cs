﻿using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using static AspectInjector.Broker.Advice.Argument;
using AspectInjector.Core.Fluent;
using static AspectInjector.Broker.Advice;

namespace AspectInjector.Core.Advice.Weavers
{
    public abstract class AdviceWeaverBase<TEffect> : EffectWeaverBase<IMemberDefinition, TEffect>
        where TEffect : AdviceEffectBase
    {
        public AdviceWeaverBase(ILogger logger) : base(logger)
        {
        }

        protected override bool CanWeave(Injection injection)
        {
            return base.CanWeave(injection) &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);
        }

        protected override void Weave(IMemberDefinition target, TEffect effect, Injection injection)
        {
            if (target is EventDefinition)
            {
                WeaveTargetEvent((EventDefinition)target, effect, injection);
                return;
            }

            if (target is PropertyDefinition)
            {
                WeaveTargetProperty((PropertyDefinition)target, effect, injection);
                return;
            }

            if (target is MethodDefinition)
            {
                WeaveTargetMethod((MethodDefinition)target, effect, injection);
                return;
            }

            _log.LogError(CompilationMessage.From($"Unsupported target {target.GetType().Name}", target));
        }

        protected abstract void WeaveMethod(MethodDefinition target, TEffect effect, Injection injection);

        protected virtual void WeaveTargetProperty(PropertyDefinition target, TEffect effect, Injection injection)
        {
            if (target.SetMethod != null && effect.Target.HasFlag(Target.Setter))
                WeaveMethod(target.SetMethod, effect, injection);

            if (target.GetMethod != null && effect.Target.HasFlag(Target.Getter))
                WeaveMethod(target.GetMethod, effect, injection);
        }

        protected virtual void WeaveTargetMethod(MethodDefinition target, TEffect effect, Injection injection)
        {
            if (target.IsConstructor && effect.Target.HasFlag(Target.Constructor))
                WeaveMethod(target, effect, injection);

            if (!target.IsConstructor && !target.IsSetter && !target.IsGetter && !target.IsAddOn && !target.IsRemoveOn && effect.Target.HasFlag(Target.Method))
                WeaveMethod(target, effect, injection);
        }

        protected virtual void WeaveTargetEvent(EventDefinition target, TEffect effect, Injection injection)
        {
            if (target.AddMethod != null && effect.Target.HasFlag(Target.EventAdd))
                WeaveMethod(target.AddMethod, effect, injection);

            if (target.RemoveMethod != null && effect.Target.HasFlag(Target.EventRemove))
                WeaveMethod(target.RemoveMethod, effect, injection);
        }

        protected void LoadAdviceArgs(PointCut pc, MethodDefinition target, AdviceEffectBase effect, Injection injection)
        {
            foreach (var arg in effect.Arguments.OrderBy(a => a.Parameter.Index))
            {
                switch (arg.Source)
                {
                    case Source.Arguments: LoadArgumentsArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Attributes: LoadAttributesArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Instance: LoadInstanceArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Method: LoadMethodArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Name: LoadNameArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.ReturnType: LoadReturnTypeArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.ReturnValue: LoadReturnValueArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Target: LoadTargetArgument(pc, effect, target, arg.Parameter, injection); break;
                    case Source.Type: LoadTypeArgument(pc, effect, target, arg.Parameter, injection); break;
                    default: _log.LogError(CompilationMessage.From($"Unknown argument source {arg.Source.ToString()}", target)); break;
                }
            }
        }

        protected virtual void LoadTypeArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.TypeOf(target.DeclaringType);
        }

        protected virtual void LoadTargetArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.Null();
        }

        protected virtual void LoadReturnValueArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.Null();
        }

        protected virtual void LoadReturnTypeArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.TypeOf(target.ReturnType);
        }

        protected virtual void LoadMethodArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.MethodOf(target).Cast(target.Module.GetTypeSystem().MethodBase);
        }

        protected virtual void LoadInstanceArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            if (target.IsStatic)
                pc.Null();
            else
                pc.This();
        }

        protected virtual void LoadAttributesArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.Null();
        }

        protected virtual void LoadArgumentsArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.Null();
        }

        protected virtual void LoadNameArgument(PointCut pc, AdviceEffectBase effect, MethodDefinition target, ParameterDefinition parameter, Injection injection)
        {
            pc.Value(((IMemberDefinition)injection.Target).Name);
        }
    }
}