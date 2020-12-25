using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core
{
    public class GetExpressionTree
    {
        private static Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>> _expressionTree = new Dictionary<KeyValuePair<int, Side>, Dictionary<Side, Expression>>();
        public static void Main(string[] args)
        {
            new GetExpressionTree().Start();
        }
        
        private void Start()
        {
            Expression<Func<int, int, int>> exp = (x, y) => (x + y + 1 + x + y + 1) * 10;
            GenerateExpressionTree(exp.Body, 0);
        }

        private void GenerateExpressionTree(Expression exp, int depth = 0, Side side = Side.None)
        {
            var key = CreateKey(depth, side);

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
}
