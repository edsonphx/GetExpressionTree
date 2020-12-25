using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core
{
    public class GetExpressionTree
    {
        private static Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> _expressionTree = new Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>>();
        private static List<KeyValuePair<int, Side>> _keys = new List<KeyValuePair<int, Side>>();
        public static void Main(string[] args)
        {
            new GetExpressionTree().Start();
        }
        public void Start()
        {
            Expression<Func<int, int, int>> exp = (x, y) => 8 + x + y + 1 + 3 + 1 + 3;
            GenerateExpressionTree(exp.Body, 0);
            
            _expressionTree.CountNumberOfVars(_keys);
        }

        private void GenerateExpressionTree(Expression exp, int depth = 0, Side side = Side.None)
        {
            var key = CreateKey(depth, side);
            _keys.Add(key);

            if (exp.NodeType == ExpressionType.Constant || exp.NodeType == ExpressionType.Parameter)
            {
                _expressionTree[key] = CreateValues(exp);
            }
            else
            {
                var bExp = exp as BinaryExpression;

                var value = CreateValues(bExp);

                _expressionTree[key] = value;

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

            if(bExp != null)
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
    }
    public enum Side
    {
        None = -1,
        Left = 0,
        Right = 1
    }
    public static class ExtensionMethods
    {
        public static int CountNumberOfVars(this Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> dict, IEnumerable<KeyValuePair<int, Side>> keys)
        {
            int numberOfVars = 0;

            foreach (var key in keys)
            {
                if (dict[key].ContainsKey(Side.None))
                {
                    numberOfVars += 1;
                }
            }

            return numberOfVars;
        }

        public static int CountNumberOfParameters(this Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> dict, IEnumerable<KeyValuePair<int, Side>> keys)
        {
            int numberOfParameters = 0;

            foreach (var key in keys)
            {
                if (dict[key].ContainsKey(Side.None))
                {
                    if (dict[key][Side.None].NodeType == ExpressionType.Parameter)
                    {

                        numberOfParameters += 1;
                    }
                }
            }

            return numberOfParameters;
        }

        public static int CountNumberOfConstants(this Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> dict, IEnumerable<KeyValuePair<int, Side>> keys)
        {
            int numberOfParameters = 0;

            foreach (var key in keys)
            {
                if (dict[key].ContainsKey(Side.None))
                {
                    if (dict[key][Side.None].NodeType == ExpressionType.Constant)
                    {

                        numberOfParameters += 1;
                    }
                }
            }

            return numberOfParameters;
        }
    }
}
