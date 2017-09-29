using System;
using System.Linq.Expressions;
using System.ComponentModel;

namespace OICExplorer.Extensions
{
    static class INotifyPropertyChangedExtension
    {
        public static void Invoke(this PropertyChangedEventHandler owner, object sender, Expression<Func<object>> expr)
        {
            var body = expr.Body as MemberExpression;
            if(body == null)
            {
                var uBody = expr.Body as UnaryExpression;
                body = uBody.Operand as MemberExpression;
            }

            owner.Invoke(sender, new PropertyChangedEventArgs(body.Member.Name));
        }
    }
}
