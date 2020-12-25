using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Core
{
    public class ExpressionTree
    {
        private Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> Value { get; set; }
        private List<KeyValuePair<int, Side>> Keys { get; set; }
        private int Depth { get; set; }

        public ExpressionTree(Expression exp)
        {
            Value = new Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>>();
            Keys = new List<KeyValuePair<int, Side>>();
            Start(exp);
        }

        private void Start(Expression exp)
        {
            GenerateExpressionTree(exp);
            Depth = Value.Keys.OrderByDescending(x => x.Key).First().Key;
        }
        private void GenerateExpressionTree(Expression exp, int depth = 0, Side side = Side.None)
        {
            var key = CreateKey(depth, side);
            Keys.Add(key);

            if (exp.NodeType == ExpressionType.Constant || exp.NodeType == ExpressionType.Parameter)
            {
                Value[key] = CreateValues(exp);
            }
            else
            {
                var bExp = exp as BinaryExpression;

                var value = CreateValues(bExp);

                Value[key] = value;

                depth += 1;
                GenerateExpressionTree(bExp.Left, depth, Side.Left);
                GenerateExpressionTree(bExp.Right, depth, Side.Right);
            }
        }

        private KeyValuePair<int, Side> CreateKey(int depth, Side side)
        {
            return new KeyValuePair<int, Side>(depth, side);
        }

        private Dictionary<Side, Expression> CreateValues(Expression exp)
        {
            var dict = new Dictionary<Side, Expression>();

            var bExp = exp as BinaryExpression;

            if (bExp != null)
            {
                dict[Side.Left] = bExp.Left;
                dict[Side.Right] = bExp.Right;
            }
            else
            {
                dict[Side.None] = exp;
            }

            return dict;
        }

        public int CountNumberOfVars()
        {
            int numberOfVars = 0;

            foreach (var key in Keys)
            {
                if (Value[key].ContainsKey(Side.None))
                {
                    numberOfVars += 1;
                }
            }

            return numberOfVars;
        }

        public int CountNumberOfParameters()
        {
            int numberOfParameters = 0;

            foreach (var key in Keys)
            {
                if (Value[key].ContainsKey(Side.None))
                {
                    if (Value[key][Side.None].NodeType == ExpressionType.Parameter)
                    {
                        numberOfParameters += 1;
                    }
                }
            }

            return numberOfParameters;
        }

        public int CountNumberOfConstants()
        {
            int numberOfParameters = 0;

            foreach (var key in Keys)
            {
                if (Value[key].ContainsKey(Side.None))
                {
                    if (Value[key][Side.None].NodeType == ExpressionType.Constant)
                    {
                        numberOfParameters += 1;
                    }
                }
            }

            return numberOfParameters;
        }

        public static void Main(string[] args)
        {
            Expression<Func<int, int, int>> exp = (x, y) => x + y + 1 + 3;
            var expressionTree = new ExpressionTree(exp.Body);
        }
    }
    public enum Side
    {
        None = -1,
        Left = 0,
        Right = 1
    }
}
