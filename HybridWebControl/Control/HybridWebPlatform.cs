using System;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Serialization;
using System.Reflection;
using System.Linq;

namespace HybridWebControl
{
	public class HybridWebPlatform<T> : HybridWebView where T : new()
	{
		public HybridWebPlatform(IJsonSerializer jsonSerializer) : base(jsonSerializer)
		{
			JsBridge = new T();
			GenerateFunctionsForType();
		}

		public HybridWebPlatform() : this((Resolver.IsSet ? Resolver.Resolve<IJsonSerializer>() : null)
				?? DependencyService.Get<IJsonSerializer>() ?? new SystemJsonSerializer())
		{
		}

		public T JsBridge
		{
			get;
			private set;
		}

		private void GenerateFunctionsForType()
		{
			var info = typeof(T);

			var fields = info.GetRuntimeFields();

			foreach (var item in fields)
			{
				ExamineField(item);
			}

			var events = info.GetRuntimeEvents();

			foreach (var item in events)
			{

			}
		}

		private void ExamineField(FieldInfo field)
		{
			foreach (var attribute in field.CustomAttributes)
			{
				if (attribute.AttributeType == typeof(JsFunctionInjectAttribute))
				{
					var convertedAttribute = field.GetCustomAttribute<JsFunctionInjectAttribute>();

					SubscribeNewFunctionCall(field, convertedAttribute);

					break;
				}

				if (attribute.AttributeType == typeof(JsFunctionCallAttribute))
				{
					var convertedAttribute = field.GetCustomAttribute<JsFunctionCallAttribute>();

					SubscribeExistingFunctionCall(field, convertedAttribute);

					break;
				}
			}
		}

		private void ExamineEvent(EventInfo eventInfo)
		{
			foreach (var attribute in eventInfo.CustomAttributes)
			{
				if (attribute.AttributeType == typeof(JsFunctionCallbackAttribute))
				{
					var convertedAttribute = eventInfo.GetCustomAttribute<JsFunctionCallbackAttribute>();

					SubscribeToEvent(eventInfo, convertedAttribute);

					break;
				}
			}
		}

		private void SubscribeExistingFunctionCall(FieldInfo field, JsFunctionCallAttribute attribute)
		{
			TypeInfo info = field.FieldType.GetTypeInfo();

			MethodInfo invoke = info.GetDeclaredMethod("Invoke");
			var p = invoke.GetParameters();

			if (p.Length != 1)
			{
				throw new Exception("Function call should be Action<string> type");
			}

			if (p[0].ParameterType != typeof(string))
			{
				throw new Exception("Function call should be Action<string> type");
			}

			if (invoke.ReturnType != typeof(void))
			{
				throw new Exception("Function call should be Action<string> type");
			}

			MethodInfo combineImpl = info.BaseType.GetTypeInfo().GetDeclaredMethod("CombineImpl");

			//combineImpl.Invoke(JsBridge, new Delegate[] { new Action<string>((arg) => ExecuteJavascriptFunction(attribute.JsFunctionName, arg)) });
		}

		private void SubscribeNewFunctionCall(FieldInfo field, JsFunctionInjectAttribute attribute)
		{
			//ExecuteJavascript($"function {attribute.JsFunctionName} (param1) {{ {attribute.JsFunctionBody} }}");

			SubscribeExistingFunctionCall(field, new JsFunctionCallAttribute(attribute.JsFunctionName));
		}

		private void SubscribeToEvent(EventInfo eventInfo, JsFunctionCallbackAttribute attribute)
		{
			RegisterCallback(attribute.JsCallbackName, (obj) => eventInfo.RaiseMethod.Invoke(JsBridge, new[] { obj }));
		}
	}
}
