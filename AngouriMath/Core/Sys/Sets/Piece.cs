﻿
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    // First bool is whether the edge is closed for Re(Entity)
    // Second bool is whether the edge is closed for Im(Entity)
    using Edge = Tuple<Entity, bool, bool>;

    public abstract class Piece
    {
        public enum PieceType
        {
            ENTITY,
            INTERVAL,
        }

        public PieceType Type { get; private set; }

        public static bool operator ==(Piece a, Piece b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.Type != b.Type)
                return false;
            if (a.Type == PieceType.ENTITY)
                return a as OneElementPiece == b as OneElementPiece;
            else
                return a as IntervalPiece == b as IntervalPiece;
        }

        public static bool operator !=(Piece a, Piece b)
            => !(a == b);

        /// <summary>
        /// See Edge definition above in this file
        /// </summary>
        /// <returns></returns>
        public abstract Edge UpperBound();

        /// <summary>
        /// See Edge definition above in this file
        /// </summary>
        /// <returns></returns>
        public abstract Edge LowerBound();

        /// <summary>
        /// True if num is in between a, b
        /// </summary>
        /// <param name="a">
        /// one bound
        /// </param>
        /// <param name="b">
        /// another bound (if a > b, they swap)
        /// </param>
        /// <param name="closedA">
        /// whether a inclusive
        /// </param>
        /// <param name="closedB">
        /// whether b inclusive
        /// </param>
        /// <param name="closedNum">
        /// if false, then
        /// (2 is in (2; 3)
        /// </param>
        /// <param name="num"></param>
        /// <returns></returns>
        private static bool InBetween(double a, double b, bool closedA, bool closedB, double num, bool closedNum)
        {
            if (num == a && (closedA || !closedNum))
                return true;
            if (num == b && (closedB || !closedNum))
                return true;
            if (a > b)
                Const.Swap(ref a, ref b);
            return num > a && num < b;
        }

        /// <summary>
        /// Performs InBetween on both Re and Im parts of the number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="closedARe"></param>
        /// <param name="closedAIm"></param>
        /// <param name="closedBRe"></param>
        /// <param name="closedBIm"></param>
        /// <param name="num"></param>
        /// <param name="closedRe"></param>
        /// <param name="closedIm"></param>
        /// <returns></returns>
        private static bool ComplexInBetween(Number a, Number b, bool closedARe, bool closedAIm, bool closedBRe,
            bool closedBIm, Number num, bool closedRe, bool closedIm)
            => InBetween(a.Re, b.Re, closedARe, closedBRe, num.Re, closedRe) &&
               InBetween(a.Im, b.Im, closedAIm, closedBIm, num.Im, closedIm);

        /// <summary>
        /// Determines whether interval or element of piece is in this
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public bool Contains(Piece piece)
        {
            // Gather all information
            var up = UpperBound();
            var low = LowerBound();
            if (!MathS.CanBeEvaluated(up.Item1) || !MathS.CanBeEvaluated(low.Item1))
                return false;
            var up_l = piece.LowerBound();
            var low_l = piece.UpperBound();
            if (!MathS.CanBeEvaluated(up_l.Item1) || !MathS.CanBeEvaluated(low_l.Item1))
                return false;
            // If still running the function, all entities are numbers now

            // Evaluate them
            var num1 = up.Item1.Eval();
            var num2 = low.Item1.Eval();
            var num_up = up_l.Item1.Eval();
            var num_low = low_l.Item1.Eval();
            // // //

            return ComplexInBetween(num1, num2, up.Item2, up.Item3, 
                low.Item2, low.Item3, num_low, low_l.Item2,
                low_l.Item3) &&
                   ComplexInBetween(num1, num2, up.Item2, up.Item3,
                       low.Item2, low.Item3, num_up, up_l.Item2,
                       up_l.Item3);
        }

        protected Piece(PieceType type)
        {
            Type = type;
        }

        internal static Edge CopyEdge(Edge edge)
        => new Edge(edge.Item1.DeepCopy(), edge.Item2, edge.Item3);


        /// <summary>
        /// Creates an instance of a closed interval (use SetNode-functions to change it,
        /// see more in MathS.Sets.Interval() )
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static IntervalPiece Interval(Entity a, Entity b)
        {
            var interval = new IntervalPiece(a, b, true, true, true, true);
            return interval;
        }

        /// <summary>
        /// Creates an instance of an element
        /// See more in MathS.Sets.Element()
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        internal static OneElementPiece Element(Entity a)
        => new OneElementPiece(a);
    }

    public class OneElementPiece : Piece
    {
        private readonly Edge entity;

        internal OneElementPiece(Entity element) : base(PieceType.ENTITY)
        {
            entity = new Edge(element, true, true);
        }

        public override Edge UpperBound()
            => CopyEdge(entity);

        public override Edge LowerBound()
            => CopyEdge(entity);

        public override string ToString()
            => "{" + entity.Item1.ToString() + "}";
    }

    public class IntervalPiece : Piece
    {
        private Edge leftEdge;
        private Edge rightEdge;

        internal IntervalPiece(Entity left, Entity right, bool closedARe, bool closedAIm, bool closedBRe, bool closedBIm) : base(PieceType.INTERVAL)
        {
            leftEdge = new Edge(left, closedARe, closedAIm);
            rightEdge = new Edge(right, closedBRe, closedBIm);
        }

        public override Edge UpperBound()
            => CopyEdge(leftEdge);

        public override Edge LowerBound()
            => CopyEdge(rightEdge);

        /// <summary>
        /// Used for real intervals only.
        /// true: [
        /// false: (
        /// </summary>
        /// <param name="isClosed"></param>
        /// <returns></returns>
        public IntervalPiece SetLeftClosed(bool isClosed)
            => SetLeftClosed(isClosed, true);

        /// <summary>
        /// Used for real intervals only.
        /// true: ]
        /// false: )
        /// </summary>
        /// <param name="isClosed"></param>
        /// <returns></returns>
        public IntervalPiece SetRightClosed(bool isClosed)
            => SetRightClosed(isClosed, true);

        /// <summary>
        /// Used for any type of interval
        /// sets [ or ( for real and [ or ( for imaginary part of the number
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        /// <returns></returns>
        public IntervalPiece SetLeftClosed(bool Re, bool Im)
        {
            leftEdge = new Edge(leftEdge.Item1, Re, Im);
            return this;
        }

        /// <summary>
        /// Used for any type of interval
        /// sets ] or ) for real and ] or ) for imaginary part of the number
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        /// <returns></returns>
        public IntervalPiece SetRightClosed(bool Re, bool Im)
        {
            leftEdge = new Edge(leftEdge.Item1, Re, Im);
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (leftEdge.Item3)
                sb.Append("[");
            else
                sb.Append("(");
            if (leftEdge.Item2)
                sb.Append("[");
            else
                sb.Append("(");
            sb.Append(leftEdge.Item1.ToString());
            sb.Append("; ");
            sb.Append(rightEdge.Item1.ToString());
            if (leftEdge.Item2)
                sb.Append("]");
            else
                sb.Append(")");
            if (leftEdge.Item3)
                sb.Append("]");
            else
                sb.Append(")");
            return sb.ToString();
        }
    }
}
