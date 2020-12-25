using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Core
{
    public class ExpressionTree
    {
        public Expression Expression { get; set; }
        public IDictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> Value { get; set; }
        public IList<KeyValuePair<int, Side>> Keys { get; set; }
        public Type Type { get; set; }
        public int Depth { get; set; }
        public IEnumerable<(Type Type, string Name)> Parameters { get; set; }
        public IEnumerable<(Type Type, object Value)> Constants { get; set; }

        public ExpressionTree(Expression exp, Type type)
        {
            Expression = exp;
            Value = new Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>>();
            Keys = new List<KeyValuePair<int, Side>>();
            Type = type;
            Start(exp);
        }

        private void Start(Expression exp)
        {
            GenerateExpressionTree(exp);
            Depth = GetDepth();
            Parameters = GetParameters();
            Constants = GetConstants();
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
        private int GetDepth()
        {
            return Value.Keys.OrderByDescending(x => x.Key).First().Key;
        }

        private IEnumerable<(Type Type, string Name)> GetParameters()
        {
            var result = new List<(Type Type, string Name)>();

            foreach (var key in Keys)
            {
                if (Value[key].ContainsKey(Side.None))
                {
                    if (Value[key][Side.None].NodeType == ExpressionType.Parameter)
                    {
                        var param = Value[key][Side.None] as ParameterExpression;

                        result.Add((param.Type, param.Name));
                    }
                }
            }

            return result;
        }

        private IEnumerable<(Type Type, object Value)> GetConstants()
        {
            var result = new List<(Type Type, object Value)>();

            foreach (var key in Keys)
            {
                if (Value[key].ContainsKey(Side.None))
                {
                    if (Value[key][Side.None].NodeType == ExpressionType.Constant)
                    {
                        var param = Value[key][Side.None] as ConstantExpression;

                        result.Add((param.Type, param.Value));
                    }
                }
            }

            return result;
        }

        public static void Main(string[] args)
        {
            Expression<Func<int, int, int>> exp = (x, y) => x + y + 1 + 3;
            var expressionTree = new ExpressionTree(exp.Body, exp.Type);
        }
    }
    public enum Side
    {
        None = -1,
        Left = 0,
        Right = 1
    }
}
