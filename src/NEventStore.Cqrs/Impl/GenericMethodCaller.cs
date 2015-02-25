using System.Reflection;

namespace NEventStore.Cqrs.Impl
{
    public class GenericMethodCaller
    {
        private readonly object obj;
        private readonly MethodInfo openMethod;
        public GenericMethodCaller(object obj, string methodName)
        {
            this.obj = obj;
            openMethod = obj.GetType().GetMethod(methodName);
        }

        public void Call(object arg)
        {
            // openMethod.MakeGenericMethod(arg.GetType()).Invoke(obj, new[] { arg });
            var genericMethod = openMethod.MakeGenericMethod(arg.GetType());
            var method = DelegateFactory.CreateVoid(genericMethod);
            method(obj, new[] { arg });
        }
    }
}