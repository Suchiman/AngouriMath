﻿using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    /// <summary>
    /// It is a storage of default constant converters that you can use
    /// </summary>
    public static class CompilationProtocolBuiltinConstantConverters
    {
        private static object CastToT<T>(Number num)
        {
            if (typeof(T) == typeof(Complex))
                return (Complex)num;
            if (typeof(T) == typeof(double))
                return (double)num;
            if (typeof(T) == typeof(float))
                return (float)num;
            if (typeof(T) == typeof(int))
                return (int)num;
            if (typeof(T) == typeof(long))
                return (long)num;
            if (typeof(T) == typeof(BigInteger))
                return (BigInteger)num;
            throw new InvalidProtocolProvided("Undefined type, provide valid compilation protocol");
        }

        /// <summary>
        /// This treats any number as <see cref="Complex"/> and any boolean as <see cref="bool"/>
        /// </summary>
        public static Func<Entity, Expression> CreateConverterConstant<TNumeric>()
            => e => e switch
            {
                Number n => Expression.Constant(CastToT<TNumeric>(n)),
                Entity.Boolean b => Expression.Constant((bool)b),
                _ => throw new AngouriBugException("Undefined constant type")
            };

        private static Type[] GenerateArrayOfType(int argCount, Type type)
        {
            var l = new List<Type>();
            for (int i = 0; i < argCount; i++)
                l.Add(type);
            return l.ToArray();
        }

        private static MethodInfo GetDef(string name, int argCount, Type type)
            => typeof(MathAllMethods).GetMethod(name, GenerateArrayOfType(argCount, type));            

        /// <summary>
        /// This is a default converter for binary nodes (for those inherited from <see cref="ITwoArgumentNode"/>)
        /// </summary>
        public static Func<Expression, Expression, Entity, Expression> CreateTwoArgumentEntity<T>()
            => (left, right, typeHolder) => typeHolder switch
            {
                Sumf => Expression.Add(left, right),
                Minusf => Expression.Subtract(left, right),
                Mulf => Expression.Multiply(left, right),
                Divf => Expression.Divide(left, right),
                Powf => Expression.Call(GetDef("Pow", 2, left.Type), left, right),
                Logf => Expression.Call(GetDef("Log", 2, right.Type), left, right),

                Andf => Expression.And(left, right),
                Orf => Expression.Or(left, right),
                Xorf => Expression.ExclusiveOr(left, right),
                Impliesf => Expression.Or(Expression.Not(left), right),

                Lessf => Expression.LessThan(left, right),
                LessOrEqualf => Expression.LessThanOrEqual(left, right),
                Greaterf => Expression.GreaterThan(left, right),
                GreaterOrEqualf => Expression.GreaterThanOrEqual(left, right),
                Equalsf => Expression.Equal(left, right),

                _ => throw new AngouriBugException("A node seems to be not added")
            };

        /// <summary>
        /// This is a default converter for unary nodes (for those inherited from <see cref="IOneArgumentNode"/>)
        /// </summary>
        public static Func<Expression, Entity, Expression> CreateOneArgumentEntity<T>()
            => (e, typeHolder) => typeHolder switch
            {
                Sinf =>         Expression.Call(GetDef("Sin", 1, e.Type), e),
                Cosf =>         Expression.Call(GetDef("Cos", 1, e.Type), e),
                Tanf =>         Expression.Call(GetDef("Tan", 1, e.Type), e),
                Cotanf =>       Expression.Call(GetDef("Cot", 1, e.Type), e),
                Secantf =>      Expression.Call(GetDef("Sec", 1, e.Type), e),
                Cosecantf =>    Expression.Call(GetDef("Csc", 1, e.Type), e),

                Arcsinf =>      Expression.Call(GetDef("Asin", 1, e.Type), e),
                Arccosf =>      Expression.Call(GetDef("Acos", 1, e.Type), e),
                Arctanf =>      Expression.Call(GetDef("Atan", 1, e.Type), e),
                Arccotanf =>    Expression.Call(GetDef("Acot", 1, e.Type), e),
                Arcsecantf =>   Expression.Call(GetDef("Asec", 1, e.Type), e),
                Arccosecantf => Expression.Call(GetDef("Acsc", 1, e.Type), e),

                Absf =>         Expression.Call(GetDef("Abs", 1, e.Type), e),
                Signumf =>      Expression.Call(GetDef("Sgn", 1, e.Type), e),

                Notf =>         Expression.Not(e),

                _ => throw new AngouriBugException("A node seems to be not added")
            };

        /// <summary>
        /// This is a default converter for other (non-unary and non-binary) nodes
        /// </summary>
        public static Func<IEnumerable<Expression>, Entity, Expression> CreateAnyArgumentEntity<T>()
            => (en, typeHolder) => typeHolder switch
            {
                // TODO: finite set -> hash set
                _ => throw new AngouriBugException("A node seems to be not added")
                //FiniteSet => Expression.
            };
    }
}
